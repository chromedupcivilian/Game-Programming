using UnityEngine;
using TMPro; // Required for TextMeshPro

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
    public ForceMode forceMode = ForceMode.Force;

    [Header("UI Display")]
    [Tooltip("The TextMeshPro UI element to display the current law.")]
    public TextMeshProUGUI lawDisplayText; // Assign this in the Inspector!

    // Reference to the Rigidbody component
    private Rigidbody rb;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    // State tracking for UI text
    private bool justCollided = false;
    private float collisionDisplayTime = 1.5f; // How long to show collision text
    private float collisionTimer = 0f;
    private bool isApplyingInputForce = false; // Track if input force is active this frame

    // Threshold to consider the object 'at rest'
    private const float velocityRestThreshold = 0.1f;

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

        // Check if the UI text element is assigned
        if (lawDisplayText == null)
        {
            Debug.LogError("Law Display Text (TextMeshProUGUI) not assigned in the Inspector!", this);
            // Don't disable the whole script, just the UI part won't work
        }

        // Store initial state for reset
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        Debug.Log("--- Newtonian Physics Demo ---");
        Debug.Log("1st Law (Inertia): Object at rest. Press arrow keys or Space to apply force.");
        Debug.Log("Rigidbody Mass: " + rb.mass); // Relevant for 2nd Law

        UpdateLawText(); // Set initial text
    }

    void Update()
    {
        // --- Input Handling ---
        // Reset position and velocity
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetObject();
        }

        // Jump Input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ApplyJumpForce();
            // Force UI update for jump action (2nd Law)
            if (lawDisplayText != null)
            {
                 lawDisplayText.text = "Newton's 2nd Law: Applying jump force (F=ma)";
                 // Reset collision timer if jump interrupts it
                 collisionTimer = 0f;
                 justCollided = false;
            }
        }

        // Check movement input status for FixedUpdate
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        isApplyingInputForce = Mathf.Abs(horizontalInput) > 0.01f || Mathf.Abs(verticalInput) > 0.01f;

        // --- Collision Text Timer ---
        if (justCollided)
        {
            collisionTimer -= Time.deltaTime;
            if (collisionTimer <= 0)
            {
                justCollided = false;
                // Force UI update after collision message expires
                UpdateLawText();
            }
        }
    }

    void FixedUpdate()
    {
        // --- Physics Calculations and Force Application ---
        if (isApplyingInputForce)
        {
            float horizontalInput = Input.GetAxis("Horizontal"); // Read again for FixedUpdate timing
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 movementDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;
            Vector3 forceToApply = movementDirection * pushForce;
            rb.AddForce(forceToApply, forceMode);
        }

        // --- Update UI Text (if not showing collision message) ---
        if (!justCollided)
        {
            UpdateLawText();
        }
    }

    // --- 3rd Law (Action-Reaction) Demonstration ---
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"--- Collision! (3rd Law) ---");
        Debug.Log($"Action: {gameObject.name} collided with {collision.gameObject.name}.");
        ContactPoint contact = collision.contacts[0];
        Debug.Log($"Reaction: Opposing force applied at {contact.point}. Impulse: {collision.impulse.magnitude:F2}");

        if (lawDisplayText != null)
        {
            lawDisplayText.text = "Newton's 3rd Law: Collision (Action-Reaction!)";
            justCollided = true;
            collisionTimer = collisionDisplayTime; // Start timer to show message
        }
    }

    // --- Helper Methods ---

    void ApplyJumpForce()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        Debug.Log($"Action (2nd Law): Applied Jump Force: {Vector3.up * jumpForce} (Mode: Impulse)");
        // UI text updated in Update() where jump is detected
    }

    void ResetObject()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        justCollided = false; // Clear collision state
        collisionTimer = 0f;
        isApplyingInputForce = false; // Reset input state
        Debug.Log("--- Object Reset ---");
        UpdateLawText(); // Update text to resting state
    }

    // Central method to update the law text based on current state
    void UpdateLawText()
    {
        if (lawDisplayText == null || justCollided) // Don't update if null or showing collision
        {
            return;
        }

        bool isMoving = rb.velocity.magnitude > velocityRestThreshold;

        if (isApplyingInputForce) // Check the flag set in Update
        {
            lawDisplayText.text = "Newton's 2nd Law: Applying force (F=ma)";
        }
        else if (isMoving)
        {
            lawDisplayText.text = "Newton's 1st Law: Object in motion stays in motion (Inertia)";
        }
        else // Not applying force, not significantly moving
        {
            lawDisplayText.text = "Newton's 1st Law: Object at rest stays at rest (Inertia)";
        }
    }
}
