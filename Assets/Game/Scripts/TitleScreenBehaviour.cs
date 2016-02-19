using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TitleScreenBehaviour : MonoBehaviour {

    private bool loading;

	// Use this for initialization
	void Start () {
        loading = false;
	}
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        } else if (!loading && (Input.GetButtonDown("ToggleFly") || Input.GetButtonDown("Submit"))) {
            StartCoroutine(LoadRoutine());
        }
	}

    private IEnumerator LoadRoutine() {
        loading = true;

        FadeBehaviour fade = FindObjectOfType<FadeBehaviour>();
        fade.FadeOut();

        yield return new WaitForSeconds(fade.fadeTime * 1.1f);

        SceneManager.LoadScene("TestScene", LoadSceneMode.Single);
    }
}
