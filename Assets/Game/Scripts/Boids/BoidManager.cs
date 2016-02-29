using UnityEngine;
using System.Collections.Generic;

public class BoidManager : MonoBehaviour {

    public BoidBehaviour boidPrefab;
    public BoidBehaviour bigBoidPrefab;
    public BoidTargetGroup bigBoidTargets;

    public float minBoidCallTime = 5f;
    public float maxBoidCallTime = 15f;

    public float minBigBoidCallTime = 20f;
    public float maxBigBoidCallTime = 40f;

    public int disabledUpdateFrames = 10;
    public int activeBoidMax = 50;
    private int startIndex;

    private HashSet<BoidBehaviour> activeBoids;
    private HashSet<BoidBehaviour> disabledBoids;
    private PlayerPhysicsBehaviour player;
    private SoundManager soundManager;

    private List<BoidBehaviour> scratch;

    void Awake() {
        player = FindObjectOfType<PlayerPhysicsBehaviour>();
        soundManager = FindObjectOfType<SoundManager>();

        // clean cache
        GetComponentCache.ClearCache();
    }

    // Use this for initialization
    void Start() {
        activeBoids = new HashSet<BoidBehaviour>();
        disabledBoids = new HashSet<BoidBehaviour>();
        scratch = new List<BoidBehaviour>();

        startIndex = 0;

        StartCoroutine(CallRoutine());
        StartCoroutine(BigBoidCallRoutine());
    }

    // Update is called once per frame
    void Update() {
        Vector3 playerPosition = player.position;
        Vector3 playerVelocity = player.velocity;

        // update active boids
        scratch.Clear();
        foreach (BoidBehaviour boid in activeBoids) {
            if (boid.State == BoidState.Disabled) {
                scratch.Add(boid);
                disabledBoids.Add(boid);
            } else {
                boid.UpdatePosition(playerPosition, playerVelocity, true);
            }
        }

        // remove inactive boids
        activeBoids.ExceptWith(scratch);

        // update inactive boids {
        scratch.Clear();
        int i = startIndex;
        foreach (BoidBehaviour boid in disabledBoids) {
            if (i++ % disabledUpdateFrames == 0) {
                boid.UpdatePosition(playerPosition, playerVelocity, false);

                if (boid.State != BoidState.Disabled && (boid.type == BoidType.Giant || (activeBoids.Count + scratch.Count) < activeBoidMax)) {
                    scratch.Add(boid);
                    activeBoids.Add(boid);
                }
            }
        }
        startIndex = (startIndex + 1) % disabledUpdateFrames;

        // remove active boids
        disabledBoids.ExceptWith(scratch);

        if (Input.GetKeyDown(KeyCode.B)) {
            CreateBigBoid(Vector3.zero, Vector3.up);
        }
    }

    private System.Collections.IEnumerator CallRoutine() {
        yield return new WaitForSeconds(Random.Range(minBoidCallTime, maxBoidCallTime));

        foreach (BoidBehaviour boid in activeBoids) {
            if (boid.type == BoidType.Bird) {
                boid.Call();
            }
        }

        StartCoroutine(CallRoutine());
    }

    private System.Collections.IEnumerator BigBoidCallRoutine() {
        yield return new WaitForSeconds(Random.Range(minBigBoidCallTime, maxBigBoidCallTime));

        foreach (BoidBehaviour boid in activeBoids) {
            if (boid.type == BoidType.Giant) {
                boid.Call();
            }
        }

        StartCoroutine(BigBoidCallRoutine());
    }

    public void CreateBoid(Vector3 position, Vector3 launchDirection, BoidTarget target = null) {
        BoidBehaviour boid = (BoidBehaviour)Instantiate(boidPrefab, position, Quaternion.identity);
        boid.SoundManager = soundManager;
        boid.Launch(launchDirection);

        boid.target = target;

        activeBoids.Add(boid);
    }

    public void CreateBigBoid(Vector3 position, Vector3 launchDirection) {
        BoidBehaviour boid = (BoidBehaviour)Instantiate(bigBoidPrefab, position, Quaternion.identity);
        boid.SoundManager = soundManager;
        boid.Launch(launchDirection);

        boid.target = bigBoidTargets.FirstTarget;

        activeBoids.Add(boid);

        FindObjectOfType<PlayerCameraBehaviour>().Shake(1f);
    }
}
