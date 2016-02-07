using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FractalPlantBehaviour : PlantBehaviour {

    public FractalPlantComponent bottomPrefab;
    public FractalPlantComponent middlePrefab;
    public FractalPlantComponent topPrefab;

    public AnimationCurve curve;

    public int height = 6;

    private FractalPlantComponent root;
    private List<Renderer> renderers;

    private Vector3 goalScale;

    new private AudioSource audio;

    new void Awake() {
        base.Awake();

        audio = GetComponent<AudioSource>();

        GeneratePlant();
        renderers = new List<Renderer>(root.Count());
        root.PopulateRenderers(ref renderers);
    }

    new void Start() {
        base.Start();

        foreach (Renderer r in renderers) {
            r.enabled = false;
        }

        root.LerpScaleRecursive(0f);

        goalScale = transform.localScale;
    }

    public override void SetNormal(Vector3 normal) {
        // do nothing
    }

    protected override IEnumerator SpawnRoutine() {
        if (audio != null) {
            audio.Play();
        }

        foreach (Renderer r in renderers) {
            r.enabled = true;
        }

        root.LerpScaleRecursive(0f);

        for (float timer = 0f; timer < spawnTime; timer += Time.deltaTime) {
            root.LerpScaleRecursive(curve.Evaluate(timer / spawnTime));

            yield return null;
        }

        root.LerpScaleRecursive(1f);

        this.enabled = false;
    }

    private void GeneratePlant() {
        if (bottomPrefab != null) {
            root = (FractalPlantComponent)Instantiate(bottomPrefab);
        } else {
            root = (FractalPlantComponent)Instantiate(middlePrefab);
        }
        root.transform.parent = transform;
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;

        List<Vector3> termPositions = new List<Vector3>();

        root.GenerateRecursive(height, middlePrefab, topPrefab, ref termPositions);
    }
}
