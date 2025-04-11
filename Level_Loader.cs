using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Combines Level Loading logic with a Singleton pattern
public class Level_Loader : MonoBehaviour
{
    // --- Singleton Pattern ---
    private static Level_Loader instance; // Private static instance

    // Public static accessor for the instance
    public static Level_Loader Instance
    {
        get
        {
            // Optional: Add a check here to find the instance if it's null
            // but the Awake pattern is generally preferred for initialization.
            if (instance == null)
            {
                Debug.LogError(
                    "Level_Loader instance is null. Ensure a Level_Loader " +
                        "object exists in the scene."
                );
            }
            return instance;
        }
    }

    void Awake()
    {
        // Basic Singleton Pattern Implementation
        if (instance == null)
        {
            instance = this; // Set the singleton instance
            DontDestroyOnLoad(gameObject); // Keep this GameObject alive across scenes
        }
        else if (instance != this)
        {
            // If another instance already exists, destroy this one
            Debug.LogWarning(
                "Duplicate Level_Loader instance found. Destroying this one."
            );
            Destroy(gameObject);
        }
    }
    // --- End Singleton Pattern ---

    public Animator animator; // For playing transition animations

    // Example Update: Consider moving trigger logic elsewhere (e.g., GameManager)
    // void Update()
    // {
    //     // Example: Load next level on Enter key press
    //     if (Input.GetKeyDown(KeyCode.Return))
    //     {
    //         LoadNextLevel();
    //     }
    // }

    // Loads the next scene in the build order
    public void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // Check if the next scene index is valid
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            StartCoroutine(LoadLevel(nextSceneIndex));
        }
        else
        {
            Debug.LogWarning(
                "Trying to load beyond the last scene in Build Settings! " +
                    "Loading first scene instead."
            );
            // Optional: Loop back to the first scene (index 0)
            StartCoroutine(LoadLevel(0));
        }
    }

    // Public method to load a specific level by its build index
    public void LoadSpecificLevel(int levelIndex)
    {
        if (
            levelIndex >= 0 &&
            levelIndex < SceneManager.sceneCountInBuildSettings
        )
        {
            StartCoroutine(LoadLevel(levelIndex));
        }
        else
        {
            Debug.LogError($"Invalid scene index requested: {levelIndex}");
        }
    }

    // Coroutine to handle the animated loading sequence
    IEnumerator LoadLevel(int levelIndex)
    {
        // 1. Trigger the "Start" animation (e.g., fade out)
        if (animator != null)
        {
            animator.SetTrigger("Start");
        }
        else
        {
            Debug.LogWarning("Level Loader Animator not assigned!");
        }

        // 2. Wait for the animation duration
        // NOTE: Using WaitForSeconds is simple but not robust.
        // Consider using Animation Events or waiting for an Animator state
        // for better synchronization with the actual animation length.
        yield return new WaitForSeconds(1); // Adjust this time to match animation

        // 3. Load the new scene
        SceneManager.LoadScene(levelIndex);

        // Note: Any "End" animation (e.g., fade in) would typically be
        // triggered by something in the newly loaded scene, or you might
        // need more complex logic if the Level_Loader itself handles it.
    }
}
