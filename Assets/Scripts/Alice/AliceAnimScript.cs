using UnityEngine;

public class AliceAnimScript : MonoBehaviour
{

    [SerializeField] Animator animator;
    private playerStates currentState;

    public void ChangeAnimationState(playerStates newState)
    {
        if(newState == currentState) return;
        animator.Play(newState.ToString());
        currentState = newState;
    }

    public playerStates returnCurrentState(){
        return currentState;
    }
}
