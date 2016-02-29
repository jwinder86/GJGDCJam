using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class OneShotSoundBehaviour : MonoBehaviour {

    public AudioClip[] clips;
    public AudioClip burstSound;

    public bool limitTime = false;
    public float playTime;
    public float fadeTime;

    public float randomizePitch = 0f;
    public float randomDelay = 0f;

    new private AudioSource audio;
    private Vector3 latestPosition;

    private float maxVolume;
    private int nextIndex;

    private bool locked;

    void Awake() {
        audio = GetComponent<AudioSource>();
        audio.loop = false;
        audio.playOnAwake = false;
        maxVolume = audio.volume;

        if (audio.isPlaying) {
            audio.Stop();
        }

        nextIndex = -1;
        locked = false;
    }


    public bool PlaySound(Vector3 position, ref int index) {
        Debug.Log("Sound requestion at " + position);
        if (audio.isPlaying || locked) {
            Debug.Log("Sound is already playing: " + name);
            return false;
        }

        latestPosition = position;

        selectIndex(ref index);

        StopAllCoroutines();
        StartCoroutine(PlaySoundRoutine());
        return true;
    }

    private IEnumerator PlaySoundRoutine() {
        Debug.Log("Will play sound at end of frame");
        yield return new WaitForEndOfFrame();
        locked = true;
        transform.position = latestPosition;

        if (randomDelay > 0f) {
            yield return new WaitForSeconds(Random.Range(0f, randomDelay));
        }

        AudioClip clip = clips[nextIndex];
        nextIndex = -1;

        Debug.Log("Playing clip: " + clip.name);

        if (randomizePitch > 0f) {
            audio.pitch = 1f + Random.Range(-randomizePitch, randomizePitch);
        }

        audio.volume = maxVolume;
        audio.clip = clip;
        audio.Play();
        
        if (burstSound != null) {
            audio.PlayOneShot(burstSound);
        }

        locked = false;

        if (limitTime) {
            yield return new WaitForSeconds(playTime);

            for (float timer = 0f; timer < fadeTime; timer += Time.deltaTime) {
                audio.volume = Mathf.Lerp(maxVolume, 0f, timer / fadeTime);
                yield return null;
            }

            audio.volume = 0f;
            audio.Stop();
        }
    }

    private void selectIndex(ref int index) {
        if (nextIndex == -1) {
            nextIndex = Random.Range(0, clips.Length - 1);
            if (nextIndex >= index) {
                nextIndex++;
            }
        }

        index = nextIndex;
    }
}
