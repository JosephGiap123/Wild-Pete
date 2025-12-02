using UnityEngine;

public class SpiderAnimRelay : MonoBehaviour
{
    [SerializeField] SpiderAI spiderAI;
    [SerializeField] GenericAttackHitbox hitboxScript;

    [SerializeField] SpiderAudioManager audioMgr;

    public void CallEndAttack()
    {
        spiderAI.EndAttack();
    }
    public void CallBoxHitbox()
    {
        hitboxScript.ActivateHitbox();
    }
    public void CallDisableHitbox()
    {
        hitboxScript.DisableHitbox();
    }

    public void CallSpawnBullet()
    {
        spiderAI.InstBullet();
    }

    public void CallEndHurt()
    {
        spiderAI.EndHurtState();
    }

    public void CallDashVelocityIncrease()
    {
        spiderAI.DashVelocityIncrease();
    }

    public void CallEndDash()
    {
        spiderAI.StopMoving();
    }
    public void CallLungeSound()
    {
        audioMgr.PlayLunge();
    }
    public void CallSwipeSound()
    {
        audioMgr.PlaySwipe();
    }
    public void CallWebShotSound()
    {
        audioMgr.PlayWebShot();
    }
    public void CallHurtSound()
    {
        audioMgr.PlayHurt();
    }
    public void CallDeathSound()
    {
        audioMgr.PlayDeath();
    }
    public void CallWalkSound()
    {
        audioMgr.StopCrawlLoop();
        audioMgr.StartCrawlLoop();
    }
}
