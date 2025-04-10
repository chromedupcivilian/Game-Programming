using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The object the camera will follow.")]
    public Transform player; // Assign your Player GameObject's Transform here

    [Header("Settings")]
    [Tooltip("How far the camera is positioned from the player.")]
    public Vector3 offset;

    [Tooltip("How fast the mouse rotates the view.")]
    public float mouseSensitivity = 200f;

    [Tooltip("How far up/down the camera can look (degrees).")]
    public float verticalRotationLimit = 80f;

    [Tooltip("Optional: Smooth out camera movement.")]
    public float positionSmoothTime = 0.1f;
    [Tooltip("Optional: Smooth out camera rotation.")]
    public float rotationSmoothTime = 0.05f; // Shorter for responsiveness

    // Internal variables
    private float xRotation = 0f; // Stores vertical rotation
    private float yRotation = 0f; // Stores horizontal rotation (used in free look)

    // Variables for SmoothDamp (optional smoothing)
    private Vector3 currentPositionVelocity;
    private float currentYRotationVelocity;
    private float currentXRotationVelocity;


    void Start()
    {
        // Lock cursor to the center of the screen and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (player == null)
        {
            Debug.LogError("Player Transform not assigned to ThirdPersonCamera script!", this);
            this.enabled = false; // Disable script if no player
            return;
        }
        
        offset = player.position - transform.positon;

        yRotation = transform.eulerAngles.y;
        xRotation = transform.eulerAngles.x;
    }

    // LateUpdate runs after all Update calls, ideal for cameras
    // to ensure the player has finished moving for the frame.
    void LateUpdate()
    {
        if (player == null) return; // Don't run if player is missing

        // --- Input ---
        // Get mouse movement (scaled by sensitivity and time)
        // Using Time.deltaTime makes rotation speed independent of frame rate
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // --- Rotation Calculation ---
        // Vertical rotation (Pitch) - Always applied to the camera
        xRotation -= mouseY; // Subtract because Mouse Y is inverted (up = negative)
        xRotation = Mathf.Clamp(xRotation, -verticalRotationLimit, verticalRotationLimit);

        // Horizontal rotation (Yaw) - Applied differently based on Alt key
        bool isFreeLook = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

        Quaternion targetCameraRotation;
        Vector3 targetPosition;

        if (isFreeLook)
        {
            // --- Free Look Mode (Alt Pressed) ---
            // Accumulate horizontal rotation for the camera
            yRotation += mouseX;

            // Calculate the desired camera rotation
            targetCameraRotation = Quaternion.Euler(xRotation, yRotation, 0f);

            // Calculate desired position based on player position + offset rotated by CAMERA's new rotation
            targetPosition = player.position + targetCameraRotation * offset;
        }
        else
        {
            // --- Normal Follow Mode (Alt Not Pressed) ---
            // Rotate the PLAYER horizontally
            player.Rotate(Vector3.up * mouseX);

            // Calculate the desired camera rotation:
            // Match player's horizontal rotation, apply camera's vertical tilt
            targetCameraRotation = Quaternion.Euler(xRotation, player.eulerAngles.y, 0f);

            // Calculate desired position based on player position + offset rotated by PLAYER's rotation
            targetPosition = player.position + player.rotation * offset; // Use player's rotation here
        }


        // --- Apply Position & Rotation ---

        // Optional Smoothing (Lerp or SmoothDamp)
        if (positionSmoothTime > 0f)
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentPositionVelocity, positionSmoothTime);
        }
        else
        {
            // Direct application (no smoothing)
            transform.position = targetPosition;
        }

        if (rotationSmoothTime > 0f)
        {
             // SmoothDamp for Quaternions is tricky, Slerp is often easier for rotation
             transform.rotation = Quaternion.Slerp(transform.rotation, targetCameraRotation, Time.deltaTime / rotationSmoothTime); // Adjust speed by dividing deltaTime
             // Note: Slerp approach might feel slightly different than direct setting or true SmoothDamp
        }
        else
        {
             // Direct application (no smoothing)
             transform.rotation = targetCameraRotation;
        }


        // --- Alternative Rotation Smoothing using SmoothDamp (More complex) ---
        /*
        if (rotationSmoothTime > 0f && !isFreeLook) // Example for normal mode smoothing
        {
            // SmoothDamp Euler angles (can have issues crossing 0/360 boundaries)
            float smoothY = Mathf.SmoothDampAngle(transform.eulerAngles.y, player.eulerAngles.y, ref currentYRotationVelocity, rotationSmoothTime);
            float smoothX = Mathf.SmoothDampAngle(transform.eulerAngles.x, xRotation, ref currentXRotationVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(smoothX, smoothY, 0f);
        }
        else if (rotationSmoothTime > 0f && isFreeLook)
        {
             // Similar smoothing for free look angles
             float smoothY = Mathf.SmoothDampAngle(transform.eulerAngles.y, yRotation, ref currentYRotationVelocity, rotationSmoothTime);
             float smoothX = Mathf.SmoothDampAngle(transform.eulerAngles.x, xRotation, ref currentXRotationVelocity, rotationSmoothTime);
             transform.rotation = Quaternion.Euler(smoothX, smoothY, 0f);
        }
        else
        {
            // Direct application (no smoothing)
            transform.rotation = targetCameraRotation;
        }
        */
    }
}
