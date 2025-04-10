using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level_Loader : MonoBehaviour
{
    public Animator animator; // For playing transition animations

    void Update()
    {
        // Example: Load next level on Enter key press
        if (Input.GetKeyDown(KeyCode.Return))
        {
            LoadNextLevel();
        }
    }

    public void LoadNextLevel()
    {
        // Get the index of the currently active scene
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        // Calculate the index of the next scene
        int nextSceneIndex = currentSceneIndex + 1;

        // Check if the next scene index is valid (exists in Build Settings)
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            // Start the coroutine to handle the loading sequence
            StartCoroutine(LoadLevel(nextSceneIndex));
        }
        else
        {
            Debug.LogWarning("Trying to load beyond the last scene in Build Settings! Loading first scene instead.");
            // Optional: Loop back to the first scene (index 0)
            StartCoroutine(LoadLevel(0));
        }
    }

    // Coroutine to handle the loading sequence
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

        // 2. Wait for the animation duration (adjust time as needed)
        // NOTE: This is a fixed wait, not tied to actual load time!
        yield return new WaitForSeconds(1); // Assuming 1-second animation

        // 3. Load the new scene (Synchronously in this example)
        SceneManager.LoadScene(levelIndex);
    }
}
