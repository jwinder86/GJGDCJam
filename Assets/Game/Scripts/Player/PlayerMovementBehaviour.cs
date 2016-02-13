using UnityEngine;
using System.Collections;

public class PlayerMovementBehaviour : MonoBehaviour {

    // components
    new private Transform camera;
    private PlayerPhysicsBehaviour phys;

    // settings
    public float moveForce = 3f;
    public float maxSpeed = 5f;
    public float directionalDamping = 0.2f;
    public float rotationTime = 0.5f;

    private Vector2 latestMoveDirection;

    private float yAngle, yAngleVel;
    private Quaternion tilt;

    public float Heading {
        get { return yAngle; }
    }

    public void InitializeAngles(float heading) {
        yAngle = heading;
        yAngleVel = 0f;
        tilt = Quaternion.identity;
    }

    // Use this for initialization
    void Awake() {
        phys = GetComponentInChildren<PlayerPhysicsBehaviour>();
        camera = Camera.main.transform;
    }

    void Start() {
        yAngle = 0f;
        yAngleVel = 0;
        tilt = Quaternion.identity;
    }

    // Update is called once per frame
    void Update() {
        Vector2 camForward = new Vector2(camera.forward.x, camera.forward.z);
        camForward.Normalize();
        Vector2 camRight = new Vector2(camera.right.x, camera.right.z);
        camRight.Normalize();

        // move character
        latestMoveDirection = Input.GetAxis("Horizontal") * camRight +
            Input.GetAxis("Vertical") * camForward;

        latestMoveDirection = Vector2.ClampMagnitude(latestMoveDirection, 1f);

        // rotate towards move direction
        float goalAngle;
        if (latestMoveDirection.sqrMagnitude > 0f) {
            goalAngle = 90f - Mathf.Atan2(latestMoveDirection.y, latestMoveDirection.x) * Mathf.Rad2Deg;
        } else if (Mathf.Abs(yAngleVel) > Helpers.rotationMinSpeed) {
            goalAngle = yAngle - Helpers.rotationEpsilon * Mathf.Sign(yAngleVel);
        } else {
            goalAngle = yAngle;
            yAngleVel = 0f;
        }

        yAngle = Mathf.SmoothDampAngle(yAngle, goalAngle, ref yAngleVel, rotationTime);

        // tilt towards force direction
        Quaternion newGoal;
        if (latestMoveDirection.sqrMagnitude > 0f) {
            Vector3 left = new Vector3(-latestMoveDirection.y, 0f, latestMoveDirection.x);
            left.Normalize();
            newGoal = Quaternion.AngleAxis(-30f, left);
        } else {
            newGoal = Quaternion.identity;
        }

        tilt = Quaternion.Lerp(tilt, newGoal, Time.deltaTime / rotationTime);

        transform.rotation = Quaternion.identity;
        transform.Rotate(Vector3.up, yAngle, Space.World);
        transform.rotation = tilt * transform.rotation;
    }

    void FixedUpdate() {
        Move(latestMoveDirection);
    }

    // handle actually moving player
    private void Move(Vector3 moveDirection) {
        Vector3 velocity = phys.absoluteVelocity;
        Vector2 velocity2d = new Vector2(velocity.x, velocity.z);
        if (moveDirection.sqrMagnitude > 0f) {
            Vector2 moveDirectionNormalized = moveDirection.normalized;
            Vector2 left = new Vector2(-moveDirectionNormalized.y, moveDirectionNormalized.x);

            // dampen non-directional speed
            float leftSpeed = Vector2.Dot(velocity2d, left);
            Vector2 dampenForce = leftSpeed * leftSpeed * Mathf.Sign(leftSpeed) * directionalDamping * left;

            // find goal speed
            float curDirectionSpeed = Vector2.Dot(velocity2d, moveDirectionNormalized);
            float goalMaxSpeed = moveDirection.magnitude * maxSpeed;

            float mult;
            if (curDirectionSpeed >= goalMaxSpeed) {
                mult = 0f;
            } else if (curDirectionSpeed > 0f) {
                mult = 1f - curDirectionSpeed / goalMaxSpeed;
            } else {
                mult = 1f;
            }

            Vector2 force = moveForce * mult * moveDirectionNormalized;
            Vector2 total = force - dampenForce;

            phys.AddForce(new Vector3(total.x, 0f, total.y), ForceMode.Acceleration);

            // Debug.Log("Speed: " + rigidbody.velocity + " (" + curDirectionSpeed + " / " + goalMaxSpeed + ") force added: " + force + " dampening: " + dampenForce);

        } else {
            Vector2 dampenForce = -velocity2d.sqrMagnitude * velocity2d.normalized * directionalDamping;

            phys.AddForce(new Vector3(dampenForce.x, 0f, dampenForce.y), ForceMode.Acceleration);

            // Debug.Log("Speed: " + rigidbody.velocity + " dampening: " + dampenForce);
        }
    }
}
