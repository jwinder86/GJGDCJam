using UnityEngine;
using System.Collections.Generic;

public class MiniBoidManager : MonoBehaviour {

    public MiniBoidBehaviour prefab;

    public BoidTarget target;

    private HashSet<MiniBoidBehaviour> activeBoids;
    private PlayerPhysicsBehaviour player;

    private List<MiniBoidBehaviour> scratch;

    void Awake() {
        player = FindObjectOfType<PlayerPhysicsBehaviour>();
    }

	// Use this for initialization
	void Start () {
        activeBoids = new HashSet<MiniBoidBehaviour>();
        scratch = new List<MiniBoidBehaviour>();
	}
	
	// Update is called once per frame
	void Update () {
        scratch.Clear();

        Vector3 playerPosition = player.position;
        Vector3 playerVelocity = player.velocity;

        // update active boids
        foreach (MiniBoidBehaviour boid in activeBoids) {
            if (boid.State == MiniBoidState.Pooled) {
                scratch.Add(boid);
            } else {
                boid.UpdatePosition(playerPosition, playerVelocity);
            }
        }

        // remove inactive boids
        activeBoids.ExceptWith(scratch);

        /*if (Input.GetKey(KeyCode.B)) {
            CreateBoid(Vector3.zero, Vector3.up);
        }*/
	}

    public void CreateBoid(Vector3 position, Vector3 launchDirection) {
        MiniBoidBehaviour boid = PoolManager.Instance.GetPooledObject(prefab);
        boid.transform.position = position;
        boid.Activate();
        boid.Launch(launchDirection);

        boid.target = target;

        activeBoids.Add(boid);
    }
}
