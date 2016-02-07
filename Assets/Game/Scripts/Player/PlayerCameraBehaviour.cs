using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Rigidbody))]
public class PlayerCameraBehaviour : MonoBehaviour {

    public float vertAngle = 30f;
    public float cameraDistance = 10f;
    public float minDist = 5f;
    public float maxDist = 15f;

    public Vector3 targetOffset = new Vector3(0f, 3f, 0f);
    public float rollPercentage = 0.3f;
    
    new private Transform camera;
    new private Rigidbody rigidbody;
    private PlayerFlightBehaviour flight;

    private Vector3 camVelocity;

    private float rollAngle, rollAngleVelocity;
    
	void Awake () {
        camera = Camera.main.transform;
        rigidbody = GetComponent<Rigidbody>();
        flight = GetComponent<PlayerFlightBehaviour>();
	}

    void Start() {
        camVelocity = Vector3.zero;

        rollAngle = 0f;
        rollAngleVelocity = 0f;
    }
	
	// Update is called once per frame
	void LateUpdate () {
        Vector3 playerPosition = transform.position; // transform.TransformPoint(targetOffset);
        Vector3 toPlayer = camera.position - playerPosition;
        toPlayer.y = 0f;
        float len = toPlayer.magnitude;

        toPlayer.y = Mathf.Sin(vertAngle * Mathf.Deg2Rad) * len;

        toPlayer = toPlayer.normalized * cameraDistance;

        Vector3 newCamPosition = Vector3.SmoothDamp(camera.position, playerPosition + toPlayer, ref camVelocity, 1f);

        // clamp distance
        Vector3 newToPlayer = newCamPosition - playerPosition;
        if (newToPlayer.magnitude > maxDist) {
            newToPlayer = newToPlayer.normalized * maxDist;
            newCamPosition = playerPosition + newToPlayer;
        } else if (newToPlayer.magnitude < minDist) {
            newToPlayer = newToPlayer.normalized * minDist;
            newCamPosition = playerPosition + newToPlayer;
        }

        camera.position = newCamPosition;

        camera.LookAt(playerPosition);

        /*float goalRollAngle;
        if (flight.enabled) {
            goalRollAngle = flight.Roll * -rollPercentage;
        } else {
            goalRollAngle = 0f;
        }

        rollAngle = Mathf.SmoothDampAngle(rollAngle, goalRollAngle, ref rollAngleVelocity, 1f);

        camera.Rotate(transform.forward, rollAngle, Space.World);*/

        // Flight mode
        /*camera.position = transform.position + transform.TransformDirection(new Vector3(0f, 3f, -10f));
        camera.LookAt(transform.position + transform.TransformDirection(targetOffset));

        float goalRollAngle;
        if (flight.enabled) {
            goalRollAngle = flight.Roll * -rollPercentage;
        } else {
            goalRollAngle = 0f;
        }

        rollAngle = Mathf.SmoothDampAngle(rollAngle, goalRollAngle, ref rollAngleVelocity, 1f);

        camera.Rotate(transform.forward, rollAngle, Space.World);*/
    }
}
