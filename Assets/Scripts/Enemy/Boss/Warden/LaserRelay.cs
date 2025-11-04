using UnityEngine;

public class LaserRelay : MonoBehaviour
{
    [SerializeField] GroundLaserBeam laserScript;

    public void CallDestroy()
    {
        laserScript.DestroyLaser();
    }
}
