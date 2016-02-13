using UnityEngine;
using System.Collections;

public class PlayerCameraBehaviour : MonoBehaviour {

    public float vertAngle = 30f;
    public float cameraDistance = 10f;

    public Vector3 flightCameraOffset = new Vector3(0f, 3f, -10f);
    public float rollPercentage = 0.3f;
    public float flightLookAhead = 3f;

    public float cameraMoveTime = 0.3f;
    
    new private Transform camera;
    private PlayerFlightBehaviour flight;

    private Vector3 camVelocity;

    private float rollAngle, rollAngleVelocity;

    private float camMovedLast = 0f;
    
	void Awake () {
        camera = Camera.main.transform;
        flight = GetComponent<PlayerFlightBehaviour>();
	}

    void Start() {
        camVelocity = Vector3.zero;

        rollAngle = 0f;
        rollAngleVelocity = 0f;
    }
	
	// Update is called once per frame
	void LateUpdate () {
        Vector3 goalPosition;

        if (flight.enabled) {
            goalPosition = FlightModeGoal();
        } else {
            goalPosition = HoverModeGoal();
        }

        camera.position = Vector3.SmoothDamp(camera.position, goalPosition, ref camVelocity, cameraMoveTime);

        if (flight.enabled) {
            camera.LookAt(transform.position + transform.forward * flightLookAhead);
        } else {
            camera.LookAt(transform.position);
        }

        float goalRollAngle;
        if (flight.enabled) {
            goalRollAngle = flight.Roll * -rollPercentage;
        } else {
            goalRollAngle = 0f;
        }

        rollAngle = Mathf.SmoothDampAngle(rollAngle, goalRollAngle, ref rollAngleVelocity, 1f);

        //camera.Rotate(transform.forward, rollAngle, Space.World);
    }

    private Vector3 HoverModeGoal() {
        Vector3 playerPosition = transform.position;
        Vector3 toPlayer = camera.position - playerPosition;
        toPlayer.y = 0f;
        float len = toPlayer.magnitude;

        toPlayer.y = Mathf.Sin(vertAngle * Mathf.Deg2Rad) * len;

        toPlayer = toPlayer.normalized * cameraDistance;

        return playerPosition + toPlayer;
    }

    private Vector3 FlightModeGoal() {
        return transform.position + transform.TransformDirection(flightCameraOffset);
    }
}
