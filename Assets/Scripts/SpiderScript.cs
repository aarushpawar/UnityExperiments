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

    [SerializeField, Range(0f, 1f)]
    public float legSizeFalloff = 0.5f;

    [Header("Animation Settings")]
    [SerializeField]
    private float stepHeight = 0.5f;
    [SerializeField]
    private float moveSpeed = 5f;
    [SerializeField]
    private float legReachDistance = 1.5f;
    [SerializeField]
    private float groundCheckDistance = 2f;
    [SerializeField]
    private LayerMask groundLayer;

    // Store target and current positions for each leg
    private Vector3[] legTargets;
    private Vector3[] currentLegPositions;
    private bool[] legIsMoving;
    private float[] legMovementProgress;

    // Which legs can move together (opposite corners)
    private int[][] legGroups = new int[][] 
    {
        new int[] {0, 3, 4, 7}, // front-right, back-left, front-right, back-left
        new int[] {1, 2, 5, 6}  // front-left, back-right, front-left, back-right
    };
    private int currentMovingGroup = 0;

    public void GenerateLegs()
    {
        DestroyLegs();

        legs = new List<List<GameObject>>();


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

                if (i != 0) segment.transform.localPosition = new Vector3(0f, -previousSegment.transform.localScale.y * (1f / legSizeFalloff), 0f);
                segment.transform.localScale = Vector3.one * legSizeFalloff;

                segment.transform.LookAt(centerOfMass.transform.position + 
                    Vector3.up * Mathf.Lerp(-10f, -1f, (i / (float) (segmentCount - 1))), 
                    Vector3.up);

                previousSegment = segment;
                segments.Add(segment);
            }

            legs.Add(segments);
        }
    }

    public void DestroyLegs()
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
        GenerateLegs();
        InitializeLegArrays();
    }

    private void InitializeLegArrays()
    {
        int legCount = legPoints.Count;
        legTargets = new Vector3[legCount];
        currentLegPositions = new Vector3[legCount];
        legIsMoving = new bool[legCount];
        legMovementProgress = new float[legCount];

        // Initialize leg positions
        for (int i = 0; i < legCount; i++)
        {
            RaycastHit hit;
            Vector3 rayStart = legPoints[i].transform.position;
            Vector3 rayDirection = Vector3.down;

            if (Physics.Raycast(rayStart, rayDirection, out hit, groundCheckDistance, groundLayer))
            {
                legTargets[i] = hit.point;
                currentLegPositions[i] = hit.point;
            }
            else
            {
                legTargets[i] = rayStart + rayDirection * groundCheckDistance;
                currentLegPositions[i] = legTargets[i];
            }
        }
    }

    void Update()
    {
        UpdateLegMovement();
        UpdateBodyPosition();
    }

    private void UpdateLegMovement()
    {
        // Check if current group's legs need to move
        bool allLegsGrounded = true;
        foreach (int legIndex in legGroups[currentMovingGroup])
        {
            if (ShouldMoveLeg(legIndex))
            {
                allLegsGrounded = false;
                if (!legIsMoving[legIndex])
                {
                    StartLegMovement(legIndex);
                }
            }
        }

        // Update moving legs
        for (int i = 0; i < legPoints.Count; i++)
        {
            if (legIsMoving[i])
            {
                UpdateSingleLegMovement(i);
            }
        }

        // Switch groups if all legs are grounded
        if (allLegsGrounded)
        {
            currentMovingGroup = (currentMovingGroup + 1) % 2;
        }
    }

    private bool ShouldMoveLeg(int legIndex)
    {
        Vector3 idealPosition = GetIdealLegPosition(legIndex);
        float distanceToIdeal = Vector3.Distance(currentLegPositions[legIndex], idealPosition);
        return distanceToIdeal > legReachDistance;
    }

    private Vector3 GetIdealLegPosition(int legIndex)
    {
        Vector3 rayStart = legPoints[legIndex].transform.position;
        RaycastHit hit;
        if (Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance, groundLayer))
        {
            return hit.point;
        }
        return rayStart + Vector3.down * groundCheckDistance;
    }

    private void StartLegMovement(int legIndex)
    {
        legIsMoving[legIndex] = true;
        legMovementProgress[legIndex] = 0f;
        legTargets[legIndex] = GetIdealLegPosition(legIndex);
    }

    private void UpdateSingleLegMovement(int legIndex)
    {
        legMovementProgress[legIndex] += Time.deltaTime * moveSpeed;
        
        if (legMovementProgress[legIndex] >= 1f)
        {
            currentLegPositions[legIndex] = legTargets[legIndex];
            legIsMoving[legIndex] = false;
            return;
        }

        float height = Mathf.Sin(legMovementProgress[legIndex] * Mathf.PI) * stepHeight;
        Vector3 start = currentLegPositions[legIndex];
        Vector3 end = legTargets[legIndex];
        
        Vector3 current = Vector3.Lerp(start, end, legMovementProgress[legIndex]);
        current.y += height;
        
        currentLegPositions[legIndex] = current;
        UpdateLegSegments(legIndex, current);
    }

    private void UpdateLegSegments(int legIndex, Vector3 targetPosition)
    {
        List<GameObject> legSegments = legs[legIndex];
        Vector3[] positions = new Vector3[segmentCount + 1];
        
        // Get positions of each joint
        positions[0] = legPoints[legIndex].transform.position;
        positions[segmentCount] = targetPosition;

        // Simple FABRIK (Inverse Kinematics)
        float totalLength = 0f;
        float[] segmentLengths = new float[segmentCount];
        
        for (int i = 0; i < segmentCount; i++)
        {
            segmentLengths[i] = legSegments[i].transform.localScale.y;
            totalLength += segmentLengths[i];
        }

        // If target is too far, just point in that direction
        float targetDistance = Vector3.Distance(positions[0], positions[segmentCount]);
        if (targetDistance > totalLength)
        {
            Vector3 direction = (positions[segmentCount] - positions[0]).normalized;
            float currentDist = 0f;
            
            for (int i = 1; i < segmentCount; i++)
            {
                currentDist += segmentLengths[i - 1];
                positions[i] = positions[0] + direction * currentDist;
            }
        }
        else
        {
            // FABRIK algorithm
            for (int iteration = 0; iteration < 10; iteration++)
            {
                // Forward pass
                for (int i = segmentCount - 1; i > 0; i--)
                {
                    if (i == segmentCount - 1)
                        positions[i] = positions[segmentCount];
                    else
                    {
                        Vector3 direction = (positions[i + 1] - positions[i]).normalized;
                        positions[i] = positions[i + 1] - direction * segmentLengths[i];
                    }
                }

                // Backward pass
                for (int i = 1; i < segmentCount; i++)
                {
                    Vector3 direction = (positions[i] - positions[i - 1]).normalized;
                    positions[i] = positions[i - 1] + direction * segmentLengths[i - 1];
                }
            }
        }

        // Update segment positions and rotations
        for (int i = 0; i < segmentCount; i++)
        {
            GameObject segment = legSegments[i];
            segment.transform.position = positions[i];
            
            if (i < segmentCount - 1)
            {
                segment.transform.LookAt(positions[i + 1]);
            }
            else
            {
                segment.transform.LookAt(positions[segmentCount]);
            }
        }
    }

    private void UpdateBodyPosition()
    {
        // Calculate the average position of all grounded legs
        Vector3 averagePos = Vector3.zero;
        int groundedLegs = 0;
        
        for (int i = 0; i < legPoints.Count; i++)
        {
            if (!legIsMoving[i])
            {
                averagePos += currentLegPositions[i];
                groundedLegs++;
            }
        }
        
        if (groundedLegs > 0)
        {
            averagePos /= groundedLegs;
            
            // Move the body to maintain height above the average leg position
            Vector3 targetPos = new Vector3(
                averagePos.x,
                averagePos.y + groundCheckDistance * 0.5f,
                averagePos.z
            );
            
            centerOfMass.transform.position = Vector3.Lerp(
                centerOfMass.transform.position,
                targetPos,
                Time.deltaTime * moveSpeed
            );
        }
    }
}
