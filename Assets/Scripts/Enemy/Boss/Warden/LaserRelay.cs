using UnityEngine;

public class LaserRelay : MonoBehaviour
{
    [SerializeField] GroundLaserBeam laserScript;
    [SerializeField] private AudioSource audMgr;

    public void CallDestroy()
    {
        laserScript.DestroyLaser();
    }

    public void PlaySound()
    {
        audMgr.Play();
    }
}
