using UnityEngine;
using UnityEngine.AI; // Required for NavMeshAgent
using System.Collections; // Required for Coroutines (like waiting)

public enum AIState { Idle, Patrol, Chase, Attack }

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))] // Assuming you have animations
public class AIController : MonoBehaviour
{
    [Header("AI Settings")]
    public AIState currentState = AIState.Patrol;
    public float detectionRadius = 10f;
    public float attackRange = 2f; // Must be <= agent.stoppingDistance for attack state
    public float attackCooldown = 1.5f;
    public float patrolWaitTime = 3.0f;

    [Header("References")]
    public Transform playerTarget; // Assign the Player's Transform
    public Transform[] patrolPoints; // Assign empty GameObjects for patrol route

    // Components
    private NavMeshAgent agent;
    private Animator animator;

    // State variables
    private int currentPatrolIndex = -1;
    private bool isWaitingAtPatrolPoint = false;
    private float lastAttackTime = -Mathf.Infinity; // Ensure can attack immediately

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // --- Error Checks ---
        if (agent == null) { Debug.LogError("NavMeshAgent component missing!", this); this.enabled = false; return; }
        if (animator == null) { Debug.LogWarning("Animator component missing - animations won't play.", this); }
        if (playerTarget == null) { Debug.LogWarning("Player Target not assigned. Chase/Attack states disabled.", this); }
        if (patrolPoints == null || patrolPoints.Length == 0) { Debug.LogWarning("Patrol Points not assigned. Patrol state disabled.", this); }

