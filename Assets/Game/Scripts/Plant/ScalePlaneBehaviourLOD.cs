using UnityEngine;
using System.Collections;

public class ScalePlaneBehaviourLOD : ScalePlantBehaviour {

    public float lodDist;
    private float lodDistSqr = -1;

    public Material farDistMaterial;
    private Material defaultMaterial;

    public override void UpdatePlayerPos(Vector3 playerPos) {
        base.UpdatePlayerPos(playerPos);

        if (spawned && renderEnabled) {
            if (defaultMaterial == null) {
                defaultMaterial = renderer.sharedMaterial;
            }

            if (lodDistSqr == -1) {
                lodDistSqr = lodDist * lodDist;
            }

            float sqrDist = (playerPos - transform.position).sqrMagnitude;
            bool farLOD = sqrDist >= lodDistSqr;
            renderer.sharedMaterial = farLOD ? farDistMaterial : defaultMaterial;
        }
    }
}
