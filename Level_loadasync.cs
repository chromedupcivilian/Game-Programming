using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Example: If using a Slider for progress

public class AsyncLevelLoader : MonoBehaviour
{
    public Animator animator;
    public float transitionTime = 1f;
    // Optional: UI elements for loading screen
    public GameObject loadingScreen; // Assign a parent object for loading UI
    public Slider progressBar;      // Assign a UI Slider

    public void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            StartCoroutine(LoadLevelAsync(nextSceneIndex));
        }
        else
        {
            Debug.LogWarning("End of scenes reached. Loading first scene.");
            StartCoroutine(LoadLevelAsync(0)); // Loop back
        }
    }

     // Overload to load by name (useful for buttons)
    public void LoadLevelByName(string sceneName)
    {
         StartCoroutine(LoadLevelAsync(sceneName));
    }

    IEnumerator LoadLevelAsync(int levelIndex)
    {
        yield return StartCoroutine(LoadSequence(() => SceneManager.LoadSceneAsync(levelIndex)));
    }

    IEnumerator LoadLevelAsync(string sceneName)
    {
         yield return StartCoroutine(LoadSequence(() => SceneManager.LoadSceneAsync(sceneName)));
    }

    // Generic loading sequence coroutine
    IEnumerator LoadSequence(System.Func<AsyncOperation> loadOperationCreator)
    {
         // 1. Activate loading screen UI (optional)
        if (loadingScreen != null) loadingScreen.SetActive(true);
        if (progressBar != null) progressBar.value = 0;

        // 2. Trigger transition animation (e.g., fade out)
        if (animator != null) animator.SetTrigger("Start");

        // 3. Wait for fade out animation
        yield return new WaitForSeconds(transitionTime);

        // 4. Start loading the scene asynchronously
        AsyncOperation asyncLoad = loadOperationCreator();

        // 5. Prevent the scene from activating immediately after loading
        asyncLoad.allowSceneActivation = false;

        // 6. Wait until the asynchronous scene fully loads
        // Update progress bar while loading
        while (asyncLoad.progress < 0.9f) // Loop until loading is almost complete
        {
            // Update progress bar (map 0->0.9 to 0->1)
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            if (progressBar != null) progressBar.value = progress;
            // Debug.Log("Loading progress: " + (progress * 100f) + "%");

            yield return null; // Wait for the next frame
        }

        // Loading is complete (progress is 0.9).
        // You could add a "Press Any Key" prompt here if desired.
        Debug.Log("Scene loaded. Ready for activation.");
        if (progressBar != null) progressBar.value = 1f; // Show 100%

        // Optional: Wait for a key press or another animation
        // yield return new WaitUntil(() => Input.anyKeyDown);

        // 7. Allow the scene to activate (completes the transition)
        asyncLoad.allowSceneActivation = true;

        // The scene switch will happen automatically now.
        // Loading screen deactivation would happen in the new scene's Start/Awake
        // or via DontDestroyOnLoad logic if the loader persists.
    }
}
