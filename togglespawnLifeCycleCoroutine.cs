using UnityEngine;
using System.Collections; // Required for IEnumerator (Coroutines)

public class LifecycleAndTimingDemo : MonoBehaviour
{
    [Header("Object References")]
    [Tooltip("The Prefab to spawn (e.g., the Cube Prefab). Assign in Inspector.")]
    public GameObject objectPrefabToSpawn;

    [Tooltip("A target object in the scene to destroy. Assign the TargetCapsule.")]
    public GameObject objectToDestroy;

    [Tooltip("A component on this GameObject to toggle. Assign the Light component.")]
    public Light componentToToggle; // Assign the Point Light from PlayerSphere

    [Header("Spawning Settings")]
    [Tooltip("Where the new object should spawn relative to this object.")]
    public Vector3 spawnOffset = new Vector3(2f, 0f, 0f);

    [Header("Timing Settings")]
    public float invokeDelay = 2.0f;
    public float repeatInitialDelay = 1.0f;
    public float repeatInterval = 1.5f;
    public float coroutineWaitTime = 1.0f;

    // Internal reference for stopping coroutine
    private Coroutine runningCoroutine = null;
    private bool isRepeatingInvoked = false;

    void Start()
    {
        // Basic checks
        if (objectPrefabToSpawn == null)
            Debug.LogWarning("Object Prefab To Spawn is not assigned!");
        if (objectToDestroy == null)
            Debug.LogWarning("Object To Destroy is not assigned!");
        if (componentToToggle == null)
            Debug.LogWarning("Component To Toggle (Light) is not assigned!");

        Debug.Log("--- Lifecycle and Timing Demo ---");
        Debug.Log("G: Toggle GameObject Active (PlayerSphere)");
        Debug.Log("H: Toggle Component Enabled (Light on PlayerSphere)");
        Debug.Log("J: Destroy Target Object (TargetCapsule) after delay");
        Debug.Log("K: Instantiate (Spawn) Prefab (Cube)");
        Debug.Log("L: Invoke Delayed Method");
        Debug.Log("I: Invoke Repeating Method (Press C to Cancel)");
        Debug.Log("O: Start Coroutine Sequence");
        Debug.Log("P: Stop Running Coroutine");
        Debug.Log("---------------------------------");
    }

    void Update()
    {
        // --- 1. Enable/Disable GameObject ---
        if (Input.GetKeyDown(KeyCode.G))
        {
            // Toggle the active state of the GameObject this script is attached to
            bool currentActiveState = gameObject.activeSelf;
            gameObject.SetActive(!currentActiveState);
            Debug.Log($"GameObject Active Toggled: {!currentActiveState}");
            // Note: If you disable the GameObject, Update() will stop running
            // until it's re-enabled externally.
        }

        // --- 2. Enable/Disable Component ---
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (componentToToggle != null)
            {
                componentToToggle.enabled = !componentToToggle.enabled;
                Debug.Log($"Component '{componentToToggle.GetType().Name}' Enabled Toggled: {componentToToggle.enabled}");
            }
            else
            {
                Debug.LogWarning("Cannot toggle component - reference not set.");
            }
        }

        // --- 3. Destroy GameObject ---
        if (Input.GetKeyDown(KeyCode.J))
        {
            if (objectToDestroy != null)
            {
                Debug.Log($"Attempting to destroy '{objectToDestroy.name}' after {invokeDelay} seconds.");
                // Destroy the specified GameObject after a delay
                Destroy(objectToDestroy, invokeDelay);
                // Set the reference to null so we don't try again
                objectToDestroy = null;
            }
            else
            {
                Debug.LogWarning("Object to destroy is already destroyed or not assigned.");
            }
            // To destroy this object immediately: Destroy(gameObject);
            // To destroy a component: Destroy(GetComponent<Rigidbody>());
        }

