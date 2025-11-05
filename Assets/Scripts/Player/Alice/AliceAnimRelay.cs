using UnityEngine;

public class AliceAnimRelayScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] AliceMovement2D movementScript;
    [SerializeField] AttackHitbox hitboxScript;
    [SerializeField] AliceAudioManager audioMgr;

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

    public void CallStartHyperArmor()
    {
        movementScript.StartHyperArmor();
    }

    public void CallEndHyperArmor()
    {
        movementScript.EndHyperArmor();
    }

    public void CallHammerSound()
    {
        audioMgr.PlayHammer();
    }
    public void CallCrouchAttackSound()
    {
        audioMgr.PlaySweep();
    }

    public void CallGunSound()
    {
        audioMgr.PlayShotgun();
    }

    public void CallReloadSound()
    {
        audioMgr.PlayReload();
    }

    public void InitDynamite()
    {
        movementScript.InitDynamite();
    }

}
