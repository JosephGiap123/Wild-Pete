using UnityEngine;

public class PlayerAnimRelayScript : MonoBehaviour
{

    [SerializeField] PeteMovement2D movementScript;
    [SerializeField] AttackHitbox hitboxScript;
    [SerializeField] PeteAudioManager audioMgr;

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

    public void CallSpawnBullet()
    {
        movementScript.InstBullet();
    }

    public void CallEndHurt()
    {
        movementScript.EndHurt();
    }

    public void CallEndReload()
    {
        movementScript.EndReload();
    }

    public void CallKnifeSound()
    {
        audioMgr.PlayMelee();
    }
    public void CallPunchSound()
    {
        audioMgr.PlayPunch();
    }

    public void CallGunSound()
    {
        audioMgr.PlayRevolver();
    }

    public void CallReloadSound()
    {
        audioMgr.PlayReload();
    }
}
