using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerFlightBehaviour : MonoBehaviour {

    // minimum degrees velocity
    private const float MIN_PITCH_VEL = 0.01f;

    // offset to make smoothing work (negates SmoothDampAngle's anti-overshoot property)
    private const float EPSILON_VEL = 0.0001f;

    [Header("Pitch Movement")]
    public float maxPitchSpeed = 30f;
    public float deadPitchSpeed = 10f;
    public float pitchTime = 0.1f;
    public float maxPitchAngle = 80f;

    [Header("Roll/Heading Movement")]
    public float maxRollSpeed = 30f;
    public float deadRollSpeed = 10f;
    public float rollTime = 0.1f;
    public float maxRollAngle = 60f;

    public float rollHeadingMultiplier = 0.5f;

    [Header("Flight")]
    public float verticalCoef = 0.5f;
    public float horizontalCoef = 0.25f;
    public float dragCoef = 0.1f;
    public float thrust = 10f;

    [Header("Misc.")]
    public Vector3[] gizmoPositions;

    private float pitch, pitchVelocity;
    private float roll, rollVelocity;
    private float heading;

    new private Rigidbody rigidbody;

    public float Heading {
        get { return heading; }
    }

    public float Roll {
        get { return roll; }
    }

    public void InitializeHeading(float heading) {
        this.heading = heading;
        pitch = 0f;
        pitchVelocity = 0;
        roll = 0f;
        rollVelocity = 0f;
    }

    void Awake() {
        rigidbody = GetComponent<Rigidbody>();
    }

    // Use this for initialization
    void Start() {
        pitch = 0f;
        pitchVelocity = 0f;
        roll = 0f;
        rollVelocity = 0f;
    }

    // Update is called once per frame
    void Update() {
        Vector2 input = Vector2.ClampMagnitude(
            new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")),
            1f);

        // handle pitch
        float pitchSpeed;
        float goalPitch;
        if (input.y != 0f) {
            goalPitch = maxPitchAngle * Mathf.Sign(input.y);
            pitchSpeed = AngleSpeed(maxPitchSpeed, deadPitchSpeed, input.y);

        } else if (pitchVelocity < MIN_PITCH_VEL && pitchVelocity > -MIN_PITCH_VEL) { // under a certain velocity, just stop rotating
            goalPitch = pitch;
            pitchSpeed = 0f;
            pitchVelocity = 0f;

        } else { // otherwise, retain momentum
            goalPitch = pitch - Mathf.Sign(pitchVelocity) * EPSILON_VEL;
            pitchSpeed = deadPitchSpeed;
        }
        pitch = Mathf.SmoothDampAngle(pitch, goalPitch, ref pitchVelocity, pitchTime, pitchSpeed, Time.deltaTime);

        // handle roll
        float rollSpeed = AngleSpeed(maxRollSpeed, deadRollSpeed, input.x);
        roll = Mathf.SmoothDampAngle(roll, maxRollAngle * input.x, ref rollVelocity, rollTime, rollSpeed, Time.deltaTime);

        // addjust heading based on roll
        heading += roll * rollHeadingMultiplier * Time.deltaTime;

        // update rotation
        transform.rotation = Quaternion.identity;
        transform.Rotate(0f, heading, 0f);
        transform.Rotate(transform.right, pitch, Space.World);
        transform.Rotate(-transform.forward, roll, Space.World);
    }

    void FixedUpdate() {
        // calc wing forces
        Vector3 verForce = CalculateNormalDrag(-rigidbody.velocity, transform.up) * verticalCoef;
        Vector3 horForce = CalculateNormalDrag(-rigidbody.velocity, transform.right) * horizontalCoef;
        Vector3 dragForce = CalculateDragForce(rigidbody.velocity) * dragCoef;
        Vector3 thrustForce = thrust * transform.forward;// * (!Input.GetButton("Jump") ? 1f : 0f);

        // draw gizmos
        DrawGizmo(verForce, Color.green);
        DrawGizmo(horForce, Color.blue);
        DrawGizmo(dragForce, Color.red);
        DrawGizmo(thrustForce, Color.cyan);

        // apply force to body
        rigidbody.AddForce(verForce + horForce + dragForce + thrustForce, ForceMode.Force);
    }

    private float AngleSpeed(float max, float dead, float input) {
        return Mathf.Lerp(dead, max, Mathf.Abs(input));
    }

    private Vector3 CalculateNormalDrag(Vector3 velocity, Vector3 normal) {
        float speed = Vector3.Dot(velocity, normal);
        return speed * speed * Mathf.Sign(speed) * normal;
    }

    private Vector3 CalculateDragForce(Vector3 velocity) {
        return -velocity.sqrMagnitude * velocity.normalized;
    }

    private void DrawGizmo(Vector3 direction, Color color) {
        for (int i = 0; i < gizmoPositions.Length; i++) {
            Vector3 worldPos = transform.position + transform.TransformDirection(gizmoPositions[i]);
            Debug.DrawLine(worldPos, worldPos + direction, color);
        }
    }
}
