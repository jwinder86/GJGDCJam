using UnityEngine;
using System.Collections;

public class VinePlantBehaviour : ScalePlantBehaviour {

    public float yOffsetRandomization = 1f;

    public override void SetNormal(Vector3 normal) {
        transform.rotation *= Quaternion.FromToRotation(Vector3.up, Vector3.down);
    }

    protected override IEnumerator SpawnRoutine() {
        soundManager.PlaySound(SoundType.Rustle, transform);

        transform.position += new Vector3(0f, Random.Range(0f, yOffsetRandomization), 0f);

        yield return StartCoroutine(base.SpawnRoutine());
    }
}
