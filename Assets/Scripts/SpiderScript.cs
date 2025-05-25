using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderScript : MonoBehaviour {
    [SerializeField]
    public GameObject centerOfMass;

    [SerializeField]
    public List<GameObject> legPoints = new List<GameObject>();

    [SerializeField]
    public GameObject legPrefab;

    [SerializeField]
    public int segmentCount = 3;

    List<List<GameObject>> legs = new List<List<GameObject>>();

    void Start() {
        float legLength = 0.8f;
        float segmentLength = legLength / segmentCount;

        foreach (GameObject point in legPoints) {
            List<GameObject> segments = new List<GameObject>();

            Vector3 start = point.transform.position;
            Vector3 toCenter = (centerOfMass.transform.position - start).normalized;
            Vector3 outward = -toCenter;
            Vector3 lastPosition = start;

            for (int i = 0; i < segmentCount; i++) {
                float t = i / (float)(segmentCount - 1);

                Vector3 arcOffset = Vector3.up * Mathf.Cos(t * Mathf.PI) * 0.4f;
                Vector3 outwardOffset = outward * segmentLength;

                Vector3 nextPosition = lastPosition + outwardOffset + arcOffset;

                GameObject segment = Instantiate(legPrefab, lastPosition, Quaternion.identity);
                segment.transform.localScale = new Vector3(0.1f, 0.1f, segmentLength);
                segment.transform.rotation = Quaternion.LookRotation((nextPosition - lastPosition).normalized);
                segment.transform.SetParent(point.transform);

                segments.Add(segment);
                lastPosition = nextPosition;
            }

            legs.Add(segments);
        }
    }

    void Update() {

    }
}
