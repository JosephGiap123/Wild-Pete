using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    private TextMeshProUGUI text;
    private Rigidbody2D rb;

    void Awake()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 velocity, int damage, Color textColor, Color outlineColor)
    {
        if (text != null)
        {
            text.text = damage.ToString();
            text.color = textColor;
        }

        if (rb != null)
        {
            rb.linearVelocity = velocity;
        }

        // Destroy after 1 second
        Destroy(gameObject, 1f);
    }
}

