using UnityEngine;
using UnityEngine.UI;

public class UIBirdFlap : MonoBehaviour
{
  public Sprite[] frames;
  public float fps = 10f;

  private Image img;
  private int index = 0;
  private float timer = 0f;

  void Start()
  {
    img = GetComponent<Image>();
  }

  void Update()
  {
    timer += Time.deltaTime;

    if (timer >= 1f / fps)
    {
      index = (index + 1) % frames.Length;
      img.sprite = frames[index];
      timer = 0f;
    }
  }
}
