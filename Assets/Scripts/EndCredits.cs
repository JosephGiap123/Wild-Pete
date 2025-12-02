using UnityEngine;
using UnityEngine.UI;  // Use TMPro if using TextMeshPro

public class creditsScript : MonoBehaviour
{
    public float scrollSpeed = 20f; // Adjust the speed of the scroll
    private RectTransform rectTransform;

    void Start()
    {
        // Get the RectTransform component of the UI element
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        // Move the text upwards over time
        rectTransform.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);
    }
}