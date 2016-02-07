using UnityEngine;
using System.Collections;

[RequireComponent (typeof(ParticleSystem))]
public class ParticleBehaviour : MonoBehaviour {

    private ParticleSystem particles;

    void Awake() {
        particles = GetComponent<ParticleSystem>();
    }

	// Use this for initialization
	void Start () {
        StartCoroutine(ParticleRoutine());
	}
	
	private IEnumerator ParticleRoutine() {
        yield return new WaitForSeconds(particles.duration);

        Destroy(gameObject);
    }
}
