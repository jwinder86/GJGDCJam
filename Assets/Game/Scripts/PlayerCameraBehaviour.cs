using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Rigidbody))]
public class PlayerCameraBehaviour : MonoBehaviour {

    private const float ROT_EPSILON = 0.0001f;
    private const float ROT_SPEED_MIN = 0.1f;

    public float smoothTime = 2f;
    public float vertAngle = 30f;
    public float cameraDistance = 10f;
    public float minSpeedForRotate = 1f;

    new private Transform camera;
    new private Rigidbody rigidbody;

    private float horzAngle, horzAngleSpeed;
    
	void Awake () {
        camera = Camera.main.transform;
        rigidbody = GetComponent<Rigidbody>();
	}

    void Start() {
        horzAngle = 0f;
        horzAngleSpeed = 0f;

        camera.rotation = Quaternion.identity;
        camera.Rotate(camera.right, vertAngle, Space.World);
    }
	
	// Update is called once per frame
	void LateUpdate () {
        if (rigidbody.velocity.x * rigidbody.velocity.x + rigidbody.velocity.z * rigidbody.velocity.z > minSpeedForRotate * minSpeedForRotate) {
            // get horizontal goal angle
            float goalAngle = -Mathf.Atan2(rigidbody.velocity.z, rigidbody.velocity.x) * Mathf.Rad2Deg + 90f;

            // calc new angle
            horzAngle = Mathf.SmoothDampAngle(horzAngle, goalAngle, ref horzAngleSpeed, smoothTime, Mathf.Infinity, Time.deltaTime);
        } else if (Mathf.Abs(horzAngleSpeed) > ROT_SPEED_MIN) {
            // calc new angle
            float goalAngle = horzAngle - ROT_EPSILON * Mathf.Sign(horzAngleSpeed);
            horzAngle = Mathf.SmoothDampAngle(horzAngle, goalAngle, ref horzAngleSpeed, smoothTime, Mathf.Infinity, Time.deltaTime);
        }

        // move Camera
        camera.rotation = Quaternion.identity;
        camera.Rotate(Vector3.up, horzAngle);
        camera.Rotate(camera.right, vertAngle, Space.World);

        //Debug.Log("New cam angle: " + camera.eulerAngles.y + "Goal Angle: " + goalAngle);

        camera.position = transform.position - camera.forward * cameraDistance;
    }
}
