using UnityEngine;
using UnityEngine.UI;

public class CubeHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public Slider healthBar;
    public GameObject gameOverText;
    private Vector3 originalPos;

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.value = currentHealth;
        gameOverText.SetActive(false);
        originalPos = transform.position;
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.name == "Cube")
        {
            TakeDamage(10);
        }
    }

    void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        currentHealth = Mathf.Max(currentHealth, 0);
        healthBar.value = currentHealth;

        if (currentHealth == 0)
        {
            gameOverText.SetActive(true);

            // Respawn after 2 seconds
            Invoke(nameof(Respawn), 2f);
        }
    }

    void Respawn()
    {
        transform.position = originalPos;
        currentHealth = maxHealth;
        healthBar.value = currentHealth;
        gameOverText.SetActive(false);
    }
}
