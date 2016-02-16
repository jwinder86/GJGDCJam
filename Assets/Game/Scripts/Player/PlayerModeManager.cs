using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerModeManager : MonoBehaviour {

    public enum PlayerMode {
        Hover,
        Flight
    }

    public float transitionTime;
    public float transitionDrag;

    public float hardHitSpeed = 10f;
    public AudioClip hitHard;
    public ParticleBehaviour hitParticlePrefab;

    public float flightBurstSpeed = 20f;

    public float minHeight = -150f;
    public float maxHeight = 500f;

    private PlayerMode mode;
    public PlayerMode Mode {
        get { return mode; }
    }

    private Vector3 spawnPos;
    private Quaternion spawnAngle;

    private PlayerMovementBehaviour hoverMove;
    private PlayerFlightBehaviour flight;
    private PlayerCameraBehaviour cam;
    private float defaultDrag;
    private PlayerPhysicsBehaviour phys;

    private Animator animator;
    new private AudioSource audio;

    private bool paused;

    void Awake() {
        hoverMove = GetComponent<PlayerMovementBehaviour>();
        flight = GetComponent<PlayerFlightBehaviour>();
        cam = GetComponent<PlayerCameraBehaviour>();
        phys = GetComponentInChildren<PlayerPhysicsBehaviour>();

        animator = GetComponentInChildren<Animator>();
        audio = GetComponent<AudioSource>();
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

        spawnPos = transform.position;
        spawnAngle = transform.rotation;
        hoverMove.InitializeAngles(spawnAngle.eulerAngles.y);
    }
	
   // Update is called once per frame
	void Update () {
        if (paused) {
            return;
        }

        // manual toggle
	    if (Input.GetButtonDown("ToggleFly")) {
            setMode(mode == PlayerMode.Flight ? PlayerMode.Hover : PlayerMode.Flight);
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            StartCoroutine(ExitRoutine());
        }

        if (phys.position.y < minHeight || phys.position.y > maxHeight) {
            StartCoroutine(ResetRoutine());
        }

        // sync position and rotation with phys
        transform.position = phys.position;
        phys.rotation = transform.rotation;
	}

    private IEnumerator ResetRoutine() {
        paused = true;

        StartCoroutine(ChangeMode(PlayerMode.Hover));
        
        FadeBehaviour fade = FindObjectOfType<FadeBehaviour>();
        fade.FadeOut();

        yield return new WaitForSeconds(fade.fadeTime);

        phys.position = spawnPos;
        phys.rotation = spawnAngle;
        phys.velocity = Vector3.zero;

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

    public void HandleCollision(Collision collision) {
        if (mode == PlayerMode.Flight && collision.relativeVelocity.magnitude >= hardHitSpeed) {
            audio.PlayOneShot(hitHard);
            phys.velocity = -collision.relativeVelocity;

            if (collision.contacts.Length > 0) {
                ContactPoint p = collision.contacts[0];
                Instantiate(hitParticlePrefab, p.point, Quaternion.LookRotation(p.normal));
            }

            setMode(PlayerMode.Hover);
        }
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
        PlayerMode oldMode = mode;
        mode = newMode;

        hoverMove.enabled = false;
        flight.enabled = false;
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
            hoverMove.InitializeAngles(heading);
            hoverMove.enabled = true;
        }

        mode = newMode;
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
}
