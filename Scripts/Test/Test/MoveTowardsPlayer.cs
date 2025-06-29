using UnityEngine;

public class MoveTowardsPlayer : MonoBehaviour
{
    public float speed = 5f;
    private Transform player;
    private Vector3 moveDirection;
    
    // If true, the object will move along a predetermined axis instead of directly to the player
    public bool constrainToAxis = true;
    
    // The axis to move along (typically the spawner's forward direction)
    public Vector3 movementAxis = Vector3.forward;

    private void Start()
    {
        player = Camera.main.transform;
        
        if (constrainToAxis)
        {
            // Normalize the movement axis
            movementAxis.Normalize();
            
            // Use the specified axis as the movement direction
            moveDirection = movementAxis;
        }
        else
        {
            // Calculate initial direction toward player (only done once at Start)
            moveDirection = (player.position - transform.position).normalized;
        }
    }

    private void Update()
    {
        if (constrainToAxis)
        {
            // Move along the constrained axis, maintaining a straight line
            transform.position -= moveDirection * speed * Time.deltaTime;
        }
        else
        {
            // Original behavior - move directly toward the camera's current position
            transform.position = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        }
    }

    // Optional: Visualize the movement direction in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, movementAxis.normalized * 3f);
    }
}