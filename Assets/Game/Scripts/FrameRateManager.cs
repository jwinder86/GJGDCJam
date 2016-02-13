using UnityEngine;
using System.Collections;

public class FrameRateManager : MonoBehaviour {

    public float smoothing = 0.1f;

    public bool display = false;

    private float b5, b15, b30, b45, b60, b90, bMax;
    private float smoothedFrameRate;
    private float curFrameRate;

	// Use this for initialization
	void Start () {
        b5 = 0f;
        b15 = 0f;
        b30 = 0f;
        b45 = 0f;
        b60 = 0f;
        b90 = 0f;
        bMax = 0f;

        curFrameRate = -1f;
        smoothedFrameRate = -1f;
	}
	
	// Update is called once per frame
	void Update () {
        curFrameRate = 1f / Time.deltaTime;

        if (smoothedFrameRate < 0) {
            smoothedFrameRate = curFrameRate;
        } else {
            float alpha = (smoothing * Time.deltaTime);
            smoothedFrameRate = curFrameRate * alpha + smoothedFrameRate * (1f - alpha);
        }

        if (curFrameRate <= 5f) {
            b5 += Time.deltaTime;
        } else if (curFrameRate <= 15f) {
            b15 += Time.deltaTime;
        } else if (curFrameRate <= 30f) {
            b30 += Time.deltaTime;
        } else if (curFrameRate <= 45f) {
            b45 += Time.deltaTime;
        } else if (curFrameRate <= 60f) {
            b60 += Time.deltaTime;
        } else if (curFrameRate <= 90f) {
            b90 += Time.deltaTime;
        } else {
            bMax += Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.I)) {
            display = !display;
        }
	}

    void OnGUI() {
        if (display) {
            GUI.Box(new Rect(5, 5, 310f, 220f), "Frame Rate Info");
            GUI.Label(new Rect(10f, 30f, 300f, 20f), "FPS: " + Mathf.Round(curFrameRate));
            GUI.Label(new Rect(10f, 50f, 300f, 20f), "Smooth FPS: " + Mathf.Round(smoothedFrameRate));

            float sum = b5 + b15 + b30 + b45 + b60 + b90 + bMax;
            GUI.Label(new Rect(10f, 70f, 300f, 20f), "    < 5: " + timeString(b5) + "  " + Mathf.Round(b5 / sum * 100f) + "%");
            GUI.Label(new Rect(10f, 90f, 300f, 20f), " 5 - 15: " + timeString(b15) + "  " + Mathf.Round(b15 / sum * 100f) + "%");
            GUI.Label(new Rect(10f, 110f, 300f, 20f), "15 - 30: " + timeString(b30) + "  " + Mathf.Round(b30 / sum * 100f) + "%");
            GUI.Label(new Rect(10f, 130f, 300f, 20f), "30 - 45: " + timeString(b45) + "  " + Mathf.Round(b45 / sum * 100f) + "%");
            GUI.Label(new Rect(10f, 150f, 300f, 20f), "45 - 60: " + timeString(b60) + "  " + Mathf.Round(b60 / sum * 100f) + "%");
            GUI.Label(new Rect(10f, 170f, 300f, 20f), "60 - 90: " + timeString(b90) + "  " + Mathf.Round(b90 / sum * 100f) + "%");
            GUI.Label(new Rect(10f, 190f, 300f, 20f), "   > 90: " + timeString(bMax) + "  " + Mathf.Round(bMax / sum * 100f) + "%");
        }
    }

    private string timeString(float time) {
        if (time > 180f) {
            return Mathf.Floor(time / 60f) + "min";
        } else if (time > 5f) {
            return Mathf.Floor(time) + "s";
        } else {
            return Mathf.Round(time * 1000f) + "ms";
        }
    }
}
