using UnityEngine;

// Ensure the GameObject has a Rigidbody component
[RequireComponent(typeof(Rigidbody))]
public class NewtonianPhysicsDemo : MonoBehaviour
{
    [Header("Physics Settings")]
    [Tooltip("The magnitude of the force applied with arrow keys.")]
    public float pushForce = 10f;

    [Tooltip("The magnitude of the upward force for jumping.")]
    public float jumpForce = 7f;

    [Tooltip("How the force is applied (Force accelerates over time, Impulse applies instantly).")]
    public ForceMode forceMode = ForceMode.Force; // Change to Impulse for sudden pushes

    // Reference to the Rigidbody component
    private Rigidbody rb;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Awake()
    {
        // Get the Rigidbody component attached to this GameObject
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody component not found on this GameObject!", this);
            this.enabled = false; // Disable script if no Rigidbody
            return;
        }

        // Store initial state for reset
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        Debug.Log("--- Newtonian Physics Demo ---");
        Debug.Log("1st Law (Inertia): Object at rest. Press arrow keys or Space to apply force.");
        Debug.Log("Rigidbody Mass: " + rb.mass); // Relevant for 2nd Law
    }

    void Update()
    {
        // --- Input Handling (Detecting the intent to apply force) ---
        // We apply the actual force in FixedUpdate for physics consistency.

        // Reset position and velocity
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetObject();
        }

        // Jump Input (Applies force upwards)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ApplyJumpForce(); // We'll call the Rigidbody method in FixedUpdate via a flag or direct call if needed, but simple jump is okay here for demo
             Debug.Log("Input: Space pressed - Attempting Jump (See FixedUpdate for actual force).");
        }

        // Movement Input (Handled in FixedUpdate)
        HandleMovementInput();
    }

    void FixedUpdate()
    {
        // --- Physics Calculations and Force Application ---
        // FixedUpdate runs at a fixed interval, making it ideal for physics.

        // --- 1st Law (Inertia) Demonstration ---
        // If no external forces are being applied via input below, and the object
        // is resting or moving, it will continue in that state due to inertia.
        // Gravity is acting, but if resting on the plane, the normal force balances it.
        // We log velocity to observe its state.
        // Debug.Log($"FixedUpdate - Velocity: {rb.velocity.magnitude:F2} m/s"); // Can be spammy

        // --- 2nd Law (F=ma) Demonstration ---
        // Apply forces based on input detected in Update.
        // The Rigidbody's mass influences the resulting acceleration.
        float horizontalInput = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float verticalInput = Input.GetAxis("Vertical");     // W/S or Up/Down

        if (Mathf.Abs(horizontalInput) > 0.01f || Mathf.Abs(verticalInput) > 0.01f)
        {
            Vector3 movementDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;
            Vector3 forceToApply = movementDirection * pushForce;

            rb.AddForce(forceToApply, forceMode);

            // Log the force being applied (demonstrates applying force)
            // Debug.Log($"Applying Force: {forceToApply} (Mode: {forceMode})");
        }

        // Log velocity change resulting from force (demonstrates acceleration)
        // You'll see velocity change when forces are applied.
    }

    // --- 3rd Law (Action-Reaction) Demonstration ---
    void OnCollisionEnter(Collision collision)
    {
        // This function is called by Unity when this collider/rigidbody
        // starts touching another rigidbody/collider.
        Debug.Log($"--- Collision! (3rd Law) ---");
        Debug.Log($"Action: {gameObject.name} collided with {collision.gameObject.name}.");

        // The physics engine automatically applies the reaction force.
        // If 'collision.gameObject' also has a Rigidbody, it will react to the collision.
        // If it's a static collider (like the default plane/wall), it provides resistance.
        ContactPoint contact = collision.contacts[0]; // Get the first contact point
        Debug.Log($"Reaction: An opposing force is applied at the contact point ({contact.point}). Magnitude depends on impact.");

        // You can get the impulse (force * time) of the collision
        Debug.Log($"Collision Impulse Magnitude: {collision.impulse.magnitude:F2}");
    }

    // --- Helper Methods ---

    void HandleMovementInput()
    {
         // In this setup, we read input in Update but apply force in FixedUpdate
         // based on the current axis values. This is a common pattern.
         // No extra code needed here as FixedUpdate reads Input.GetAxis directly.
    }

    void ApplyJumpForce()
    {
        // It's generally best practice to apply forces in FixedUpdate.
        // For a simple jump triggered by GetKeyDown, applying it directly here
        // or in FixedUpdate is often acceptable, though FixedUpdate is technically purer.
        // Let's apply it directly for simplicity in this demo.
         rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); // Impulse for sudden jump
         Debug.Log($"Action (2nd Law): Applied Jump Force: {Vector3.up * jumpForce} (Mode: Impulse)");
    }


    void ResetObject()
    {
        rb.velocity = Vector3.zero;         // Stop current motion
        rb.angularVelocity = Vector3.zero;  // Stop current rotation
        transform.position = initialPosition; // Reset position
        transform.rotation = initialRotation; // Reset rotation
        Debug.Log("--- Object Reset ---");
        Debug.Log("1st Law (Inertia): Object at rest. Press arrow keys or Space to apply force.");
    }
}
