using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [SerializeField] LayerMask enemyMask;
    [SerializeField] CircleCollider2D circleCol;
    [SerializeField] BoxCollider2D boxCol;
    private bool active = false;

    private void DisableAll(){
        if(circleCol != null) circleCol.enabled = false;
        if(boxCol != null) boxCol.enabled = false;
    } 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!active) return;

        if (((1 << other.gameObject.layer) & enemyMask) != 0) //bitshifting to find if sometihng is in said layer
        {
            Debug.Log($"Hit enemy");
        }
    }

    public void ChangeHitboxCircle(Vector2 localOffset, float radius){
        DisableAll();
        circleCol.offset = new Vector2(localOffset.x, localOffset.y);
        circleCol.radius = radius;
    }

    public void ChangeHitboxBox(Vector2 localOffset, Vector2 size){
        DisableAll();
        boxCol.offset = new Vector2(localOffset.x, localOffset.y);
        boxCol.size = size;
    }

    public void ActivateCircle(){
        active = true;
        circleCol.enabled = true;
    }

    public void ActivateBox(){
        active = true;
        boxCol.enabled = true;
    }

    public void DisableHitbox(){
        DisableAll();
        active = false;
    }
}
