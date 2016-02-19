using UnityEngine;
using System.Collections;

[RequireComponent (typeof(AudioSource))]
public class RustleSoundBehaviour : MonoBehaviour {

    public float playTime = 2f;
    public float fadeTime = 1f;

    new private AudioSource audio;

	void Awake () {
        audio = GetComponent<AudioSource>();
	}

    void Start() {
        audio.loop = true;
        audio.volume = 0f;

        if (!audio.isPlaying) {
            audio.Play();
        }

        audio.Pause();
    }

    public void SetPosition(float p) {
        audio.time = audio.clip.length * p;
    }
	
	public void PlayRustle(Vector3 position) {
        transform.position = position;

        StopAllCoroutines();
        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine() {
        audio.volume = 1f;
        audio.UnPause();

        yield return new WaitForSeconds(playTime);

        for (float timer = 0f; timer < fadeTime; timer += Time.deltaTime) {
            audio.volume = 1f - (timer / fadeTime);
            yield return null;
        }

        audio.volume = 0f;
        audio.Pause();
    }
}
