using UnityEngine;
using System.Collections;

public class PlayerCameraBehaviour : MonoBehaviour {

    public float cameraDistance = 10f;
    public float cameraMoveTime = 0.3f;

    [Header ("Hover Settings")]
    public float vertAngle = 30f;
    public float minAngle = -30f;
    public float maxAngle = 80f;

    public float vertSensitivity = 1f;
    public float horzSensitivity = 1f;

    [Header ("Flight Settings")]
    public Vector3 flightCameraOffset = new Vector3(0f, 3f, -10f);
    public float rollPercentage = 0.3f;
    public float flightLookAhead = 3f;

    new private Transform camera;
    private PlayerFlightBehaviour flight;

    private Vector3 camVelocity;

    private float rollAngle, rollAngleVelocity;

    private float curVertAngle;

    private bool transitioning;
    private float transitionFlightRatio;

    void Awake () {
        camera = Camera.main.transform;
        flight = GetComponent<PlayerFlightBehaviour>();
	}

    void Start() {
        camVelocity = Vector3.zero;

        rollAngle = 0f;
        rollAngleVelocity = 0f;

        curVertAngle = vertAngle;

        transitioning = false;
        transitionFlightRatio = 0f;
    }
	
	// Update is called once per frame
	void LateUpdate () {
        Vector2 input = new Vector2(
            Input.GetAxis("LookHorizontal"),
            Input.GetAxis("LookVertical"));

        camera.position = Vector3.SmoothDamp(camera.position, PositionGoal(input), ref camVelocity, cameraMoveTime);
        camera.LookAt(TargetGoal());

        float goalRollAngle;
        if (flight.enabled) {
            goalRollAngle = flight.Roll * -rollPercentage;
        } else {
            goalRollAngle = 0f;
        }

        rollAngle = Mathf.SmoothDampAngle(rollAngle, goalRollAngle, ref rollAngleVelocity, 1f);
    }

    private Vector3 PositionGoal(Vector2 input) {
        if (transitioning) {
            return Vector3.Lerp(HoverModeGoal(input), FlightModeGoal(input), transitionFlightRatio);
        } else if (flight.enabled) {
            return FlightModeGoal(input);
        } else {
            return HoverModeGoal(input);
        }
    }

    private Vector3 TargetGoal() {
        if (transitioning) {
            return transform.position + transform.forward * flightLookAhead * transitionFlightRatio;
        } else if (flight.enabled) {
            return transform.position + transform.forward * flightLookAhead;
        } else {
            return transform.position;
        }
    }

    private Vector3 HoverModeGoal(Vector2 input) {
        // handle input
        curVertAngle += input.y * vertSensitivity * Time.deltaTime;
        curVertAngle = Mathf.Clamp(curVertAngle, minAngle, maxAngle);

        Quaternion rot = Quaternion.Euler(0f, input.x * horzSensitivity, 0f);

        Vector3 playerPosition = transform.position;
        Vector3 toPlayer = camera.position - playerPosition;
        toPlayer.y = 0f;
        float len = toPlayer.magnitude;

        toPlayer *= Mathf.Cos(curVertAngle * Mathf.Deg2Rad);
        toPlayer.y = Mathf.Sin(curVertAngle * Mathf.Deg2Rad) * len;

        toPlayer = toPlayer.normalized * cameraDistance;

        toPlayer = rot * toPlayer;

        return playerPosition + toPlayer;
    }

    private Vector3 FlightModeGoal(Vector2 input) {
        return transform.position + transform.TransformDirection(flightCameraOffset);
    }

    public void Transition(float transitionTime, bool toFlight) {
        StopAllCoroutines();
        StartCoroutine(TransitionRoutine(transitionTime, toFlight, transitioning));
    }

    private IEnumerator TransitionRoutine(float transitionTime, bool toFlight, bool interrupted) {
        transitioning = true;
        float start;
        if (interrupted) {
            start = transitionFlightRatio;
        } else {
            start = toFlight ? 0f : 1f;
        }
        float goal = toFlight ? 1f : 0f;

        transitionFlightRatio = start;

        for (float timer = 0f; timer < transitionTime; timer += Time.deltaTime) {
            transitionFlightRatio = Mathf.Lerp(start, goal, timer / transitionTime);
            yield return null;
        }

        transitionFlightRatio = goal;
        transitioning = false;
    }
}
