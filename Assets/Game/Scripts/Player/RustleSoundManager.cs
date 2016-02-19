using UnityEngine;
using System.Collections.Generic;

public class RustleSoundManager : MonoBehaviour {

    public RustleSoundBehaviour prefab;
    public int soundCount;
    public float minDist;
    
    private List<RustleSoundBehaviour> sounds;
    private Vector3 lastPos;
    private int lastSoundIndex;
    private float minDistSqr;

    // Use this for initialization
    void Start () {
        minDistSqr = minDist * minDist;
        lastPos = Vector3.zero;
        lastSoundIndex = 0;

        sounds = new List<RustleSoundBehaviour>(soundCount);
        for (int i = 0; i < soundCount; i++) {
            RustleSoundBehaviour s = Instantiate(prefab);
            s.SetPosition((float)i / (float)soundCount);
            sounds.Add(s);
        }
	}

    public void PlaySound(Vector3 position) {
        if ((lastPos - position).sqrMagnitude >= minDistSqr) {
            nextSound().PlayRustle(position);
        }
    }

    private RustleSoundBehaviour nextSound() {
        lastSoundIndex = (lastSoundIndex + 1) % sounds.Count;
        return sounds[lastSoundIndex];
    }
}
