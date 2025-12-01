using UnityEngine;

public class InteractableDoor : MonoBehaviour
{
    [SerializeField] private Sprite openDoorSprite;
    private Sprite originalSprite; // Store original sprite for restoration
    private BoxCollider2D doorHitBox;
    private SpriteRenderer spriteRenderer;

    void Start(){
        doorHitBox = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Store original sprite if not already stored
        if (originalSprite == null && spriteRenderer != null)
        {
            originalSprite = spriteRenderer.sprite;
        }
    }

    public void Open(){
        if (doorHitBox != null) doorHitBox.enabled = false;
        if (spriteRenderer != null && openDoorSprite != null) spriteRenderer.sprite = openDoorSprite;
    }
    
    public void Close(){
        if (doorHitBox != null) doorHitBox.enabled = true;
        if (spriteRenderer != null && originalSprite != null) spriteRenderer.sprite = originalSprite;
    }
}
