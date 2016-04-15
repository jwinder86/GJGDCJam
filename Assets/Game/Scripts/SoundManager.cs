using UnityEngine;
using System.Collections.Generic;

public enum SoundType {
    Rustle,
    ShortRustle,
    Creak,
    Whale,
    Bird,
    BirdSqueak
}

public class SoundManager : MonoBehaviour {

    public RepeatingSoundBehaviour rustlePrefab, longRustlePrefab, shortRustlePrefab;
    public OneShotSoundBehaviour creakPrefab, whalePrefab, birdPrefab, birdSqueakPrefab;

    public int rustleCount, shortRustleCount, creakCount, whaleCount, birdCount, birdSqueakCount;
    public float minDist;

    private RepeatingSoundManager rustleManager, longRustleManager, shortRustleManager;
    private OneShotSoundManager creakManager, whaleManager, birdManager, birdSqueakManager;

	// Use this for initialization
	void Start () {
        rustleManager = new RepeatingSoundManager(rustlePrefab, rustleCount, minDist, transform);
        longRustleManager = new RepeatingSoundManager(longRustlePrefab, creakCount, minDist, transform);
        shortRustleManager = new RepeatingSoundManager(shortRustlePrefab, shortRustleCount, minDist, transform);
        creakManager = new OneShotSoundManager(creakPrefab, creakCount, minDist, transform);
        whaleManager = new OneShotSoundManager(whalePrefab, whaleCount, 0f, transform);
        birdManager = new OneShotSoundManager(birdPrefab, birdCount, 0f, transform);
        birdSqueakManager = new OneShotSoundManager(birdSqueakPrefab, birdSqueakCount, 0f, transform);
    }
	
	public void PlaySound(SoundType type, Transform position) {
        switch (type) {
            case SoundType.Rustle:
                rustleManager.PlaySound(position);
                break;
            case SoundType.ShortRustle:
                shortRustleManager.PlaySound(position);
                break;
            case SoundType.Creak:
                creakManager.PlaySound(position);
                longRustleManager.PlaySound(position);
                break;
            case SoundType.Whale:
                whaleManager.PlaySound(position);
                break;
            case SoundType.Bird:
                birdManager.PlaySound(position);
                break;
            case SoundType.BirdSqueak:
                birdSqueakManager.PlaySound(position);
                break;
            default:
                Debug.LogError("Unexpected sound type: " + type);
                break;
        }
    }

    private class RepeatingSoundManager {
        private RepeatingSoundBehaviour[] sounds;
        private float minDistSqr;
        private Vector3 lastPosition;
        private int index;

        public RepeatingSoundManager(RepeatingSoundBehaviour prefab, int count, float minDist, Transform parent) {
            sounds = new RepeatingSoundBehaviour[count];
            for (int i = 0; i < count; i++) {
                sounds[i] = Instantiate(prefab);
                sounds[i].transform.parent = parent;
            }

            minDistSqr = minDist * minDist;
            lastPosition = Vector3.zero;
            index = 0;
        }

        public void PlaySound(Transform transform) {
            if ((transform.position - lastPosition).sqrMagnitude >= minDistSqr &&
                    NextSound().PlaySound(transform)) {
                lastPosition = transform.position;
            }
        }

        private RepeatingSoundBehaviour NextSound() {
            RepeatingSoundBehaviour sound = sounds[index];
            index = (index + 1) % sounds.Length;
            return sound;
        }
    }

    private class OneShotSoundManager {
        private OneShotSoundBehaviour[] sounds;
        private float minDistSqr;
        private Vector3 lastPosition;
        private int index;
        private int clipIndex;

        public OneShotSoundManager(OneShotSoundBehaviour prefab, int count, float minDist, Transform parent) {
            sounds = new OneShotSoundBehaviour[count];
            for (int i = 0; i < count; i++) {
                sounds[i] = Instantiate(prefab);
                sounds[i].transform.parent = parent;
            }

            minDistSqr = minDist * minDist;
            lastPosition = Vector3.zero;
            index = 0;
            clipIndex = 0;
        }

        public void PlaySound(Transform transform) {
            if ((transform.position - lastPosition).sqrMagnitude >= minDistSqr && 
                    NextSound().PlaySound(transform, ref clipIndex)) {
                lastPosition = transform.position;
            }
        }

        private OneShotSoundBehaviour NextSound() {
            OneShotSoundBehaviour sound = sounds[index];
            index = (index + 1) % sounds.Length;
            return sound;
        }
    }
}

