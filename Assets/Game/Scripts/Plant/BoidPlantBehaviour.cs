using UnityEngine;
using System.Collections;

public class BoidPlantBehaviour : ScalePlantBehaviour {

    public BoidBehaviour boidPrefab;
    public int boidCount;
    public float spawnDelay;
    public float launchHeight = 2f;

    new private AudioSource audio;

    void Awake() {
        audio = GetComponent<AudioSource>();
    }

    protected override IEnumerator SpawnRoutine() {
        // do scaling
        yield return StartCoroutine(base.SpawnRoutine());

        for (int i = 0; i < boidCount; i++) {
            BoidBehaviour boid = (BoidBehaviour) Instantiate(boidPrefab, transform.position + transform.up * launchHeight, transform.rotation);
            boid.Launch(transform.up);
            boid.target = transform;

            if (audio != null) {
                audio.Play();
            }

            yield return new WaitForSeconds(spawnDelay);
        }
    }
}
