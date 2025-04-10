using UnityEngine;

public class FireflyMovement : MonoBehaviour
{
    [Tooltip("Assign the empty GameObjects that define the patrol path.")]
    public Transform[] waypoints;

    [Tooltip("How fast the firefly/emitter moves.")]
    public float speed = 0.5f;

    [Tooltip("How close the firefly needs to get to a waypoint to switch target.")]
    public float waypointThreshold = 0.1f;

    private int currentWaypointIndex = 0;
    private Transform targetWaypoint;

    void Start()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning("No waypoints assigned to FireflyMovement. Disabling script.", this);
            this.enabled = false;
            return;
        }

        // Start moving towards the first waypoint
        targetWaypoint = waypoints[currentWaypointIndex];
        // Optional: Start at the first waypoint's position
        // transform.position = targetWaypoint.position;
    }

    void Update()
    {
        if (targetWaypoint == null) return; // Should not happen if Start check passes

        // Calculate direction and distance to the target waypoint
        Vector3 direction = targetWaypoint.position - transform.position;
        float distance = direction.magnitude;

        // Check if close enough to the target waypoint
        if (distance <= waypointThreshold)
        {
            // Move to the next waypoint index, looping back to 0
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            targetWaypoint = waypoints[currentWaypointIndex];
            // Debug.Log("Moving to waypoint: " + currentWaypointIndex);
        }
        else
        {
            // Move towards the target waypoint using Lerp for smooth movement
            // Note: Simple MoveTowards is often easier for constant speed path following
            // transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, speed * Time.deltaTime);

            // Using Lerp (requires careful calculation for constant speed, MoveTowards is simpler here)
            // float step = speed * Time.deltaTime;
            // transform.position = Vector3.Lerp(transform.position, targetWaypoint.position, step / distance); // Approximation

            // Let's stick to MoveTowards for simplicity and constant speed:
            transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, speed * Time.deltaTime);

             // Optional: Make the firefly gently look towards the direction it's moving
             if (direction != Vector3.zero) // Avoid zero vector warning
             {
                // Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
                // transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2f); // Adjust rotation speed
             }
        }
    }
}
