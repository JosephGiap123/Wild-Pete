using UnityEngine;

public class PlayerAnimRelayScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] PlayerMovement2D movementScript;
    
    public void CallEndAttack(){
        movementScript.EndAttack();
    }
}
