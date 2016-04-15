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
    private Transform latestTransform;

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


    public bool PlaySound(Transform transform, ref int index) {
        //Debug.Log("Sound requestion at " + position);
        if (audio.isPlaying || locked) {
            //Debug.Log("Sound is already playing: " + name);
            return false;
        }

        latestTransform = transform;

        selectIndex(ref index);

        StopAllCoroutines();
        StartCoroutine(PlaySoundRoutine());
        return true;
    }

    private IEnumerator PlaySoundRoutine() {
        //Debug.Log("Will play sound at end of frame");
        yield return new WaitForEndOfFrame();
        locked = true;

        if (randomDelay > 0f) {
            yield return new WaitForSeconds(Random.Range(0f, randomDelay));
        }

        AudioClip clip = clips[nextIndex];
        nextIndex = -1;

        //Debug.Log("Playing clip: " + clip.name);

        if (randomizePitch > 0f) {
            audio.pitch = 1f + Random.Range(-randomizePitch, randomizePitch);
        }

        transform.position = latestTransform.position;
        audio.volume = maxVolume;
        audio.clip = clip;
        audio.Play();
        
        if (burstSound != null) {
            audio.PlayOneShot(burstSound);
        }

        locked = false;

        if (limitTime) {
            for (float timer = 0f; timer < playTime; timer += Time.deltaTime) {
                transform.position = latestTransform.position;
                yield return null;
            }

            for (float timer = 0f; timer < fadeTime; timer += Time.deltaTime) {
                transform.position = latestTransform.position;
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
