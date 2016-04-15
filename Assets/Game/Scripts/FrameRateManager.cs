using UnityEngine;
using UnityStandardAssets.ImageEffects;
using System.Collections;

public class FrameRateManager : MonoBehaviour {

    public float smoothing = 0.1f;

    public bool display = false;

    private float b5, b15, b30, b45, b60, b90, bMax;
    private float smoothedFrameRate;
    private float curFrameRate;

    private CameraMotionBlur blur;
    private DepthOfField dof;
    private AudioSource musicPlayer;

    void Awake() {
        blur = FindObjectOfType<CameraMotionBlur>();
        dof = FindObjectOfType<DepthOfField>();
        musicPlayer = GameObject.Find("MusicPlayer").GetComponent<AudioSource>();
    }

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

        if (QualitySettings.GetQualityLevel() >= 6) {
            blur.enabled = true;
            dof.enabled = true;
        } else {
            blur.enabled = false;
            dof.enabled = false;
        }
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

        if (display) {
            if (Input.GetKeyDown(KeyCode.B) && blur != null) {
                blur.enabled = !blur.enabled;
            }

            if (Input.GetKeyDown(KeyCode.F) && dof != null) {
                dof.enabled = !dof.enabled;
            }

            if (Input.GetKeyDown(KeyCode.V)) {
                QualitySettings.vSyncCount = (QualitySettings.vSyncCount == 1) ? 0 : 1;
            }

            if (Input.GetKeyDown(KeyCode.M)) {
                if (musicPlayer.isPlaying) {
                    musicPlayer.Stop();
                } else {
                    musicPlayer.Play();
                }
            }

            if (Input.GetKeyDown(KeyCode.F4)) {
                QualitySettings.IncreaseLevel();
            } else if (Input.GetKeyDown(KeyCode.F3)) {
                QualitySettings.DecreaseLevel();
            }
        }
	}

    void OnGUI() {
        if (display) {
            GUI.Box(new Rect(5, 5, 210f, 360f), "Performance and Settings");
            GUI.Label(new Rect(10f, 30f, 200f, 20f), string.Format("FPS: {0}", Mathf.Round(curFrameRate)));
            GUI.Label(new Rect(10f, 50f, 200f, 20f), string.Format("Smooth FPS: {0}", Mathf.Round(smoothedFrameRate)));

            float sum = b5 + b15 + b30 + b45 + b60 + b90 + bMax;
            GUI.Label(new Rect(10f, 70f, 200f, 20f), string.Format("    < 5: {0} {1}%", timeString(b5), Mathf.Round(b5 / sum * 100f)));
            GUI.Label(new Rect(10f, 90f, 200f, 20f), string.Format(" 5 - 15: {0} {1}%", timeString(b15), Mathf.Round(b15 / sum * 100f)));
            GUI.Label(new Rect(10f, 110f, 200f, 20f), string.Format("15 - 30: {0} {1}%", timeString(b30), Mathf.Round(b30 / sum * 100f)));
            GUI.Label(new Rect(10f, 130f, 200f, 20f), string.Format("30 - 45: {0} {1}%", timeString(b45), Mathf.Round(b45 / sum * 100f)));
            GUI.Label(new Rect(10f, 150f, 200f, 20f), string.Format("45 - 60: {0} {1}%", timeString(b60), Mathf.Round(b60 / sum * 100f)));
            GUI.Label(new Rect(10f, 170f, 200f, 20f), string.Format("60 - 90: {0} {1}%", timeString(b90), Mathf.Round(b90 / sum * 100f)));
            GUI.Label(new Rect(10f, 190f, 200f, 20f), string.Format("   > 90: {0} {1}%", timeString(bMax), Mathf.Round(bMax / sum * 100f)));

            GUI.Label(new Rect(10f, 230f, 200f, 20f), string.Format("Quality (F3 - / F4 +): {0}", QualitySettings.GetQualityLevel()));
            GUI.Label(new Rect(10f, 250f, 200f, 20f), string.Format("Motion Blur (B): {0}", (blur.enabled ? "enabled" : "disabled")));
            GUI.Label(new Rect(10f, 270f, 200f, 20f), string.Format("Depth of Field (F): {0}", (dof.enabled ? "enabled" : "disabled")));
            GUI.Label(new Rect(10f, 290f, 200f, 20f), string.Format("VSync (V): {0}", QualitySettings.vSyncCount));

            GUI.Label(new Rect(10f, 330f, 200f, 20f), string.Format("Music (M): {0}", musicPlayer.isPlaying ? "enabled" : "disabled"));
        }
    }

    private string timeString(float time) {
        if (time > 180f) {
            return string.Format("{0}Min", Mathf.Floor(time / 60f));
        } else if (time > 5f) {
            return string.Format("{0}s", Mathf.Floor(time));
        } else {
            return string.Format("{0}ms", Mathf.Round(time * 1000f));
        }
    }
}
