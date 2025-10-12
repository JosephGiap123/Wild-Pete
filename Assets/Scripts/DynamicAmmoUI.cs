using UnityEngine;

public class DynamicAmmoUI : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string ammoName;
    
    
    public void changeSprite(int n)
    {
        
        switch(n)
        {
            case 0:
                animator.Play($"{ammoName}Empty");
                break;
            case 1:
                animator.Play($"{ammoName}Full");
                break;
            default:
                break;
        }
    }
    
}