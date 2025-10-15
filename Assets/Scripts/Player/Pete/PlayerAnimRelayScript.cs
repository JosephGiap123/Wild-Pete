using UnityEngine;

public class PlayerAnimRelayScript : MonoBehaviour
{

    [SerializeField] PeteMovement2D movementScript;
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

    public void CallEndHurt(){
        movementScript.EndHurt();
    }

    public void CallEndReload(){
        movementScript.EndReload();
    }
}
