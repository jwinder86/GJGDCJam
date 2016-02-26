using UnityEngine;
using System.Collections;

public class PlayerCameraBehaviour : MonoBehaviour {

    private const float X_COEF = 5f;
    private const float Y_COEF = 7f;

    public float cameraDistance = 10f;
    public float cameraMoveTime = 0.3f;
    public float cameraRadius = 0.25f;

    [Header ("Hover Settings")]
    public float vertAngle = 30f;
    public float minAngle = -30f;
    public float maxAngle = 80f;

    public float vertSensitivity = 1f;
    public float horzSensitivity = 1f;

    [Header ("Flight Settings")]
    public Vector3 flightCameraOffset = new Vector3(0f, 3f, -10f);
    public float flightLookAhead = 3f;

    [Header ("Shake Settings")]
    public float shakeMagnitude = 10f;
    public float shakeSpeed = 5f;

    public float heavyShakeMagnitude = 3f;
    public float heavyShakeSpeed = 0.5f;

    private float shakeTimer;
    private float heavyShakeTimer;
    private Vector3 shakeOffset;

    public Transform skyboxCamera;
    new private Transform camera;
    private PlayerModeManager mode;

    private Vector3 camVelocity;

    private float curVertAngle;

    private bool transitioning;
    private float transitionFlightRatio;

    private int layerMask;

    void Awake () {
        camera = Camera.main.transform;
        mode = GetComponent<PlayerModeManager>();

        layerMask = 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("Default");
    }

    void Start() {
        camVelocity = Vector3.zero;

        curVertAngle = vertAngle;

        transitioning = false;
        transitionFlightRatio = 0f;

        shakeTimer = 0f;
        heavyShakeTimer = 0f;
        shakeOffset = Vector3.zero;
    }
	
	// Update is called once per frame
	void LateUpdate () {
        // handle shaking
        if (Input.GetKey(KeyCode.Alpha1)) {
            Shake(1f);
        }

        if (Input.GetKey(KeyCode.Alpha2)) {
            HeavyShake(1f);
        }

        if (shakeTimer > 0f) {
            shakeTimer -= Time.deltaTime;
        }

        if (heavyShakeTimer > 0f) {
            heavyShakeTimer -= Time.deltaTime;
        }


        Vector2 input = new Vector2(
            Input.GetAxis("LookHorizontal"),
            Input.GetAxis("LookVertical"));

        // ignore shake offset when smoothing camera movement
        Vector3 targetPos = TargetGoal();
        Vector3 cameraPos = PositionGoal(input);

        // raycast to handle camera collisions
        // find distance before hitting something
        Vector3 toCamera = (cameraPos - targetPos);
        float goalDist = toCamera.magnitude;
        toCamera = toCamera.normalized;
        RaycastHit hit;
        if (Physics.SphereCast(targetPos, cameraRadius, toCamera, out hit, goalDist, layerMask)) {
            cameraPos = targetPos + toCamera * hit.distance;
        }

        camera.position = Vector3.SmoothDamp(camera.position - shakeOffset, cameraPos, ref camVelocity, cameraMoveTime);
        camera.LookAt(targetPos);

        if (heavyShakeTimer > 0f) {
            shakeOffset = heavyShakeMagnitude * heavyShakeTimer * (Mathf.Sin(Time.time * heavyShakeSpeed * Y_COEF) * camera.up + Mathf.Cos(Time.time * heavyShakeSpeed * X_COEF) * camera.right);
        } else if (shakeTimer > 0f) {
            shakeOffset = shakeMagnitude * shakeTimer * (Mathf.Sin(Time.time * shakeSpeed * Y_COEF) * camera.up + Mathf.Cos(Time.time * shakeSpeed * X_COEF) * camera.right) * Mathf.PerlinNoise(Time.time, 0f);
        } else {
            shakeOffset = Vector3.zero;
        }

        camera.position += shakeOffset;

        skyboxCamera.position = Vector3.zero;
        skyboxCamera.rotation = camera.rotation;
    }

    public void Shake(float time) {
        shakeTimer = Mathf.Max(shakeTimer, time);
    }

    public void HeavyShake(float time) {
        heavyShakeTimer = Mathf.Max(heavyShakeTimer, time);
    }

    public void Teleport() {
        camera.position = PositionGoal(Vector2.zero);
        camVelocity = Vector3.zero;
        camera.LookAt(TargetGoal());
        shakeTimer = 0f;
        heavyShakeTimer = 0f;
    }

    private Vector3 PositionGoal(Vector2 input) {
        if (transitioning) {
            if (mode.Mode == PlayerModeManager.PlayerMode.Stun) {
                return Vector3.Lerp(HoverModeGoal(input), camera.position, transitionFlightRatio);
            } else {
                return Vector3.Lerp(HoverModeGoal(input), FlightModeGoal(input), transitionFlightRatio);
            }
        } else if (mode.Mode == PlayerModeManager.PlayerMode.Flight) {
            return FlightModeGoal(input);
        } else {
            return HoverModeGoal(input);
        }
    }

    private Vector3 TargetGoal() {
        if (transitioning) {
            return transform.position + transform.forward * flightLookAhead * transitionFlightRatio;
        } else if (mode.Mode == PlayerModeManager.PlayerMode.Flight) {
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
        // reset camera angles
        if (!toFlight) {
            curVertAngle = vertAngle;
        }

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
