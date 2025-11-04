using UnityEngine;
using System.Collections;

public class AnimScript : MonoBehaviour
{
    public void ChangeAnimationState(playerStates state) { }
    public playerStates ReturnCurrentState() { return playerStates.Idle; }
    public IEnumerator HurtFlash(float duration) { yield return null; }
}

