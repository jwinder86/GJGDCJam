using UnityEngine;
using System.Collections;

public class PlayerCameraBehaviour : MonoBehaviour {

    private const float X_COEF = 5f;
    private const float Y_COEF = 7f;

    public float cameraDistance = 10f;
    public float cameraMoveTime = 0.3f;
    public float cameraRadius = 0.25f;

    public float introTime = 2f;
    public AnimationCurve introCurve;

    [Header("Control Settings")]
    public float manualCameraMoveTime = 0.05f;
    public float vertSensitivity = 1f;
    public float horzSensitivity = 1f;
    public float minAngle = -30f;
    public float maxAngle = 80f;

    public float controlResetTime = 1f;
    public float controlResetTimeFlight = 0.5f;
    public float controlResetLerpSpeed = 60f;
    public float controlIdleTime;
    public float manualCameraDist;

    [Header ("Hover Settings")]
    public float vertAngle = 30f;

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

    private float curHorzAngle;
    private float curVertAngle;

    private bool transitioning;
    private float transitionFlightRatio;

    private bool runningIntro;

    private bool manualControl;

    private int layerMask;

    void Awake () {
        camera = Camera.main.transform;
        mode = GetComponent<PlayerModeManager>();

        layerMask = 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("BoidSegment");
    }

    void Start() {
        camVelocity = Vector3.zero;

        manualControl = false;
        curVertAngle = 0f;
        curHorzAngle = 0f;

        transitioning = false;
        transitionFlightRatio = 0f;

        shakeTimer = 0f;
        heavyShakeTimer = 0f;
        shakeOffset = Vector3.zero;

        runningIntro = false;
    }
	
