using UnityEngine;
using System.Collections.Generic;
public class CreditsScroll : MonoBehaviour
{
    [SerializeField] private List<RectTransform> children;
    public float scrollSpeed = 100f;

    void Update()
    {
        foreach (RectTransform child in children)
        {
            child.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);
            if (child.anchoredPosition.y > 1500)
            {
                children.Remove(child);
            }
        }
        if (children.Count == 0)
        {
            Debug.Log("CreditsScroll: All credits have been scrolled");
        }
    }
}
