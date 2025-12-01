using UnityEngine;

public class Bridge : MonoBehaviour
{
    public bool isUp = false;
    public Animator anim;
    public void Awake()
    {
        anim.Play(isUp ? "BridgeUp" : "BridgeDown");
    }

    public void RaiseBridge()
    {
        isUp = true;
        anim.Play("BridgeRise");
    }

    public void LowerBridge()
    {
        isUp = false;
        anim.Play("BridgeDown");
    }
}
