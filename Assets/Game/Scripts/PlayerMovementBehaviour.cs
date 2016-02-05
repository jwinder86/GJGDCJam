using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementBehaviour : MonoBehaviour {

    // components
    new private Rigidbody rigidbody;
    new private Transform camera;

    // settings
    public float moveForce = 3f;
    public float maxSpeed = 5f;
    public float directionalDamping = 0.2f;

    private Vector2 latestMoveDirection;

    // Use this for initialization
    void Awake() {
        rigidbody = GetComponent<Rigidbody>();
        camera = Camera.main.transform;
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
    }

    void FixedUpdate() {
        Move(latestMoveDirection);
    }

    // handle actually moving player
    private void Move(Vector3 moveDirection) {
        Vector2 velocity = new Vector2(rigidbody.velocity.x, rigidbody.velocity.z);
        if (moveDirection.sqrMagnitude > 0f) {
            Vector2 moveDirectionNormalized = moveDirection.normalized;
            Vector2 left = new Vector2(-moveDirectionNormalized.y, moveDirectionNormalized.x);

            // dampen non-directional speed
            float leftSpeed = Vector2.Dot(velocity, left);
            Vector2 dampenForce = leftSpeed * leftSpeed * Mathf.Sign(leftSpeed) * directionalDamping * left;

            // find goal speed
            float curDirectionSpeed = Vector2.Dot(velocity, moveDirectionNormalized);
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

            rigidbody.AddForce(new Vector3(total.x, 0f, total.y), ForceMode.Acceleration);

            // Debug.Log("Speed: " + rigidbody.velocity + " (" + curDirectionSpeed + " / " + goalMaxSpeed + ") force added: " + force + " dampening: " + dampenForce);

        } else {
            Vector2 dampenForce = -velocity.sqrMagnitude * velocity.normalized * directionalDamping;

            rigidbody.AddForce(new Vector3(dampenForce.x, 0f, dampenForce.y), ForceMode.Acceleration);

            // Debug.Log("Speed: " + rigidbody.velocity + " dampening: " + dampenForce);
        }
    }
}
