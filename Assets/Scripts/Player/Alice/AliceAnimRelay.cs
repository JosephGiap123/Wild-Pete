using UnityEngine;

public class AliceAnimRelayScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] AliceMovement2D movementScript;
    [SerializeField] AttackHitbox hitboxScript;

    public void CallEndAttack()
    {
        movementScript.EndAttack();
    }

    public void CallRectHitbox()
    {
        hitboxScript.ActivateBox();
    }

    public void CallCircHitbox()
    {
        hitboxScript.ActivateCircle();
    }

    public void CallDisableHitbox()
    {
        hitboxScript.DisableHitbox();
    }

    public void CallSpawnBullet(int num)
    {
        movementScript.InstBullet(num);
    }

    public void CallEndHurt()
    {
        movementScript.EndHurt();
    }

    public void CallEndReload()
    {
        movementScript.EndReload();
    }

}
