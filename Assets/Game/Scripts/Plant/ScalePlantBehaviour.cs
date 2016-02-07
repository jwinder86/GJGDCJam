using UnityEngine;
using System.Collections;

public class ScalePlantBehaviour : PlantBehaviour {

    public static readonly float MIN_SCALE = 0.01f;
    public static readonly Vector3 MIN_SCALE_VECTOR = new Vector3(MIN_SCALE, MIN_SCALE, MIN_SCALE);

    public bool randomizeYRotation;
    public AnimationCurve curve;
    public float yScaleRandomization = 0f;
    new public Renderer renderer;

    private Vector3 goalScale;

    // Use this for initialization
    new void Start () {
        base.Start();

        // rotate about upward axis
        if (randomizeYRotation) {
            transform.Rotate(Vector3.up, Random.Range(0f, 360f), Space.Self);
        }

        goalScale = transform.localScale;
        goalScale.y += Random.Range(-yScaleRandomization, yScaleRandomization);
    }

    public override void SetNormal(Vector3 normal) {
        transform.rotation *= Quaternion.FromToRotation(Vector3.up, normal);
    }

    protected override void EnableRender(bool render) {
        renderer.enabled = render;
    }

    protected override IEnumerator SpawnRoutine() {
        transform.localScale = MIN_SCALE_VECTOR;

        for (float timer = 0f; timer < spawnTime; timer += Time.deltaTime) {
            Vector3 scale = Vector3.LerpUnclamped(MIN_SCALE_VECTOR, goalScale, curve.Evaluate(timer / spawnTime));
            transform.localScale = scale;

            yield return null;
        }

        transform.localScale = goalScale;
    }
}
