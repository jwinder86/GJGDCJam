using UnityEngine;
using System.Collections.Generic;

public class BoidManager : MonoBehaviour {

    public BoidBehaviour prefab;

    public int disabledUpdateFrames = 10;
    public int activeBoidMax = 50;
    private int startIndex;

    private HashSet<BoidBehaviour> activeBoids;
    private HashSet<BoidBehaviour> disabledBoids;
    private PlayerPhysicsBehaviour player;

    private List<BoidBehaviour> scratch;

    void Awake() {
        player = FindObjectOfType<PlayerPhysicsBehaviour>();
    }

    // Use this for initialization
    void Start() {
        activeBoids = new HashSet<BoidBehaviour>();
        disabledBoids = new HashSet<BoidBehaviour>();
        scratch = new List<BoidBehaviour>();

        startIndex = 0;
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
                boid.UpdatePosition(playerPosition, playerVelocity);
            }
        }

        // remove inactive boids
        activeBoids.ExceptWith(scratch);

        // update inactive boids {
        scratch.Clear();
        int i = startIndex;
        foreach (BoidBehaviour boid in disabledBoids) {
            if (i++ % disabledUpdateFrames == 0) {
                boid.UpdatePosition(playerPosition, playerVelocity);

                if (boid.State != BoidState.Disabled && ((activeBoids.Count + scratch.Count) < activeBoidMax)) {
                    scratch.Add(boid);
                    activeBoids.Add(boid);
                }
            }
        }
        startIndex = (startIndex + 1) % disabledUpdateFrames;

        // remove active boids
        disabledBoids.ExceptWith(scratch);
        
        if (Input.GetKey(KeyCode.B)) {
            CreateBoid(Vector3.zero, Vector3.up);
        }
    }

    public void CreateBoid(Vector3 position, Vector3 launchDirection, BoidTarget target = null) {
        if (activeBoids.Count >= activeBoidMax) {
            return;
        }

        BoidBehaviour boid = (BoidBehaviour) Instantiate(prefab, position, Quaternion.identity);
        boid.Launch(launchDirection);

        boid.target = target;

        activeBoids.Add(boid);
    }
}
