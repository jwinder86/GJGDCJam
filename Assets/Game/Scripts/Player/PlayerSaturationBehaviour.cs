using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;

public class PlayerSaturationBehaviour : MonoBehaviour {

    private ColorCorrectionCurves colorCurves;
    private IslandManager[] islands;

    public float minSaturation = 0.25f;
    public float maxSaturation = 0.75f;

	// Use this for initialization
	void Awake () {
        colorCurves = FindObjectOfType<ColorCorrectionCurves>();
        islands = FindObjectsOfType<IslandManager>();
	}
	
	// Update is called once per frame
	void Update () {
        float maxSat = 0f;
        IslandManager sel = null;
        for (int i = 0; i < islands.Length; i++) {
            if (islands[i].SaturationValue > maxSat) {
                maxSat = islands[i].SaturationValue;
                sel = islands[i];
            }
        }

        /*if (sel != null) {
            Debug.Log("Saturation from: " + sel.name);
        } else {
            Debug.Log("No islands provide saturation");
        }*/

        colorCurves.saturation = Mathf.Lerp(minSaturation, maxSaturation, maxSat);
        //Debug.Log("Cur Sat: " + colorCurves.saturation);
	}
}
