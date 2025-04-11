using UnityEngine;
using TMPro;

public class SpriteCollectorTMP : MonoBehaviour
{
    public TMP_Text pointsText;
    private int points = 0;

    void Start()
    {
        pointsText.text = "Points: 0";
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Collectible"))
        {
            Destroy(other.gameObject);
            points++;
            pointsText.text = "Points: " + points;
        }
    }
}
