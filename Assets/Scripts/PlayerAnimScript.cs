using UnityEngine;

public class PlayerAnimScript : MonoBehaviour
{

    [SerializeField] Animator animator;
    private playerStates currentState;

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeAnimationState(playerStates newState)
    {
        if(newState == currentState) return;
        animator.Play(newState.ToString());
        currentState = newState;
    }
}
