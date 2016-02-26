using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum BoidState {
    Launching,
    Flying,
    Disabled
}

public class BoidBehaviour : MonoBehaviour {

    private int sensorLayerMask;
    private int collisionLayerMask;

    public float sensorRadius = 20f;

    public float avoidanceDist = 8f;
    public float centeringWeight = 0.01f;
    public float steeringWeight = 0.125f;
    public float avoidanceWeight = 0.1f;
    public float targettingWeight = 0.005f;
    public float launchTime = 3f;

    public float rotSpeed = 30f;
    private float roll, rollVelocity;
    public float maxRoll = 60f;
    public float rollCoef = 0.1f;
    public float rollTime = 0.2f;

    public float physAvoidanceMultiplier = 10f;
    public float boidRadius = 0.5f;
    public float rayCastDist = 10f;

    public float maxSpeed = 10f;
    public float speedVariance = 0.3f;
    private float noiseOffset;

    public float maxDist = 200f;
    public float maxDistFromTarget = 50f;

    public float playerSensorRadius = 40f;
    public float playerSpeedMult = 1.5f;

    public BoidTarget target;

    private Vector3 velocity;
    private BoidState state;
    public BoidState State {
        get { return state; }
    }

    private Renderer[] renderers;
    private bool renEnabled;

    private static Dictionary<BoidTarget, int> scratch;

    void Awake() {
        renderers = GetComponentsInChildren<Renderer>();

        sensorLayerMask = 1 << LayerMask.NameToLayer("Boid");
        collisionLayerMask = 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("Ground");

        if (scratch == null) {
            scratch = new Dictionary<BoidTarget, int>();
        }
    }

	// Use this for initialization
	void Start () {
        state = BoidState.Flying;
        renEnabled = true;

        noiseOffset = Random.Range(0f, 10f);

        roll = 0f;
        rollVelocity = 0f;
	}
	
	// Update is called once per frame
	public void UpdatePosition (Vector3 playerPosition, Vector3 playerVelocity) {
        Vector3 toPlayer = playerPosition - transform.position;
        float toPlayerDist = toPlayer.magnitude;

        // disable at set distance
        if (state == BoidState.Flying && toPlayerDist > maxDist) {
            state = BoidState.Disabled;
            EnableRenderers(false);

            // move within range of target
            if (target != null) {
                Vector3 toTarget = target.transform.position - transform.position;
                float distToTarget = toTarget.magnitude;
                if (distToTarget > maxDistFromTarget) {
                    transform.position = target.transform.position + (toTarget / distToTarget) * maxDistFromTarget * 0.5f;
                }
            }
        } else if (state == BoidState.Disabled && toPlayerDist <= maxDist) {
            state = BoidState.Flying;
            EnableRenderers(true);
        }

        if (state == BoidState.Flying) {
            FlyUpdate(playerPosition, playerVelocity);
        }
	}

    private void EnableRenderers(bool newRenEnabled) {
        if (newRenEnabled != renEnabled) {
            for (int i = 0; i < renderers.Length; i++) {
                renderers[i].enabled = newRenEnabled;
            }
            renEnabled = newRenEnabled;
        }
    }

