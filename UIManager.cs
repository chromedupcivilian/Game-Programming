using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{

    public Button myButton;

    public ScrollRect scrollRect;

    public TextMeshProUGUI infoText;

    void Start()
    {

        if (myButton != null)
        {

            myButton.onClick.RemoveAllListeners();

            myButton.onClick.AddListener(OnButtonClick);
        }
        else
        {
            Debug.LogError("UIManager: 'myButton' is not assigned in the Inspector!");
        }

        if (scrollRect != null)
        {

            scrollRect.onValueChanged.AddListener(OnScrollChanged);
        }
        else
        {
            Debug.LogWarning("UIManager: 'scrollRect' is not assigned in the Inspector (optional).");
        }

        if (infoText != null)
        {
            infoText.text = "Press the button or scroll the view.";
        }
        else
        {
            Debug.LogError("UIManager: 'infoText' is not assigned in the Inspector!");
        }
    }

    void OnButtonClick()
    {
        Debug.Log("Button clicked!");

        if (infoText != null)
        {
            infoText.text = "You clicked the button!";
        }
    }

    void OnScrollChanged(Vector2 position)
    {

        Debug.Log("Scrolling... Position: " + position);

    }

    public void LoadNewScene(string sceneName)
    {

        SceneManager.LoadScene(sceneName);
    }
}