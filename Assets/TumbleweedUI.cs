using UnityEngine;

public class UITumbleweed : MonoBehaviour
{
  public float moveSpeed = 150f;
  public float rotationSpeed = 200f;
  public Vector2 direction = new Vector2(-4f, -.67f);
  public float leftLimitX = -1000f;
  public float resetX = 1000f;

  private RectTransform rt;
  private Vector2 startPos;

  void Start()
  {
    rt = GetComponent<RectTransform>();
    direction.Normalize();

    // Save the original starting anchoredPosition
    startPos = rt.anchoredPosition;
  }

  void Update()
  {
    // Move along slope
    rt.anchoredPosition += direction * moveSpeed * Time.deltaTime;

    // Rotate like rolling
    rt.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);

    // Reset to original spot
    if (rt.anchoredPosition.x < leftLimitX)
    {
      rt.anchoredPosition = startPos;
    }
  }
}
