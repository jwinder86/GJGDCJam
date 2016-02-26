using UnityEngine;
using System.Collections;
using System;

public class BoidParticleEffect : MonoBehaviour, BoidEffect {

    private ParticleSystem particles;

    void Awake() {
        particles = GetComponent<ParticleSystem>();
    }

    public float DisableEffect(bool immediate) {
        ParticleSystem.EmissionModule em = particles.emission;
        em.enabled = false;

        if (immediate) {
            particles.Stop();
            particles.Clear();

            return 0f;
        } else {
            StartCoroutine(CleanupRoutine());

            return particles.startLifetime;
        }
    }

    public void EnableEffect() {
        ParticleSystem.EmissionModule em = particles.emission;
        em.enabled = true;

        particles.Play();
    }

    private IEnumerator CleanupRoutine() {
        yield return new WaitForSeconds(particles.startLifetime);

        particles.Stop();
        particles.Clear();
    }
}