    private void FlyUpdate(Vector3 playerPosition, Vector3 playerVelocity) {
        Vector3 toPlayer = playerPosition - transform.position;
        float toPlayerDist = toPlayer.magnitude;
        
        // modifications based on proximity to player
        float noise = Mathf.PerlinNoise(Time.time, noiseOffset) * 2f - 1f;
        float frameSpeed = maxSpeed * (1f + noise * speedVariance);
        bool targetPlayer = false;

        // find flock
        Collider[] colliders = Physics.OverlapSphere(transform.position, sensorRadius, sensorLayerMask, QueryTriggerInteraction.Ignore);

        // get flock center and velocity
        Vector3 flockCenter = Vector3.zero;
        Vector3 flockVelocity = Vector3.zero;
        Vector3 avoidance = Vector3.zero;
        int count = 0;

        scratch.Clear();
        if (target != null) {
            target = SelectGroupTarget(ref scratch, target, target);
        }

        for (int i = 0; i < colliders.Length; i++) {
            BoidBehaviour boid = colliders[i].GetComponent<BoidBehaviour>();
            if (boid != null && boid != this) {
                flockCenter += boid.transform.position;
                flockVelocity += boid.velocity;
                count++;

                Vector3 toBoid = boid.transform.position - transform.position;
                float toBoidDist = toBoid.magnitude;
                if (toBoidDist < avoidanceDist) {
                    avoidance -= (toBoid / toBoidDist) * (avoidanceDist - toBoidDist);
                }

                if (boid.target != null) {
                    target = SelectGroupTarget(ref scratch, target, boid.target);
                }

                continue;
            }
        }

        // special case: consider player a boid
        if (toPlayerDist < playerSensorRadius) {
            flockCenter += playerPosition;
            flockVelocity += playerVelocity;
            count++;

            if (toPlayerDist < avoidanceDist) {
                avoidance -= (toPlayer /  toPlayerDist) * (avoidanceDist - toPlayerDist);
            }

            // new max speed
            float playerSpeed = playerVelocity.magnitude * (1f + noise * speedVariance);
            frameSpeed = Mathf.Max(frameSpeed, Mathf.Lerp(playerSpeed, playerSpeed * playerSpeedMult,
                Mathf.Clamp01(Mathf.InverseLerp(playerSensorRadius, avoidanceDist, toPlayerDist))));

            targetPlayer = true;
        }

        // avoid phys collisions
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, boidRadius, transform.forward, out hit, rayCastDist, collisionLayerMask)) {
            Vector3 avoidDir = Vector3.ProjectOnPlane(hit.normal, transform.forward).normalized;
            Vector3 avoidForce =  Mathf.InverseLerp(rayCastDist, 0f, hit.distance) * avoidDir * physAvoidanceMultiplier;
            avoidance += avoidForce;

            Debug.DrawLine(hit.point, hit.point + avoidForce, Color.magenta);
        }

        // calc new velocity
        Vector3 newVelocity = Vector3.zero;

        // avoid other boids and obstacles
        newVelocity += avoidance * avoidanceWeight;

        if (count > 0) {
            flockCenter /= count;
            flockVelocity /= count;

            // move toward center of flock
            newVelocity += (flockCenter - transform.position) * centeringWeight;

            // match velocity
            newVelocity += flockVelocity * steeringWeight;
        }

        if (targetPlayer) {
            newVelocity += (playerPosition - transform.position) * targettingWeight;
        } else if (target != null) {
            if (target.InRange(transform.position)) {
                target = target.nextTarget;
            }
            newVelocity += (target.transform.position - transform.position) * targettingWeight;
        }

        // rotate towards new direction
        newVelocity = newVelocity.normalized * frameSpeed;
        float maxAngle = rotSpeed * Time.deltaTime;

        float goalAngle = Vector3.Angle(velocity, newVelocity);
        Quaternion newRotation;
        if (goalAngle > maxAngle) {
            newRotation = Quaternion.RotateTowards(Quaternion.LookRotation(velocity), Quaternion.LookRotation(newVelocity), maxAngle);
        } else {
            newRotation = Quaternion.LookRotation(newVelocity);
        }

        // find y rotation for roll calc
        float yRot = transform.rotation.eulerAngles.y - newRotation.eulerAngles.y;
        if (yRot < -180f) {
            yRot += 360f;
        } else if (yRot > 180f) {
            yRot -= 360f;
        }
        float rollGoal = Mathf.Clamp((yRot / Time.deltaTime) * rollCoef, -maxRoll, maxRoll);
        roll = Mathf.SmoothDampAngle(roll, rollGoal, ref rollVelocity, rollTime, rotSpeed);

        transform.rotation = newRotation;
        velocity = transform.forward * frameSpeed;
        transform.position += velocity * Time.deltaTime;

        // add roll
        Debug.DrawLine(transform.position, transform.position + transform.up * 2f, Color.green);
        transform.Rotate(0f, 0f, roll, Space.Self);
        Debug.DrawLine(transform.position, transform.position + transform.up * 2f, Color.green);
    }

    private BoidTarget SelectGroupTarget(ref Dictionary<BoidTarget, int> counts, BoidTarget curTarget, BoidTarget newTarget) {
        int curTargetCount;
        if (curTarget != null && counts.ContainsKey(curTarget)) {
            curTargetCount = counts[curTarget];
        } else {
            curTargetCount = 0;
        }

        int newTargetCount;
        if (counts.ContainsKey(newTarget)) {
            newTargetCount = counts[newTarget] + 1;
            counts[newTarget] = newTargetCount;
        } else {
            newTargetCount = 1;
            counts.Add(newTarget, newTargetCount);
        }

        if (newTargetCount > curTargetCount) {
            return newTarget;
        } else {
            return curTarget;
        }
    }

    public void Launch(Vector3 direction) {
        StopAllCoroutines();
        StartCoroutine(LaunchRoutine(direction));
    }

    private IEnumerator LaunchRoutine(Vector3 direction) {
        state = BoidState.Launching;
        Vector3 launchVel = direction.normalized * maxSpeed;

        for (float timer = 0f; timer < launchTime; timer += Time.deltaTime) {
            velocity = launchVel;
            transform.position += velocity * Time.deltaTime;
            transform.LookAt(transform.position + velocity, Vector3.up);

            yield return null;
        }

        state = BoidState.Flying;
    }
}
