using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerModeManager : MonoBehaviour {

    public enum PlayerMode {
        Hover,
        Flight,
        Stun,
        Intro
    }

    public float transitionTime;
    public float transitionDrag;

    public float hardHitSpeed = 10f;
    public AudioClip hitHard;
    public AudioClip toFlight;
    public AudioClip toHover;
    public ParticleBehaviour hitParticlePrefab;

    public float flightBurstSpeed = 20f;
    public float stunTime = 2f;
    public float maxReboundSpeed = 10f;

    public float minHeight = -150f;
    public float maxHeight = 500f;
    public float maxRadialDist = 1000f;

    private PlayerMode mode;
    public PlayerMode Mode {
        get { return mode; }
    }

    private Vector3 spawnPos;
    private Quaternion spawnAngle;

    private PlayerMovementBehaviour hoverMove;
    private PlayerFlightBehaviour flight;
    private PlayerCameraBehaviour cam;
    private PlayerLeavesBehaviour leavesEffect;
    private SpeedEffectBehaviour speedEffect;
    private SoundManager soundManager;
    private PlayerPhysicsBehaviour phys;
    private float defaultDrag;

    private Animator animator;
    new private AudioSource audio;

    private bool paused;
    private bool rotFromPhys;

    void Awake() {
        hoverMove = GetComponent<PlayerMovementBehaviour>();
        flight = GetComponent<PlayerFlightBehaviour>();
        cam = GetComponent<PlayerCameraBehaviour>();
        leavesEffect = GetComponentInChildren<PlayerLeavesBehaviour>();
        speedEffect = GetComponentInChildren<SpeedEffectBehaviour>();
        phys = GetComponentInChildren<PlayerPhysicsBehaviour>();

        animator = GetComponentInChildren<Animator>();
        audio = GetComponent<AudioSource>();

        soundManager = FindObjectOfType<SoundManager>();
    }

	// Use this for initialization
	void Start () {
        mode = PlayerMode.Hover;
        hoverMove.enabled = true;
        flight.enabled = false;
        animator.SetBool("Flying", false);
        phys.WingColliderEnabled = false;

        defaultDrag = phys.drag;

        paused = false;
        rotFromPhys = false;

        spawnPos = transform.position;
        spawnAngle = transform.rotation;
        hoverMove.InitializeAngles(spawnAngle.eulerAngles.y);
    }

    public void SetIntroMode(bool introMode) {
        if (introMode) {
            paused = true;
            mode = PlayerMode.Intro;

            cam.enabled = false;
            leavesEffect.enabled = false;
            speedEffect.SpeedEffectActive = false;
            
            hoverMove.enabled = false;
            flight.enabled = false;
        } else {
            paused = false;

            cam.enabled = true;
            cam.RunIntroRoutine();
            leavesEffect.enabled = true;
            speedEffect.SpeedEffectActive = true;
            
            mode = PlayerMode.Stun;
            StartCoroutine(ChangeMode(PlayerMode.Hover));
        }
    }
	
    // Update is called once per frame
	void Update () {
        if (!paused) {
            // manual toggle
            if (Input.GetButtonDown("ToggleFly")) {
                audio.PlayOneShot(mode == PlayerMode.Flight ? toHover : toFlight);
                setMode(mode == PlayerMode.Flight ? PlayerMode.Hover : PlayerMode.Flight);
            }

            if (Input.GetKeyDown(KeyCode.Escape) || (Input.GetKey(KeyCode.JoystickButton6) && Input.GetKey(KeyCode.JoystickButton7))) {
                StartCoroutine(ExitRoutine());
            }

            if (phys.position.y < minHeight || phys.position.y > maxHeight || phys.position.magnitude > maxRadialDist) {
                StartCoroutine(ResetRoutine());
            }
        }

        // sync position and rotation with phys
        SyncPhys();
	}

    private IEnumerator ResetRoutine() {
        paused = true;

        StartCoroutine(ChangeMode(PlayerMode.Hover));
        
        FadeBehaviour fade = FindObjectOfType<FadeBehaviour>();
        fade.FadeOut();

        yield return new WaitForSeconds(fade.fadeTime);

        phys.Teleport(spawnPos, spawnAngle);
        transform.position = phys.position;
        cam.Teleport();

        fade.FadeIn();

        yield return new WaitForSeconds(fade.fadeTime);

        paused = false;
    }

    private IEnumerator ExitRoutine() {
        paused = true;

        FadeBehaviour fade = FindObjectOfType<FadeBehaviour>();
        fade.FadeOut();

        yield return new WaitForSeconds(fade.fadeTime * 1.1f);

        SceneManager.LoadScene("TitleScene", LoadSceneMode.Single);
    }

    public void HandleLeavesTrigger() {
        leavesEffect.EmitLeaves();
        soundManager.PlaySound(SoundType.Rustle, transform);
    }

    public void HandleCollision(Collision collision) {
        if (mode == PlayerMode.Flight && collision.relativeVelocity.magnitude >= hardHitSpeed) {
            audio.PlayOneShot(hitHard);
            cam.HeavyShake(0.5f);

            // remove constrainst
            rotFromPhys = true;
            phys.ConstrainRotation(false);
            phys.velocity = Vector3.ClampMagnitude(collision.relativeVelocity, maxReboundSpeed);

            if (collision.contacts.Length > 0) {
                ContactPoint p = collision.contacts[0];
                Instantiate(hitParticlePrefab, p.point, Quaternion.LookRotation(p.normal));

                phys.AddForceAtPosition(p.normal * maxReboundSpeed, p.point, ForceMode.VelocityChange);
            }

            StopAllCoroutines();
            StartCoroutine(StunRoutine());
        }
    }

    private IEnumerator StunRoutine() {
        paused = true;
        speedEffect.SpeedEffectActive = false;

        PlayerMode oldMode = mode;
        mode = PlayerMode.Stun;

        hoverMove.enabled = false;
        flight.enabled = false;
        animator.SetBool("Stun", true);
        animator.SetBool("Flying", false);
        phys.WingColliderEnabled = (false);
        cam.Transition(transitionTime, false);

        yield return new WaitForSeconds(stunTime);

        phys.ConstrainRotation(true);
        rotFromPhys = false;
        animator.SetBool("Stun", false);

        float heading = (oldMode == PlayerMode.Flight ? flight.Heading : hoverMove.Heading);
        yield return StartCoroutine(AlignRotation(heading));

        hoverMove.InitializeAngles(heading);
        hoverMove.enabled = true;

        speedEffect.SpeedEffectActive = true;
        paused = false;
    }

    private void setMode(PlayerMode newMode) {
        if (newMode == mode) {
            return;
        } else {
            StopAllCoroutines();
            StartCoroutine(ChangeMode(newMode));
        }
    }

    private IEnumerator ChangeMode(PlayerMode newMode) {
        Debug.Log("Switching from " + mode + " to " + newMode);

        PlayerMode oldMode = mode;
        mode = newMode;

        hoverMove.enabled = false;
        flight.enabled = false;
        animator.SetBool("Stun", false);
        animator.SetBool("Flying", newMode == PlayerMode.Flight);
        phys.WingColliderEnabled = (newMode == PlayerMode.Flight);
        cam.Transition(transitionTime, newMode == PlayerMode.Flight);

        float heading = (oldMode == PlayerMode.Flight ? flight.Heading : hoverMove.Heading);
        yield return StartCoroutine(AlignRotation(heading));

        if (newMode == PlayerMode.Flight) {
            flight.InitializeHeading(heading);
            flight.enabled = true;
            phys.velocity = transform.forward * flightBurstSpeed;
        } else {
            Debug.Log("Starting hover mode");
            hoverMove.InitializeAngles(heading);
            hoverMove.enabled = true;
        }
    }

    private IEnumerator AlignRotation(float heading) {
        Quaternion start = transform.rotation;
        Quaternion goal = Quaternion.AngleAxis(heading, Vector3.up);

        for (float timer = 0f; timer < transitionTime; timer += Time.deltaTime) {
            phys.drag = Mathf.Lerp(defaultDrag, transitionDrag, timer / transitionTime);
            transform.rotation = Quaternion.Slerp(start, goal, timer / transitionTime);
            yield return null;
        }

        transform.rotation = goal;

        phys.drag = defaultDrag;
    }

    private void SyncPhys() {
        transform.position = phys.position;

        if (rotFromPhys) {
            transform.rotation = phys.rotation;
        } else {
            phys.rotation = transform.rotation;
        }
    }
}
