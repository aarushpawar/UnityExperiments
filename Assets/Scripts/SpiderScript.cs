using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class SpiderScript : MonoBehaviour
{
    [SerializeField]
    public GameObject centerOfMass;

    [SerializeField]
    public List<GameObject> legPoints = new List<GameObject>();

    [SerializeField]
    public GameObject legPrefab;
    // anchor point of prefab is at the top of the cube
    // cube is 1x1x1
    // needs to be resized because spider body is 0.4x0.4x0.4

    [Header("Leg Settings")]

    [SerializeField, Range(0, 100)]
    public int segmentCount = 3;

    List<List<GameObject>> legs = new List<List<GameObject>>();

    public void GenerateLegs()
    {
        DestroyLegs();

        legs = new List<List<GameObject>>();

        float sizeDescale = 0.7f;

        foreach (GameObject point in legPoints)
        {
            List<GameObject> segments = new List<GameObject>();

            point.transform.LookAt(centerOfMass.transform, Vector3.forward);
            point.transform.eulerAngles = new Vector3(0f, point.transform.eulerAngles.y + 180f, 0f);
            point.transform.localScale = Vector3.one * 0.8f;

            GameObject previousSegment = point;

            for (int i = 0; i < segmentCount; i++)
            {
                GameObject segment = Instantiate(legPrefab, previousSegment.transform);

                if (i != 0) segment.transform.localPosition = new Vector3(0f, -previousSegment.transform.localScale.y * (1f / sizeDescale), 0f);
                segment.transform.localScale = Vector3.one * sizeDescale;

                segment.transform.LookAt(centerOfMass.transform.position + 
                    Vector3.up * Mathf.Lerp(-10f, -1f, (i / (float) (segmentCount - 1))), 
                    Vector3.up);

                previousSegment = segment;
                segments.Add(segment);
            }

            legs.Add(segments);
        }
    }

    void DestroyLegs()
    {
        if (legs != null)
        {
            foreach (List<GameObject> leg in legs)
            {
                foreach (GameObject segment in leg)
                {
                    DestroyImmediate(segment);
                }
            }
            legs.Clear();
        }

        foreach (GameObject point in legPoints)
        {
            foreach (Transform child in point.transform)
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }

    void Start()
    {
        //GenerateLegs(); // Remove from Start
    }

    void Update()
    {

    }
}
