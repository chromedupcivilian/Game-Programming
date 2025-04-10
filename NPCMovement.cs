using UnityEngine;
using UnityEngine.AI; // Required for NavMeshAgent
using System.Collections; // Required for IEnumerator for waiting

// Define the possible states for the NPC's behavior
public enum AIState { Patrol, Chase, Wait } // Added Wait state

[RequireComponent(typeof(NavMeshAgent))]
public class NPCMovement : MonoBehaviour
{
    [Header("State Machine")]
    [Tooltip("The current state of the NPC's behavior.")]
    public AIState currentState = AIState.Patrol;

    [Header("Patrol Settings")]
    [Tooltip("Points the NPC will move between when patrolling.")]
    public Transform[] patrolPoints;
    [Tooltip("Time in seconds the NPC waits at each patrol point.")]
    public float patrolWaitTime = 2.0f;

    [Header("Chase Settings")]
    [Tooltip("Reference to the player's Transform.")]
    public Transform player;
    [Tooltip("Distance within which the NPC starts chasing the player.")]
    public float chaseDistanceThreshold = 7.0f;
    [Tooltip("Distance beyond which the NPC stops chasing and returns to patrol.")]
    public float loseSightDistanceThreshold = 10.0f;

    // Private variables
    private NavMeshAgent agent;
    private int currentPatrolIndex;
    private bool isWaiting = false; // Flag specifically for patrol waiting

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentPatrolIndex = -1; // Start at -1 so the first call to GoToNext increments to 0

        // --- Error Checks ---
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component not found on " + gameObject.name + ". Script disabled.");
            this.enabled = false;
            return;
        }

        if (player == null)
        {
            // Log warning instead of error, patrol might still work
            Debug.LogWarning("Player Transform not assigned in NPCMovement Inspector on " + gameObject.name + ". Chase behavior disabled.");
        }

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogWarning("Patrol Points array is empty or not assigned in NPCMovement Inspector on " + gameObject.name + ". Patrol behavior disabled.");
            // If no patrol points AND no player, disable script? Or just default to Chase if player exists?
            if (player != null)
            {
                currentState = AIState.Chase; // Default to chase if player exists but no patrol points
                Debug.Log(gameObject.name + " has no patrol points, defaulting to Chase state.");
            }
            else
            {
                this.enabled = false; // No player, no patrol points = nothing to do
            }
        }
        else
        {
            // Start the patrol routine if points exist
            GoToNextPatrolPoint();
        }
        // --- End Error Checks ---
    }

    void Update()
    {
        if (!agent.enabled || !agent.isOnNavMesh) return; // Exit if agent isn't ready

        // Calculate distance to player ONLY if player exists
        float distanceToPlayer = float.MaxValue;
        if (player != null)
        {
            distanceToPlayer = Vector3.Distance(transform.position, player.position);
        }

        // --- State Transition Logic ---
        // Only check for transitions if a player exists to trigger them
        if (player != null)
        {
            // Condition to switch from Patrol/Wait TO Chase
            if ((currentState == AIState.Patrol || currentState == AIState.Wait) && distanceToPlayer < chaseDistanceThreshold)
            {
                SwitchState(AIState.Chase);
            }
            // Condition to switch from Chase TO Patrol
            else if (currentState == AIState.Chase && distanceToPlayer > loseSightDistanceThreshold)
            {
                SwitchState(AIState.Patrol);
            }
        }

        // --- State Execution Logic ---
        // Execute behavior based on the current state
        switch (currentState)
        {
            case AIState.Patrol:
                Patrol();
                break;
            case AIState.Chase:
                ChasePlayer();
                break;
            case AIState.Wait:
                // Waiting logic is handled by the coroutine started in Patrol()
                // No continuous action needed here, but state prevents other actions
                break;
        }
    }

    // Central function to handle state changes and associated actions
    void SwitchState(AIState newState)
    {
        if (currentState == newState) return; // No change needed

        // Stop any waiting coroutine if we are leaving the Wait state
        if (currentState == AIState.Wait)
        {
            StopCoroutine("WaitAtPatrolPoint");
            isWaiting = false;
        }

        currentState = newState;
        // Debug.Log(gameObject.name + " switched to state: " + newState);

        // Actions to take upon ENTERING the new state
        switch (newState)
        {
            case AIState.Patrol:
                // When switching back to patrol, find the next point
                GoToNextPatrolPoint();
                break;
            case AIState.Chase:
                // Ensure agent speed is appropriate for chasing (optional)
                // agent.speed = chaseSpeed;
                break;
            case AIState.Wait:
                // The Wait state is entered via the coroutine, no direct action here
                break;
        }
    }


    void Patrol()
    {
        // Ensure agent speed is appropriate for patrolling (optional)
        // agent.speed = patrolSpeed;

        // Check if the agent has reached its destination (within stopping distance)
        // !agent.pathPending ensures the agent isn't still calculating a path
        if (!isWaiting && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            // If we've reached the point and aren't already waiting, start the wait timer
            StartCoroutine(WaitAtPatrolPoint());
        }
    }

    IEnumerator WaitAtPatrolPoint()
    {
        isWaiting = true;
        currentState = AIState.Wait; // Explicitly set state to Wait
        // Debug.Log(gameObject.name + " waiting at point " + currentPatrolIndex);
        yield return new WaitForSeconds(patrolWaitTime);

        // After waiting, if we are still in the Wait state (haven't switched to Chase)
        if (currentState == AIState.Wait)
        {
            isWaiting = false;
            SwitchState(AIState.Patrol); // Trigger state switch back to Patrol to find next point
        }
        // If state changed (e.g., to Chase) during the wait, the coroutine ends but does nothing further
    }


    void GoToNextPatrolPoint()
    {
        // Check if patrol points exist
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogWarning(gameObject.name + " cannot patrol: Patrol Points array is empty.");
            // Optionally switch to Chase if player exists, or Idle if implemented
            if (player != null) SwitchState(AIState.Chase);
            return;
        }

        // Increment patrol index, wrapping around using the modulo operator (%)
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;

        // Set the agent's destination to the new patrol point's position
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        // Debug.Log(gameObject.name + " moving to patrol point " + currentPatrolIndex + " at " + agent.destination);
    }

    void ChasePlayer()
    {
        // Check if player exists (it might have been destroyed)
        if (player == null)
        {
            // If player is lost/destroyed, go back to patrol
            SwitchState(AIState.Patrol);
            return;
        }

        // Continuously update the agent's destination to the player's current position
        agent.SetDestination(player.position);
        // Debug.Log(gameObject.name + " chasing player at " + player.position); // Can be spammy
    }
}
