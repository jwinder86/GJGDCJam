using UnityEngine;
using System.Collections;

public interface BoidEffect {
    void EnableEffect();
    float DisableEffect(bool immediate);
}

public enum MiniBoidState {
    Launching,
    Flying,
    Landing,
    CleanUp,
    Disabled,
    Pooled
}

public class MiniBoidBehaviour : PoolableBehaviour {

    private int layerMask;

    public float sensorRadius = 20f;
    public float goalRadiusEpsilon = 1f;

    public float avoidanceDist = 8f;
    public float centeringWeight = 0.01f;
    public float steeringWeight = 0.125f;
    public float avoidanceWeight = 0.5f;
    public float targettingWeight = 0.005f;
    public float launchTime = 1f;
    public float rotSpeed = 180f;

    public float speed = 10f;
    public float speedVariance = 0.1f;

    public float maxDist = 200f;

    public float playerSensorRadius = 40f;
    public float playerSpeedMult = 1.25f;

    public BoidTarget target;

    private Vector3 velocity;

    private MiniBoidState state;
    public MiniBoidState State {
        get { return state; }
    }

    private BoidEffect[] effects;
    private bool effectsEnabled;

    private float noiseOffset;

    void Awake() {
        effects = GetComponentsInChildren<BoidEffect>();

        layerMask = 1 << LayerMask.NameToLayer("Boid");
    }

	// Use this for initialization
	void Start () {
        state = MiniBoidState.Disabled;

        effectsEnabled = true;
        EnableEffects(false);

        noiseOffset = Random.Range(0f, 10f);
	}
	
	public void UpdatePosition (Vector3 playerPosition, Vector3 playerVelocity) {
        Vector3 toPlayer = playerPosition - transform.position;
        float toPlayerDist = toPlayer.magnitude;

        // disable at set distance
        if (state == MiniBoidState.Flying && toPlayerDist > maxDist) {
            state = MiniBoidState.Disabled;
            EnableEffects(false);
        } else if (state == MiniBoidState.Disabled && toPlayerDist <= maxDist) {
            state = MiniBoidState.Flying;
            EnableEffects(true);
        }

        if (state == MiniBoidState.Flying || state == MiniBoidState.Landing) {
            FlyUpdate(playerPosition, playerVelocity);
        }
	}

    private float EnableEffects(bool enable, bool immediate = true) {
        float time = 0f;
        if (enable != effectsEnabled) {
            for (int i = 0; i < effects.Length; i++) {
                if (enable) {
                    effects[i].EnableEffect();
                } else {
                    time = Mathf.Max(time, effects[i].DisableEffect(immediate));
                }
            }
            effectsEnabled = enable;
        }

        return time;
    }

    private void FlyUpdate(Vector3 playerPosition, Vector3 playerVelocity) {
        Vector3 toPlayer = playerPosition - transform.position;
        float toPlayerDist = toPlayer.magnitude;

        // noisy speed
        float noise = Mathf.PerlinNoise(Time.time, noiseOffset) * 2f - 1f;
        float frameSpeed = speed * (1f + noise * speedVariance);

        Vector3 newVelocity = Vector3.zero;
        if (state == MiniBoidState.Flying) {
            // find flock
            Collider[] colliders = Physics.OverlapSphere(transform.position, sensorRadius, layerMask, QueryTriggerInteraction.Ignore);

            // get flock center and velocity
            Vector3 flockCenter = Vector3.zero;
            Vector3 flockVelocity = Vector3.zero;
            Vector3 avoidance = Vector3.zero;
            int count = 0;
            for (int i = 0; i < colliders.Length; i++) {
                MiniBoidBehaviour boid = colliders[i].GetComponent<MiniBoidBehaviour>();
                if (boid != null && boid != this) {
                    flockCenter += boid.transform.position;
                    flockVelocity += boid.velocity;
                    count++;

                    Vector3 toBoid = boid.transform.position - transform.position;
                    float toBoidDist = toBoid.magnitude;
                    if (toBoidDist < avoidanceDist) {
                        avoidance -= (toBoid / toBoidDist) * (avoidanceDist - toBoidDist);
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
                    avoidance -= (toPlayer / toPlayerDist) * (avoidanceDist - toPlayerDist);
                }
            }

            flockCenter /= count;
            flockVelocity /= count;

            // calc new velocity
            if (count > 0) {
                // move toward center of flock
                newVelocity += (flockCenter - transform.position) * centeringWeight;

                // avoid other boids
                newVelocity += avoidance * avoidanceWeight;

                // match velocity
                newVelocity += flockVelocity * steeringWeight;
            }
        }

        if (target != null) {
            Vector3 toTarget = target.transform.position - transform.position;
            newVelocity += toTarget * targettingWeight;

            // special case, we've reached the goal
            if (state != MiniBoidState.Landing && target.InRange(transform.position)) {
                StartCoroutine(LandingRoutine());
            }
        }

        newVelocity = newVelocity.normalized * frameSpeed;

        // find rotation
        float maxAngle = rotSpeed * Time.deltaTime;
        float goalAngle = Vector3.Angle(velocity, newVelocity);
        Quaternion goalRot = Quaternion.LookRotation(newVelocity, Vector3.up);
        if (goalAngle > maxAngle) {
            transform.rotation = Quaternion.Lerp(transform.rotation, goalRot, maxAngle / goalAngle);
        } else {
            transform.rotation = goalRot;
        }

        // update position
        velocity = transform.forward * frameSpeed;
        transform.position = transform.position + velocity * Time.deltaTime;
    }

    public void Launch(Vector3 direction) {
        StopAllCoroutines();
        StartCoroutine(LaunchRoutine(direction));
    }

    private IEnumerator LaunchRoutine(Vector3 direction) {
        state = MiniBoidState.Launching;
        Vector3 launchVel = direction.normalized * speed;

        for (float timer = 0f; timer < launchTime; timer += Time.deltaTime) {
            velocity = launchVel;

            transform.position += velocity * Time.deltaTime;
            transform.LookAt(transform.position + velocity, Vector3.up);

            yield return null;
        }

        state = MiniBoidState.Flying;
    }

    private IEnumerator CleanupRoutine() {
        state = MiniBoidState.CleanUp;

        float time = EnableEffects(false, false);
        yield return new WaitForSeconds(time);

        ReturnToPool();
    }

    private IEnumerator LandingRoutine() {
        state = MiniBoidState.Landing;

        float defaultSpeed = speed;
        float defaultRotSpeed = rotSpeed;

        while ((transform.position - target.transform.position).magnitude > goalRadiusEpsilon) {
            speed *= 1f - (0.1f * Time.deltaTime);
            rotSpeed *= 1f + (1f * Time.deltaTime);
            yield return null;
        }

        transform.position = target.transform.position;
        speed = defaultSpeed;
        rotSpeed = defaultRotSpeed;

        StartCoroutine(CleanupRoutine());
    }

    public override void Activate() {
        gameObject.SetActive(true);
        EnableEffects(true);
        state = MiniBoidState.Flying;
    }

    public override void Deactivate() {
        StopAllCoroutines();
        state = MiniBoidState.Pooled;
        EnableEffects(false);
        gameObject.SetActive(false);
    }
}
