using System.Collections;
using System.Diagnostics;
using System.Numerics;
using UnityEngine;


public class TransformExamples: MonoBehaviour {

    [SerializeField]
    public Transform target;
    public float moveSpeed = 10f;
    public float rotationSpeed = 100f;
    public Transform targetPositionMarker;
    public float smoothTimePosition = 0.5f; // Time to reach target position
    public float smoothTimeRotation = 0.3f;
    private Vector3 velocity =vector3.zero;
    void Start() {
        Debug.Log("Initial World Position" + transform.position);
        Debug.Log("Initial Local Position" + transform.localposition);
    }

    void update() {
        transform.position = new Vector3(0f, 2f, 3f);
        Vector3 position = transform.position;
        position.X = 6f;
        transform.position = position;

        transform.translate(new Vector3(1f, 0f, 0f) * Time.deltaTime * moveSpeed);

        float horizontalInput = Input.getAxis("Horizontal");
        float verticalInput = Input.getAxis("Vertical");
        Vector3 movementDirection = new Vector3 (horizontalInput, 0, verticalInput).normalized;
        transform.Translate(movementDirection * moveSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Identity;
        transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        transform.Rotate(Vector3.up*rotationSpeed*time.deltaTime);

        transform.lookAt(target.position);
    if (targetPositionMarker != null)
    {
        // --- Smooth Position Lerp ---
        // Simple Lerp: Moves faster at the start, slows down near the end.
        // Good for quick transitions, but not constant speed.
        // float lerpSpeed = 3.0f;
        // transform.position = Vector3.Lerp(transform.position, targetPositionMarker.position, Time.deltaTime * lerpSpeed);

        // --- Smooth Position SmoothDamp ---
        // Creates a spring-damper like smooth movement. Often looks more natural.
        // Requires storing velocity between frames.
        transform.position = Vector3.SmoothDamp(transform.position, targetPositionMarker.position, ref velocityPos, smoothTimePosition);


        // --- Smooth Rotation Slerp ---
        // Smoothly interpolates between current rotation and target rotation
        Quaternion targetRotation = Quaternion.LookRotation(targetPositionMarker.position - transform.position); // Look at target
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime / smoothTimeRotation); // Adjust speed by dividing deltaTime
    }

    }

}