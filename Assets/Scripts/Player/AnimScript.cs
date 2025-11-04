using System.Collections;
using UnityEngine;

public class AnimScript : MonoBehaviour
{

    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer sprite;
    private playerStates currentState;

    public void ChangeAnimationState(playerStates newState)
    {
        if (newState == currentState) return;
        animator.Play(newState.ToString());
        currentState = newState;
    }

    public playerStates ReturnCurrentState()
    {
        return currentState;
    }

    public IEnumerator HurtFlash(float duration)
    {
        sprite.material.SetFloat("_FlashAmount", 1f);
        yield return new WaitForSeconds(duration);
        sprite.material.SetFloat("_FlashAmount", 0f);
    }
}
