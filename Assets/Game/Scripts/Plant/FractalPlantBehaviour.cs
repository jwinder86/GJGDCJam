using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FractalPlantBehaviour : PlantBehaviour {

    public FractalPlantComponent bottomPrefab;
    public FractalPlantComponent middlePrefab;
    public FractalPlantComponent topPrefab;

    public AnimationCurve curve;

    public int height = 6;
    public int maxRendererHeight = 4;

    public float spawnDelay = 0.3f;

    private FractalPlantComponent root;
    private Renderer[] renderers;
    private LODGroup lod;

    void Awake() {
        lod = GetComponent<LODGroup>();

        GeneratePlant();
        renderers = GetComponentsInChildren<Renderer>();
        GenerateLODs(renderers);
    }

    new void Start() {
        base.Start();

        root.LerpScaleRecursive(0f);
    }

    public override void SetNormal(Vector3 normal) {
        // do nothing
    }

    protected override void EnableRender(bool render) {
        foreach (Renderer r in renderers) {
            if (r != null) {
                r.enabled = render;
            } else {
                Debug.LogError("Got missing renderer!", this);
            }
        }
    }

    protected override IEnumerator SpawnRoutine() {
        yield return new WaitForSeconds(spawnDelay);

        soundManager.PlaySound(SoundType.Creak, transform);

        root.LerpScaleRecursive(0f);

        for (float timer = 0f; timer < spawnTime; timer += Time.deltaTime) {
            root.LerpScaleRecursive(curve.Evaluate(timer / spawnTime));

            yield return null;
        }

        root.LerpScaleRecursive(1f);
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

        root.GenerateRecursive(height, maxRendererHeight, middlePrefab, topPrefab, ref termPositions);
    }

    private void GenerateLODs(Renderer[] renderers) {
        LOD[] existingLODs = lod.GetLODs();

        if (existingLODs.Length != 2) {
            Debug.LogError("Unexpected LOD count: " + existingLODs.Length + ", " + name);
        }

        List<Renderer> lod0s = new List<Renderer>();
        List<Renderer> lod1s = new List<Renderer>();

        for (int i = 0; i < renderers.Length; i++) {
            if (renderers[i].name.Equals("LOD0")) {
                lod0s.Add(renderers[i]);
            } else if (renderers[i].name.Equals("LOD1")) {
                lod1s.Add(renderers[i]);
            } else {
                Debug.LogError("Unexpected LOD name: " + renderers[i].name);
            }
        }

        LOD lod0 = new LOD(existingLODs[0].screenRelativeTransitionHeight, lod0s.ToArray());
        lod0.fadeTransitionWidth = existingLODs[0].fadeTransitionWidth;

        LOD lod1 = new LOD(existingLODs[1].screenRelativeTransitionHeight, lod1s.ToArray());
        lod1.fadeTransitionWidth = existingLODs[1].fadeTransitionWidth;

        lod.SetLODs(new LOD[] { lod0, lod1 });
        lod.RecalculateBounds();
    }
}
