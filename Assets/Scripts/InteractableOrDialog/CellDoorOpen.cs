using UnityEngine;

public class InteractableDoor : MonoBehaviour
{
    [SerializeField] private Sprite openDoorSprite;
    private BoxCollider2D doorHitBox;
    private SpriteRenderer spriteRenderer;

    void Start(){
        doorHitBox = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Open(){
        doorHitBox.enabled = false;
        spriteRenderer.sprite = openDoorSprite;
    }
}
