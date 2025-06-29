using UnityEngine;
using System.Collections.Generic;

public class HitDetection : MonoBehaviour
{
    // The body part this object represents
    public string bodyPart;
    public string wallTag = "Wall";
    public GameObject circularEffectPrefab;
    
    // Enable to show detailed debug information about hit points
    public bool debugMode = false;

    // List of all possible body part types that can be hit
    public static readonly List<string> AllBodyParts = new List<string> 
    { 
        "LeftArm", 
        "RightArm", 

    };
    
    // Used to track which walls this body part has already hit
    private HashSet<int> hitWalls = new HashSet<int>();

    private void Start()
    {
        // Verify this object has a collider
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            Debug.LogError($"[{gameObject.name}] Missing collider component! This object won't detect collisions.");
        }
        else if (!collider.enabled)
        {
            Debug.LogWarning($"[{gameObject.name}] Collider is disabled! This object won't detect collisions.");
        }
        
        // Verify this object has a rigidbody if using collision detection
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Missing Rigidbody component. This might affect collision detection.");
        }
        
        // Verify bodyPart is one of the valid body parts
        if (!AllBodyParts.Contains(bodyPart))
        {
                Debug.LogError($"[{gameObject.name}] Invalid body part: {bodyPart}. Valid body parts are: {string.Join(", ", AllBodyParts)}");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(wallTag))
        {
            int wallId = collision.gameObject.GetInstanceID();
            
            // Check if this body part already hit this wall
            if (hitWalls.Contains(wallId))
            {
                if (debugMode) Debug.Log($"[{gameObject.name}] ({bodyPart}) already hit wall {wallId}, ignoring collision.");
                return;
            }
            
            if (debugMode) Debug.Log($"[{gameObject.name}] ({bodyPart}) collision with WALL {wallId} detected!");

            // Find the closest valid hit point from ANY body part type
            Transform closestHitPoint = FindClosestHitPointOfAnyType(collision.gameObject, collision.contacts[0].point);

            if (closestHitPoint != null)
            {
                string hitPointType = ExtractBodyPartType(closestHitPoint.name);
                if (debugMode) Debug.Log($"SUCCESS: {bodyPart} hit a {hitPointType} target! Hit point: {closestHitPoint.name}");
                
                // Register hit with ScoreManager but don't destroy the wall - ScoreManager will handle that
                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.RegisterBodyPartHit(
                        collision.gameObject, 
                        bodyPart, 
                        collision.contacts[0].point, 
                        closestHitPoint,
                        hitPointType
                    );
                    
                    // Mark this wall as hit by this body part
                    hitWalls.Add(wallId);
                }
                else
                {
                    Debug.LogError("ScoreManager instance is null! Make sure it's properly initialized.");
                }

                // Instantiate the circular effect at the collision point
                if (circularEffectPrefab != null)
                {
                    Instantiate(circularEffectPrefab, collision.contacts[0].point, Quaternion.identity);
                }
                else if (debugMode)
                {
                    Debug.LogWarning("Circular effect prefab is not assigned!");
                }
            }
            else
            {
                Debug.LogError($"FAILED: No valid hitpoints found on this wall for {gameObject.name} ({bodyPart})!");
            }
        }
    }

    // Print all hit points in the wall for debugging
    private void PrintAllHitPoints(GameObject wall)
    {
        Debug.Log($"Searching for hit points in: {wall.name}");
        foreach (Transform child in wall.GetComponentsInChildren<Transform>())
        {
            if (child != wall.transform)
            {
                Debug.Log($"Found child object: {child.name}");
            }
        }
    }

    // Extract the body part type from a hit point name
    private string ExtractBodyPartType(string hitPointName)
    {
        foreach (string part in AllBodyParts)
        {
            if (hitPointName.Contains("PerfectHitPoints_" + part))
            {
                return part;
            }
        }
        return "Unknown";
    }

    // Check if a name matches any body part's perfect hit point pattern
    private bool IsAnyPerfectHitPoint(string name)
    {
        foreach (string part in AllBodyParts)
        {
            string pattern = "PerfectHitPoints_" + part;
            if (name.Contains(pattern))
            {
                if (debugMode) Debug.Log($"Valid hit point found: {name} matches pattern {pattern}");
                return true;
            }
        }
        
        if (debugMode) Debug.Log($"Invalid hit point: {name} doesn't match any pattern");
        return false;
    }

    // Find the closest hit point of any body part type
    private Transform FindClosestHitPointOfAnyType(GameObject wall, Vector3 hitPosition)
    {
        Transform closest = null;
        float minDistance = Mathf.Infinity;
        
        if (debugMode) Debug.Log($"Looking for hit points in {wall.name} near position {hitPosition}");
        
        // First check direct children of the wall
        foreach (Transform child in wall.transform)
        {
            if (IsAnyPerfectHitPoint(child.name))
            {
                float distance = Vector3.Distance(hitPosition, child.position);
                if (debugMode) Debug.Log($"Checking hit point: {child.name}, distance: {distance}");
                
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = child;
                    if (debugMode) Debug.Log($"New closest hit point: {child.name}, distance: {distance}");
                }
            }
        }
        
        // Then check all children recursively to ensure we find everything
        foreach (Transform hitPoint in wall.GetComponentsInChildren<Transform>())
        {
            if (hitPoint == wall.transform) continue;
            
            if (IsAnyPerfectHitPoint(hitPoint.name))
            {
                float distance = Vector3.Distance(hitPosition, hitPoint.position);
                if (debugMode) Debug.Log($"Checking hit point: {hitPoint.name}, distance: {distance}");
                
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = hitPoint;
                    if (debugMode) Debug.Log($"New closest hit point: {hitPoint.name}, distance: {distance}");
                }
            }
        }
        
        if (closest != null && debugMode)
        {
            Debug.Log($"Found closest hit point: {closest.name} at distance {minDistance}");
        }
        else if (closest == null)
        {
            Debug.LogError("No valid hit points found at all!");
        }
        
        return closest;
    }
    
    // Public method to clear hit walls when needed (e.g., on level restart)
    public void ClearHitWalls()
    {
        hitWalls.Clear();
    }
}