using UnityEngine;
using System.Collections;

public class BoidBehaviour : MonoBehaviour {

    private int layerMask;

    public float sensorRadius = 20f;

    public float avoidanceDist = 8f;
    public float centeringWeight = 0.01f;
    public float steeringWeight = 0.125f;
    public float avoidanceWeight = 0.1f;
    public float targettingWeight = 0.005f;
    public float launchTime = 3f;

    public float maxSpeed = 10f;

    public float maxDist = 200f;

    public Transform target;

    private Vector3 velocity;
    private bool launching;

    private PlayerMovementBehaviour player;
    private Renderer[] renderers;
    private bool renEnabled;

    void Awake() {
        player = FindObjectOfType<PlayerMovementBehaviour>();
        renderers = GetComponentsInChildren<Renderer>();
    }

	// Use this for initialization
	void Start () {
        layerMask = 1 << LayerMask.NameToLayer("Boid") | 1 << LayerMask.NameToLayer("BoidObstacle");
        renEnabled = true;
	}
	
	// Update is called once per frame
	void Update () {
        float distToPlayer = (player.transform.position - transform.position).magnitude;

        if (!launching && !(distToPlayer > maxDist)) {
            // find flock
            Collider[] colliders = Physics.OverlapSphere(transform.position, sensorRadius, layerMask, QueryTriggerInteraction.Ignore);

            // get flock center and velocity
            Vector3 flockCenter = Vector3.zero;
            Vector3 flockVelocity = Vector3.zero;
            Vector3 avoidance = Vector3.zero;
            int count = 0;
            for (int i = 0; i < colliders.Length; i++) {
                BoidBehaviour boid = colliders[i].GetComponent<BoidBehaviour>();
                if (boid != null && boid != this) {
                    flockCenter += boid.transform.position;
                    flockVelocity += boid.velocity;
                    count++;

                    Vector3 toBoid = boid.transform.position - transform.position;
                    float toBoidDist = toBoid.magnitude;
                    if (toBoidDist < avoidanceDist) {
                        //avoidance -= toBoid.normalized * (avoidanceDist - toBoidDist);
                        avoidance -= toBoid;
                    }
                } else {
                    BoidObstacle obstacle = colliders[i].GetComponent<BoidObstacle>();
                    if (obstacle != null) {
                        Vector3 toObstacle = obstacle.transform.position - transform.position;
                        float toObstacleDist = toObstacle.magnitude;
                        if (toObstacleDist < (avoidanceDist + obstacle.radius)) {
                            avoidance -= toObstacle.normalized * (avoidanceDist + obstacle.radius - toObstacleDist);
                        }
                    }
                }
            }
            flockCenter /= count;
            flockVelocity /= count;

            // calc new velocity
            Vector3 newVelocity = Vector3.zero;

            if (count > 0) {
                // move toward center of flock
                newVelocity += (flockCenter - transform.position) * centeringWeight;

                // avoid other boids
                newVelocity += avoidance * avoidanceWeight;

                // match velocity
                newVelocity += flockVelocity * steeringWeight;
            }

            if (target != null) {
                newVelocity += (target.position - transform.position) * targettingWeight;
            }

            velocity += newVelocity;

            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

            // update position
            transform.position = transform.position + velocity * Time.deltaTime;
            transform.LookAt(transform.position + velocity, Vector3.up);
        }

        bool newRenEnabled = (distToPlayer <= maxDist);
        if (newRenEnabled != renEnabled) {
            for (int i = 0; i < renderers.Length; i++) {
                renderers[i].enabled = newRenEnabled;
            }
            renEnabled = newRenEnabled;
        }
	}

    public void Launch(Vector3 direction) {
        StopAllCoroutines();
        StartCoroutine(LaunchRoutine(direction));
    }

    private IEnumerator LaunchRoutine(Vector3 direction) {
        launching = true;
        Vector3 launchVel = direction.normalized * maxSpeed;

        for (float timer = 0f; timer < launchTime; timer += Time.deltaTime) {
            velocity = launchVel;
            transform.position += velocity * Time.deltaTime;
            transform.LookAt(transform.position + velocity, Vector3.up);

            yield return null;
        }

        launching = false;
    }
}