        // Ensure attack range makes sense with stopping distance
        if (attackRange > agent.stoppingDistance)
        {
            Debug.LogWarning($"Attack Range ({attackRange}) is greater than Agent Stopping Distance ({agent.stoppingDistance}). Agent might not reach attack range. Adjust Stopping Distance.", this);
            // Optionally force stopping distance: agent.stoppingDistance = attackRange;
        }
    }

    void Start()
    {
        // Initialize state
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            GoToNextPatrolPoint();
            currentState = AIState.Patrol;
        }
        else
        {
            currentState = AIState.Idle; // Default to Idle if no patrol points
        }
    }

    void Update()
    {
        if (!agent.isOnNavMesh) return; // Don't do anything if not on a NavMesh

        // Calculate distance to player (if player exists)
        float distanceToPlayer = playerTarget != null ? Vector3.Distance(transform.position, playerTarget.position) : Mathf.Infinity;

        // --- State Transition Logic ---
        AIState nextState = currentState; // Start by assuming no change

        switch (currentState)
        {
            case AIState.Idle:
                if (playerTarget != null && distanceToPlayer <= detectionRadius)
                    nextState = AIState.Chase;
                else if (patrolPoints != null && patrolPoints.Length > 0)
                    nextState = AIState.Patrol; // Start patrolling if possible
                break;

            case AIState.Patrol:
                if (playerTarget != null && distanceToPlayer <= detectionRadius)
                    nextState = AIState.Chase;
                break;

            case AIState.Chase:
                if (playerTarget == null || distanceToPlayer > detectionRadius * 1.2f) // Lose sight slightly beyond detection radius
                    nextState = AIState.Patrol; // Go back to patrolling
                else if (distanceToPlayer <= agent.stoppingDistance) // Use stopping distance to check if in range for potential attack
                    nextState = AIState.Attack;
                break;

            case AIState.Attack:
                if (playerTarget == null || distanceToPlayer > agent.stoppingDistance) // If player moved out of stopping distance
                    nextState = AIState.Chase; // Go back to chasing
                break;
        }

        // Apply state change if needed
        if (nextState != currentState)
        {
            SwitchState(nextState);
        }

        // --- State Execution Logic ---
        ExecuteCurrentState(distanceToPlayer);

        // --- Update Animator ---
        UpdateAnimator();
    }

    void SwitchState(AIState newState)
    {
        // Actions on EXITING the old state
        OnStateExit(currentState, newState);

        currentState = newState;
        // Debug.Log("Switched to state: " + currentState);

        // Actions on ENTERING the new state
        OnStateEnter(currentState);
    }

    void OnStateEnter(AIState state)
    {
        switch (state)
        {
            case AIState.Idle:
                agent.isStopped = true; // Stop moving
                agent.ResetPath();      // Clear any path
                break;
            case AIState.Patrol:
                agent.isStopped = false;
                agent.stoppingDistance = 0.5f; // Stop close to patrol points
                if (!isWaitingAtPatrolPoint) GoToNextPatrolPoint(); // Start moving if not already waiting
                break;
            case AIState.Chase:
                agent.isStopped = false;
                agent.stoppingDistance = attackRange; // Set stopping distance for attack transition
                // Optional: Increase speed when chasing
                // agent.speed = chaseSpeed;
                break;
            case AIState.Attack:
                agent.isStopped = true; // Stop moving to attack
                lastAttackTime = Time.time - attackCooldown; // Allow immediate attack on enter if desired
                break;
        }
    }

     void OnStateExit(AIState oldState, AIState newState)
    {
        switch (oldState)
        {
            case AIState.Patrol:
                 // Stop waiting coroutine if we were interrupted while waiting
                 if (isWaitingAtPatrolPoint)
                 {
                     StopCoroutine("WaitAtPatrolPointCoroutine");
                     isWaitingAtPatrolPoint = false;
                 }
                break;
            case AIState.Attack:
                // Ensure agent can move again if leaving attack state
                agent.isStopped = false;
                break;
            // Add other exit logic if needed
        }
    }

    void ExecuteCurrentState(float distanceToPlayer)
    {
        switch (currentState)
        {
            case AIState.Idle:
                // Do nothing specific, just wait for transitions
                break;
            case AIState.Patrol:
                HandlePatrolState();
                break;
            case AIState.Chase:
                HandleChaseState();
                break;
            case AIState.Attack:
                HandleAttackState(distanceToPlayer);
                break;
        }
    }

    void HandlePatrolState()
    {
        // Check if agent has reached the destination (within stopping distance)
        // !agent.pathPending ensures it's not still calculating
        if (!isWaitingAtPatrolPoint && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            StartCoroutine(WaitAtPatrolPointCoroutine());
        }
    }

    IEnumerator WaitAtPatrolPointCoroutine()
    {
        isWaitingAtPatrolPoint = true;
        agent.isStopped = true; // Explicitly stop while waiting
        // Debug.Log("Waiting at patrol point " + currentPatrolIndex);
        yield return new WaitForSeconds(patrolWaitTime);

        // Only proceed if we are STILL in the Patrol state after waiting
        if (currentState == AIState.Patrol)
        {
             agent.isStopped = false; // Allow movement again
             GoToNextPatrolPoint();
        }
         isWaitingAtPatrolPoint = false; // Reset flag regardless
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            SwitchState(AIState.Idle); // Can't patrol, go idle
            return;
        }

        // Cycle through patrol points
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        // Debug.Log("Moving to patrol point " + currentPatrolIndex);
    }

    void HandleChaseState()
    {
        if (playerTarget != null)
        {
            agent.SetDestination(playerTarget.position);
        }
        else
        {
            // Player lost or destroyed, go back to patrol/idle
            SwitchState(AIState.Patrol);
        }
    }

    void HandleAttackState(float distanceToPlayer)
    {
        // Ensure agent is stopped
        if (!agent.isStopped) agent.isStopped = true;

        // Face the player
        if (playerTarget != null)
        {
            Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;
            // Prevent tilting up/down when looking
            directionToPlayer.y = 0;
            if (directionToPlayer != Vector3.zero) // Avoid zero vector warning
            {
                 Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
                 // Smooth rotation (optional)
                 transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * agent.angularSpeed / 45); // Adjust divisor for speed
                 // Or instant rotation: transform.rotation = lookRotation;
            }
        }

        // Check attack cooldown
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            // Perform Attack
            Debug.Log("Attacking Player!");
            if (animator != null) animator.SetTrigger("Attack"); // Trigger animation

            lastAttackTime = Time.time; // Reset cooldown timer
        }
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        // Get speed from agent velocity
        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed); // Assuming a "Speed" float parameter

        // Example bool for walking/running based on state or speed
        bool isMoving = currentState == AIState.Chase || (currentState == AIState.Patrol && !isWaitingAtPatrolPoint && speed > 0.1f);
        animator.SetBool("IsWalking", isMoving);
    }

    // --- Dodging (Advanced Concept) ---
    // Dodging requires more complex logic, often involving:
    // 1. Detecting Threats: Raycasting, collision checks for projectiles.
    // 2. Deciding to Dodge: Based on threat proximity, angle, AI state.
    // 3. Executing Dodge:
    //    - Temporarily overriding NavMeshAgent: Calculate a dodge direction (e.g., perpendicular to threat), apply direct velocity using `agent.velocity = dodgeDirection * dodgeSpeed;` for a short duration, then potentially `agent.ResetPath()` or `agent.SetDestination()` back to the original target.
    //    - Using Off-Mesh Links or custom movement scripts.
    //    - Triggering a dodge animation.
    // This is significantly more involved than the basic states above.

    // Optional: Visualize detection radius in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
         Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, agent.stoppingDistance);
    }
}
