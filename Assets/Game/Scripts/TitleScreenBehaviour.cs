using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class TitleScreenBehaviour : MonoBehaviour {

    public Canvas canvas;
    public CanvasGroup text;
    public CanvasGroup background;

    public Text[] loadingText;

    public float fadeInTime;
    public float fadeToGameTime = 0.5f;

    private Camera cam;
    private bool loaded;
    private bool starting;

    void Awake() {
        cam = Camera.main;
    }

	// Use this for initialization
	void Start () {
        cam.tag = "Untagged";

        loaded = false;
        starting = false;
        StartCoroutine(LoadRoutine());
	}
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        } else if (loaded && !starting && (Input.GetButtonDown("ToggleFly") || Input.GetButtonDown("Submit"))) {
            StartCoroutine(BeginGameRoutine());
        }
	}

    private IEnumerator LoadRoutine() {
        AsyncOperation async = SceneManager.LoadSceneAsync("TestScene", LoadSceneMode.Additive);

        while (!async.isDone) {
            Debug.Log("Scene not yet loaded!");
            yield return null;
        }

        Debug.Log("Scene Loading complete");
        cam.enabled = false;

        SceneManager.SetActiveScene(SceneManager.GetSceneByName("TestScene"));
        SceneManager.MergeScenes(SceneManager.GetSceneByName("TitleScene"), SceneManager.GetSceneByName("TestScene"));

        Debug.Log("Transitioned to new scene");
        cam.gameObject.SetActive(false);

        FindObjectOfType<PlayerModeManager>().SetIntroMode(true);

        yield return new WaitForSeconds(0.5f);

        foreach (Text t in loadingText) {
            t.text = "Ready!";
        }

        loaded = true;
    }

    private IEnumerator BeginGameRoutine() {
        starting = true;

        for (float timer = 0f; timer < fadeInTime; timer += Time.deltaTime) {
            background.alpha = 1f - (timer / fadeInTime);
            yield return null;
        }

        background.alpha = 0f;

        FindObjectOfType<PlayerModeManager>().SetIntroMode(false);

        for (float timer = 0f; timer < fadeToGameTime; timer += Time.deltaTime) {
            text.alpha = 1f - (timer / fadeToGameTime);
            yield return null;
        }

        text.alpha = 0f;

        canvas.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }
}
