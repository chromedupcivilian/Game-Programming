using UnityEngine;

[RequireComponent(typeof(Rigidbody))] // Or Rigidbody2D
[RequireComponent(typeof(Collider))]  // Or Collider2D
[RequireComponent(typeof(Animator))]
public class InteractionAndAnimation : MonoBehaviour
{
    private Animator anim;
    private Rigidbody rb; // Example for getting speed

    // Define layer names for clarity and robustness
    private int playerLayer;
    private int enemyLayer;
    private int interactableLayer;

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>(); // Get Rigidbody if needed for speed calculation

        // Convert layer names to layer indices ONCE
        playerLayer = LayerMask.NameToLayer("PlayerLayer"); // Use the exact name you defined
        enemyLayer = LayerMask.NameToLayer("EnemyLayer");
        interactableLayer = LayerMask.NameToLayer("Interactable");

        if (anim == null)
        {
            Debug.LogError("Animator component not found!", this);
            this.enabled = false;
        }
    }

    void Update()
    {
        // --- Example: Setting Float for Blend Tree based on Speed ---
        if (rb != null)
        {
            // Calculate speed (using horizontal plane velocity magnitude)
            Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            float currentSpeed = horizontalVelocity.magnitude;

            // Set the "Speed" float parameter in the Animator
            anim.SetFloat("Speed", currentSpeed);

            // --- Example: Setting Bool based on Speed ---
            bool isCurrentlyWalking = currentSpeed > 0.1f; // Adjust threshold as needed
            anim.SetBool("IsWalking", isCurrentlyWalking);
        }

        // --- Example: Triggering Jump Animation ---
        if (Input.GetButtonDown("Jump")) // Uses Unity's Input Manager "Jump" button (default Space)
        {
            anim.SetTrigger("Jump");
        }
    }

    // --- Collision Detection (Requires non-trigger Colliders) ---
    void OnCollisionEnter(Collision collision)
    {
        GameObject otherObject = collision.gameObject; // Get the GameObject we collided with

        Debug.Log($"Collided with: {otherObject.name}, Tag: {otherObject.tag}, Layer: {LayerMask.LayerToName(otherObject.layer)}");

        // --- Check Tag ---
        if (otherObject.CompareTag("Obstacle"))
        {
            Debug.Log("Hit an Obstacle!");
            // Maybe play a stumble animation or sound
        }

        // --- Check Layer ---
        if (otherObject.layer == enemyLayer) // Compare using the stored layer index
        {
            Debug.Log("Collided with an Enemy Layer object!");
            anim.SetTrigger("TakeDamage"); // Trigger the damage animation
        }

        // --- Check Both Tag and Layer ---
        if (otherObject.CompareTag("DangerousGround") && otherObject.layer == interactableLayer)
        {
            Debug.Log("Collided with Dangerous Ground on Interactable Layer!");
            // Apply damage, trigger effect, etc.
        }
    }

    // --- Trigger Detection (Requires at least one Collider set to 'Is Trigger') ---
    void OnTriggerEnter(Collider other)
    {
        GameObject otherObject = other.gameObject; // Get the GameObject that entered the trigger

        Debug.Log($"Triggered by: {otherObject.name}, Tag: {otherObject.tag}, Layer: {LayerMask.LayerToName(otherObject.layer)}");

        // --- Check Tag ---
        if (otherObject.CompareTag("Collectable"))
        {
            Debug.Log("Collected an item!");
            // Add score, play sound, maybe trigger a "Collect" animation?
            Destroy(otherObject); // Destroy the collected item
        }

        // --- Check Layer ---
        if (otherObject.layer == playerLayer)
        {
            Debug.Log("Player entered my trigger zone!");
            // An enemy might use this to start chasing or attacking
            // Example: Set a boolean for an enemy AI state
            // anim.SetBool("PlayerInRange", true);
        }

         // --- Check Tag ---
        if (otherObject.CompareTag("DamageZone"))
        {
            Debug.Log("Entered a Damage Zone!");
            anim.SetTrigger("TakeDamage"); // Trigger the damage animation
        }
    }

    // Optional: Handle Trigger Exit
    void OnTriggerExit(Collider other)
    {
        GameObject otherObject = other.gameObject;

        // Example: If an enemy AI used OnTriggerEnter to detect the player
        if (otherObject.layer == playerLayer)
        {
            Debug.Log("Player exited my trigger zone!");
            // anim.SetBool("PlayerInRange", false);
        }
    }

    // Optional: OnCollisionStay / OnTriggerStay for continuous checks
    // void OnCollisionStay(Collision collision) { }
    // void OnTriggerStay(Collider other) { }
}
