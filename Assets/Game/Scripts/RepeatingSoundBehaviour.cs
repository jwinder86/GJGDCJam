using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class RepeatingSoundBehaviour : MonoBehaviour {

    public float playTime = 2f;
    public float fadeTime = 1f;

    new private AudioSource audio;
    private Transform latestTransform;

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

        locked = false;
    }

    public void SetPosition(float p) {
        audio.time = audio.clip.length * p;
    }

    public bool PlaySound(Transform transform) {
        if (locked) {
            return false;
        }

        latestTransform = transform;

        StopAllCoroutines();
        StartCoroutine(PlayRoutine());

        return true;
    }

    private IEnumerator PlayRoutine() {
        yield return new WaitForEndOfFrame();

        locked = true;

        transform.position = latestTransform.position;
        audio.volume = 1f;
        audio.UnPause();

        for (float timer = 0f; timer < playTime; timer += Time.deltaTime) {
            transform.position = latestTransform.position;
            yield return null;
        }
        
        for (float timer = 0f; timer < fadeTime; timer += Time.deltaTime) {
            transform.position = latestTransform.position;
            audio.volume = 1f - (timer / fadeTime);
            yield return null;
        }

        audio.volume = 0f;
        audio.Pause();
        locked = false;
    }
}
