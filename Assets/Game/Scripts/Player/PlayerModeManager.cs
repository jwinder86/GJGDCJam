using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Rigidbody))]
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

    public float minHeight = -150f;
    public float maxHeight = 500f;

    private PlayerMode mode;
    public PlayerMode Mode {
        get { return mode; }
    }

    private Vector3 spawnPos;
    private Quaternion spawnAngle;

    //private PlayerHoverBehaviour hover;
    private PlayerMovementBehaviour hoverMove;
    private PlayerFlightBehaviour flight;
    new private Rigidbody rigidbody;
    private float defaultDrag;

    private Animator animator;
    new private AudioSource audio;

    private bool paused;

    void Awake() {
        //hover = GetComponent<PlayerHoverBehaviour>();
        hoverMove = GetComponent<PlayerMovementBehaviour>();
        flight = GetComponent<PlayerFlightBehaviour>();
        rigidbody = GetComponent<Rigidbody>();

        animator = GetComponentInChildren<Animator>();
        audio = GetComponent<AudioSource>();
    }

	// Use this for initialization
	void Start () {
        mode = PlayerMode.Hover;
        hoverMove.enabled = true;
        flight.enabled = false;
        animator.SetBool("Flying", false);

        defaultDrag = rigidbody.drag;

        spawnPos = transform.position;
        spawnAngle = transform.rotation;

        paused = false;
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

        if (transform.position.y < minHeight || transform.position.y > maxHeight) {
            StartCoroutine(ResetRoutine());
        }
	}

    private IEnumerator ResetRoutine() {
        paused = true;

        StartCoroutine(ChangeMode(PlayerMode.Hover));
        
        FadeBehaviour fade = FindObjectOfType<FadeBehaviour>();
        fade.FadeOut();

        yield return new WaitForSeconds(fade.fadeTime);

        transform.position = spawnPos;
        transform.rotation = spawnAngle;
        rigidbody.velocity = Vector3.zero;

        fade.FadeIn();

        yield return new WaitForSeconds(fade.fadeTime);

        paused = false;
    }

    private IEnumerator ExitRoutine() {
        paused = true;

        FadeBehaviour fade = FindObjectOfType<FadeBehaviour>();
        fade.FadeOut();

        yield return new WaitForSeconds(fade.fadeTime * 1.1f);

        Application.LoadLevel("TitleScene");
    }

    void OnCollisionEnter(Collision collision) {
        if (mode == PlayerMode.Flight && collision.relativeVelocity.magnitude >= hardHitSpeed) {
            audio.PlayOneShot(hitHard);
            rigidbody.velocity = -collision.relativeVelocity;

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

        float heading = (oldMode == PlayerMode.Flight ? flight.Heading : hoverMove.Heading);
        yield return StartCoroutine(AlignRotation(heading));

        if (newMode == PlayerMode.Flight) {
            flight.InitializeHeading(heading);
            flight.enabled = true;
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
            rigidbody.drag = Mathf.Lerp(defaultDrag, transitionDrag, timer / transitionTime);
            transform.rotation = Quaternion.Slerp(start, goal, timer / transitionTime);
            yield return null;
        }

        transform.rotation = goal;

        rigidbody.drag = defaultDrag;
    }
}
