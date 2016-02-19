using UnityEngine;
using System.Collections;

[RequireComponent (typeof(ParticleSystem))]
public class PlayerLeavesBehaviour : MonoBehaviour {

    private ParticleSystem particles;

    public float emissionTime = 1f;
    private float defaultEmissionRate;

    private float emissionRate {
        get {
            return particles.emission.rate.constantMax;
        }
        set {
            ParticleSystem.EmissionModule em = particles.emission;
            ParticleSystem.MinMaxCurve rate = em.rate;
            rate.mode = ParticleSystemCurveMode.Constant;
            rate.constantMax = value;
            em.rate = rate;
        }
    }

	void Awake () {
        particles = GetComponent<ParticleSystem>();
	}

    void Start() {
        defaultEmissionRate = emissionRate;
        emissionRate = 0f;

        particles.loop = true;
        if (!particles.isPlaying) {
            particles.Play();
        }
    }

    public void EmitLeaves() {
        StopAllCoroutines();
        StartCoroutine(EmissionRoutine());
    }

    private IEnumerator EmissionRoutine() {
        emissionRate = defaultEmissionRate;

        for (float timer = 0f; timer < emissionTime; timer += Time.deltaTime) {
            emissionRate = Mathf.Lerp(defaultEmissionRate, 0f, timer / emissionTime);
            yield return null;
        }

        emissionRate = 0f;
    }
}
