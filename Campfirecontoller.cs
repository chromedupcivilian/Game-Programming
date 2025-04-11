using UnityEngine;
using UnityEngine.UI; // If you need to change button text

public class CampfireController : MonoBehaviour
{
    [Tooltip("Assign the campfire particle system here.")]
    public ParticleSystem campfireParticles;

    [Tooltip("Assign the campfire point light here.")]
    public Light campfireLight;

    // Optional: Assign the button to change its text
    // public Button toggleButton;
    // public TextMeshProUGUI buttonText; // If using TMP button

    private bool isCampfireOn = true; // Assume starts on

    void Start()
    {
        // Ensure components are assigned
        if (campfireParticles == null || campfireLight == null)
        {
            Debug.LogError("Campfire Particle System or Light not assigned to CampfireController!", this);
            // Disable button interaction if setup is wrong?
            // if (toggleButton != null) toggleButton.interactable = false;
            return;
        }
        // Initialize state based on isCampfireOn
        SetCampfireState(isCampfireOn);
    }

    // This public method will be called by the UI Button's OnClick event
    public void ToggleCampfire()
    {
        isCampfireOn = !isCampfireOn; // Flip the state
        SetCampfireState(isCampfireOn);
    }

    private void SetCampfireState(bool isOn)
    {
        if (isOn)
        {
            if (!campfireParticles.isPlaying)
            {
                campfireParticles.Play(); // Start emitting particles
            }
            campfireLight.enabled = true; // Turn light on
            // Optional: Update button text
            // if (buttonText != null) buttonText.text = "Turn Off Campfire";
        }
        else
        {
            if (campfireParticles.isPlaying)
            {
                campfireParticles.Stop(); // Stop emitting new particles (existing ones fade out)
            }
            campfireLight.enabled = false; // Turn light off
            // Optional: Update button text
            // if (buttonText != null) buttonText.text = "Turn On Campfire";
        }
         Debug.Log("Campfire state set to: " + (isOn ? "ON" : "OFF"));
    }
}
