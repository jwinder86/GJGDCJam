using UnityEngine;
using System.Collections;

public class ProceduralVineSegment : MonoBehaviour {
    public float length;

    public GameObject leaves;

    public void RemoveLeaves() {
        if (leaves != null) {
            DestroyImmediate(leaves);
        }
    }
}
