using UnityEngine;
using System.Collections;

public class SegmentHeadBehaviour : MonoBehaviour {

    public SegmentBehaviour headPrefab;
    public SegmentBehaviour segmentPrefab;
    public SegmentBehaviour increasingSegmentPrefab;
    public SegmentBehaviour tailPrefab;

    public int segmentCount = 10;
    public int increasingSegmentCount = 2;
    public int wingedSegments = 3;
    public int dorsalFinSegment = 2;
    public float segmentScaleRatio = 0.9f;

    private SegmentBehaviour head;
    private BoidBehaviour boid;

    void Awake() {
        boid = GetComponent<BoidBehaviour>();
    }

	// Use this for initialization
	void Start () {
        CreateSegments();
        head.Initialize(transform.position, boid);
	}

    private void CreateSegments() {
        float currentScale = 1f;
        SegmentBehaviour prevSegment = null;

        for (int i = 0; i < segmentCount; i++) {
            bool inc = false;
            SegmentBehaviour segment;
            if (i == 0) {
                segment = Instantiate(headPrefab);
            } else if (i == segmentCount - 1) {
                segment = Instantiate(tailPrefab);
            } else if (i < increasingSegmentCount) {
                segment = Instantiate(increasingSegmentPrefab);
                inc = true;
            } else {
                segment = Instantiate(segmentPrefab);
            }

            segment.transform.parent = transform;
            if (inc) {
                segment.transform.localScale *= currentScale / segmentScaleRatio;
            } else {
                segment.transform.localScale *= currentScale;
            }
            segment.name += " " + (i + 1);

            // remove wings 
            if (i > wingedSegments) {
                segment.RemoveWings();
            }

            // remove dorsal fins
            if (i != dorsalFinSegment) {
                segment.RemoveDorsalFin();
            }

            // assign child
            if (prevSegment == null) {
                head = segment;
            } else {
                prevSegment.child = segment;
            }

            prevSegment = segment;

            if (inc) {
                currentScale /= segmentScaleRatio;
            } else {
                currentScale *= segmentScaleRatio;
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
        head.UpdatePosition(transform.position, boid.Roll);
	}
}