	// Update is called once per frame
	void LateUpdate () {
        if (runningIntro) {
            return;
        }

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

        float moveTime = (manualControl) ? manualCameraMoveTime : cameraMoveTime;
        camera.position = Vector3.SmoothDamp(camera.position - shakeOffset, cameraPos, ref camVelocity, moveTime);
        camera.LookAt(targetPos);

        if (!manualControl) {
            manualCameraDist = (camera.position - targetPos).magnitude;
        }

        if (heavyShakeTimer > 0f) {
            shakeOffset = heavyShakeMagnitude * heavyShakeTimer * (Mathf.Sin(Time.time * heavyShakeSpeed * Y_COEF) * camera.up + Mathf.Cos(Time.time * heavyShakeSpeed * X_COEF) * camera.right);
        } else if (shakeTimer > 0f) {
            shakeOffset = shakeMagnitude * shakeTimer * (Mathf.Sin(Time.time * shakeSpeed * Y_COEF) * camera.up + Mathf.Cos(Time.time * shakeSpeed * X_COEF) * camera.right) * Mathf.PerlinNoise(Time.time, 0f);
        } else {
            shakeOffset = Vector3.zero;
        }

        camera.position += shakeOffset;

        if (skyboxCamera != null) {
            skyboxCamera.position = Vector3.zero;
            skyboxCamera.rotation = camera.rotation;
        }
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
        if (!transitioning && (!manualControl && input.magnitude > 0f)) {
            manualControl = true;
            controlIdleTime = 0f;

            curHorzAngle = getHorzAngle(camera.transform.forward);
            curVertAngle = getVertAngle(camera.transform.forward);

            if (mode.Mode == PlayerModeManager.PlayerMode.Flight) {
                curHorzAngle -= getHorzAngle(transform.forward);
            }
        } else if (manualControl) {
            if (input.magnitude > 0f) {
                controlIdleTime = 0f;
            } else {
                controlIdleTime += Time.deltaTime;
            }
        }

        if (transitioning) {
            if (manualControl || mode.Mode == PlayerModeManager.PlayerMode.Stun) {
                return Vector3.Lerp(HoverModeGoal(), camera.position, transitionFlightRatio);
            } else {
                return Vector3.Lerp(HoverModeGoal(), FlightModeGoal(), transitionFlightRatio);
            }
        } else if (manualControl) {
            return ManualControlGoal(input);
        } else if (mode.Mode == PlayerModeManager.PlayerMode.Flight) {
            return FlightModeGoal();
        } else {
            return HoverModeGoal();
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

    private Vector3 HoverModeGoal() {
        Vector3 playerPosition = transform.position;
        Vector3 toPlayer = camera.position - playerPosition;
        toPlayer.y = 0f;
        float len = toPlayer.magnitude;

        toPlayer *= Mathf.Cos(vertAngle * Mathf.Deg2Rad);
        toPlayer.y = Mathf.Sin(vertAngle * Mathf.Deg2Rad) * len;

        toPlayer = toPlayer.normalized * cameraDistance;

        return playerPosition + toPlayer;
    }

    private Vector3 FlightModeGoal() {
        return transform.position + transform.TransformDirection(flightCameraOffset);
    }

    private Vector3 ManualControlGoal(Vector2 input) {
        curHorzAngle += input.x * horzSensitivity * Time.deltaTime;
        curVertAngle += input.y * vertSensitivity * Time.deltaTime;
        curVertAngle = Mathf.Clamp(curVertAngle, minAngle, maxAngle);

        Vector3 startDir;
        if (mode.Mode == PlayerModeManager.PlayerMode.Flight) {
            startDir = transform.forward;
            startDir.y = 0;
            startDir = startDir.normalized;
        } else {
            startDir = Vector3.forward;
        }

        Vector3 lookDir = Quaternion.Euler(0f, curHorzAngle, 0f) * startDir;
        lookDir = Quaternion.AngleAxis(curVertAngle, Vector3.Cross(Vector3.up, lookDir)) * lookDir;
        float lookDistance = manualCameraDist;

        // flight takes less time to reset
        float resetTime = (mode.Mode == PlayerModeManager.PlayerMode.Flight) ? controlResetTimeFlight : controlResetTime;

        if (controlIdleTime < resetTime) {
            return TargetGoal() - lookDir * lookDistance;
        } else {
            Vector3 defaultGoal = (mode.Mode == PlayerModeManager.PlayerMode.Flight) ? FlightModeGoal() : HoverModeGoal();
            Vector3 defaultDir = (transform.position - defaultGoal).normalized;
            float maxFrameAngle = controlResetLerpSpeed * Time.deltaTime;

            if (Vector3.Angle(lookDir, defaultDir) < maxFrameAngle) {
                Debug.Log("Within range of default camera placement, deactivating manual control");
                lookDir = defaultDir;
                manualControl = false;
            } else {
                lookDir = Vector3.RotateTowards(lookDir, defaultDir, maxFrameAngle * Mathf.Deg2Rad, 0f);
                curHorzAngle = getHorzAngle(lookDir) - getHorzAngle(startDir);
                curVertAngle = getVertAngle(lookDir);
                //Debug.Log("Moving towards default camera, " + maxFrameAngle + " " + lookDir + " goal: " + defaultDir);
            }

            return TargetGoal() - lookDir * lookDistance;
        }
    }

    public void Transition(float transitionTime, bool toFlight) {
        if (runningIntro) {
            return;
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

        manualControl = false;
        transitionFlightRatio = goal;
        transitioning = false;
    }

    public void RunIntroRoutine() {
        StopAllCoroutines();
        StartCoroutine(IntroRoutine());
    }

    private IEnumerator IntroRoutine() {
        runningIntro = true;

        Debug.Log("Running camera intro routine!");

        Vector3 startPosition = camera.position;
        Vector3 startLookTarget = camera.position + camera.forward * cameraDistance;

        for (float timer = 0f; timer < introTime; timer += Time.deltaTime) {
            float t = introCurve.Evaluate(timer / introTime);

            camera.position = Vector3.Lerp(startPosition, HoverModeGoal(), t);
            camera.LookAt(Vector3.Lerp(startLookTarget, transform.position, t));

            yield return null;
        }

        camera.position = HoverModeGoal();
        camera.LookAt(transform.position);

        Debug.Log("Completed camera intro routine!");

        runningIntro = false;
    }

    private float getHorzAngle(Vector3 direction) {
        return -Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg + 90f;
    }

    private float getVertAngle(Vector3 direction) {
        Vector3 flatDir = new Vector3(direction.x, 0f, direction.z);
        return Vector3.Angle(flatDir, direction) * -Mathf.Sign(direction.y);
    }
}
