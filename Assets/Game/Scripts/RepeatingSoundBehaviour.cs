using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class RepeatingSoundBehaviour : MonoBehaviour {

    public float playTime = 2f;
    public float fadeTime = 1f;

    new private AudioSource audio;
    private Vector3 latestPosition;

    private bool locked;

    void Awake() {
        audio = GetComponent<AudioSource>();
    }

    void Start() {
        audio.loop = true;
        audio.volume = 0f;

        if (!audio.isPlaying) {
            audio.Play();
        }

        // move to a random position in the track
        audio.time = Random.Range(0f, audio.clip.length);

        audio.Pause();

        latestPosition = Vector3.zero;
        locked = false;
    }

    public void SetPosition(float p) {
        audio.time = audio.clip.length * p;
    }

    public bool PlaySound(Vector3 position) {
        if (locked) {
            return false;
        }

        latestPosition = position;

        StopAllCoroutines();
        StartCoroutine(PlayRoutine());

        return true;
    }

    private IEnumerator PlayRoutine() {
        yield return new WaitForEndOfFrame();

        locked = true;
        transform.position = latestPosition;

        audio.volume = 1f;
        audio.UnPause();

        yield return new WaitForSeconds(playTime);
        
        for (float timer = 0f; timer < fadeTime; timer += Time.deltaTime) {
            audio.volume = 1f - (timer / fadeTime);
            yield return null;
        }

        audio.volume = 0f;
        audio.Pause();
        locked = false;
    }
}
