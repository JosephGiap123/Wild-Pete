using UnityEngine;

public class PlayerAnimRelayScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] PlayerMovement2D movementScript;
    [SerializeField] AttackHitbox hitboxScript;
    
    public void CallEndAttack(){
        movementScript.EndAttack();
    }

    public void CallRectHitbox(){
        hitboxScript.ActivateBox();
    }

    public void CallCircHitbox(){
        hitboxScript.ActivateCircle();
    }

    public void CallDisableHitbox(){
        hitboxScript.DisableHitbox();
    }

    public void CallSpawnBullet(){
        movementScript.InstBullet();
    }

}
