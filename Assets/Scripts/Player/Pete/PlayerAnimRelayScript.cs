using UnityEngine;

public class PeteAnimRelay : MonoBehaviour
{

    [SerializeField] PeteMovement2D movementScript;
    [SerializeField] GenericAttackHitbox hitboxScript;
    [SerializeField] PeteAudioManager audioMgr;

    public void CallEndAttack()
    {
        movementScript.EndAttack();
    }

    public void CallActivateHitbox()
    {
        hitboxScript.ActivateHitbox();
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

    public void InitDynamite()
    {
        movementScript.InitDynamite();
    }

    public void CallWalkSound()
    {
        // audioMgr.StopRunLoop();
        audioMgr.StartRunLoop();
    }

}