        // --- 4. Instantiate (Spawn) GameObject ---
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (objectPrefabToSpawn != null)
            {
                Vector3 spawnPosition = transform.position + spawnOffset;
                Quaternion spawnRotation = Quaternion.identity; // No rotation
                Instantiate(objectPrefabToSpawn, spawnPosition, spawnRotation);
                Debug.Log($"Instantiated '{objectPrefabToSpawn.name}' at {spawnPosition}");
            }
            else
            {
                Debug.LogWarning("Cannot instantiate - Prefab not assigned.");
            }
        }

        // --- 5. Invoke ---
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log($"Invoking 'DelayedAction' in {invokeDelay} seconds.");
            Invoke("DelayedAction", invokeDelay);
            // Note: "DelayedAction" MUST be a method with void return type and no parameters.
        }

        // --- 6. InvokeRepeating ---
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (!isRepeatingInvoked)
            {
                Debug.Log($"Invoking 'RepeatingAction' starting in {repeatInitialDelay}s, repeating every {repeatInterval}s.");
                InvokeRepeating("RepeatingAction", repeatInitialDelay, repeatInterval);
                isRepeatingInvoked = true;
            } else {
                 Debug.Log("InvokeRepeating is already running. Press C to cancel.");
            }
        }

        // Cancel InvokeRepeating
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (isRepeatingInvoked)
            {
                CancelInvoke("RepeatingAction"); // Cancel specific repeating invoke
                // CancelInvoke(); // Cancels ALL invokes on this script
                isRepeatingInvoked = false;
                Debug.Log("Cancelled InvokeRepeating('RepeatingAction').");
            } else {
                 Debug.Log("No InvokeRepeating running to cancel.");
            }
        }

        // --- 7. Coroutines ---
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (runningCoroutine == null)
            {
                Debug.Log("Starting Coroutine 'MySequenceCoroutine'.");
                // Start the coroutine and store its reference
                runningCoroutine = StartCoroutine(MySequenceCoroutine());
                // Can also start by string: StartCoroutine("MySequenceCoroutine"); (less safe)
            }
            else
            {
                Debug.Log("Coroutine is already running. Press P to stop it first.");
            }
        }

        // Stop Coroutine
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (runningCoroutine != null)
            {
                Debug.Log("Stopping Coroutine 'MySequenceCoroutine'.");
                StopCoroutine(runningCoroutine);
                // Can also stop by string: StopCoroutine("MySequenceCoroutine");
                // StopAllCoroutines(); // Stops ALL coroutines on this script
                runningCoroutine = null; // Clear the reference
            }
            else
            {
                Debug.Log("No coroutine running to stop.");
            }
        }
    }

    // --- Methods for Invoke/InvokeRepeating ---

    void DelayedAction()
    {
        Debug.Log($"Invoke: 'DelayedAction' executed at {Time.time} seconds.");
        // Perform some action here, e.g., change color
        GetComponent<Renderer>().material.color = Random.ColorHSV();
    }

    void RepeatingAction()
    {
        Debug.Log($"InvokeRepeating: 'RepeatingAction' executed at {Time.time} seconds.");
        // Perform some repeating action, e.g., slightly move
        transform.Translate(Vector3.up * 0.1f);
    }

    // --- Coroutine Method ---

    IEnumerator MySequenceCoroutine()
    {
        Debug.Log("Coroutine: Started. Waiting for frame...");
        yield return null; // Wait for the next frame

        Debug.Log($"Coroutine: Waited one frame. Now waiting {coroutineWaitTime} seconds...");
        GetComponent<Renderer>().material.color = Color.yellow; // Change color

        yield return new WaitForSeconds(coroutineWaitTime); // Wait for specified time

        Debug.Log($"Coroutine: Waited {coroutineWaitTime} seconds. Changing color and waiting again...");
        GetComponent<Renderer>().material.color = Color.cyan;

        yield return new WaitForSeconds(coroutineWaitTime);

        Debug.Log("Coroutine: Finished sequence.");
        GetComponent<Renderer>().material.color = Color.white; // Reset color
        runningCoroutine = null; // Clear reference as it's finished
    }

    // Optional: Ensure coroutines/invokes are stopped if object is disabled/destroyed
    void OnDisable()
    {
        // Cancel invokes if the object is disabled
        CancelInvoke();
        isRepeatingInvoked = false; // Reset flag

        // Stop coroutines if the object is disabled
        if (runningCoroutine != null)
        {
            StopCoroutine(runningCoroutine);
            runningCoroutine = null;
            Debug.Log("Coroutine stopped because GameObject was disabled.");
        }
         Debug.Log("LifecycleAndTimingDemo script disabled.");
    }

     void OnEnable() {
         Debug.Log("LifecycleAndTimingDemo script enabled.");
         // You might want to restart certain things here if needed
     }

     void OnDestroy() {
         Debug.Log("LifecycleAndTimingDemo script is being destroyed.");
         // Cleanup happens automatically, but good place for final logs if needed.
     }
}
