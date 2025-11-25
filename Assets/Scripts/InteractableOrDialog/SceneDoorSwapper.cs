using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneDoorSwapper : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemSO requiredItem = null;
    [SerializeField] private string sceneToSwapTo = "";
    public bool CanInteract()
    {
        return PlayerInventory.instance.HasItem(requiredItem.itemName) > 0;
    }

    public void Interact()
    {
        print("Interacted with SceneDoorSwapper");
        if (sceneToSwapTo == "" || sceneToSwapTo == null)
        {
            Debug.LogError("SceneDoorSwapper: No scene to swap to assigned!");
            return;
        }
        SceneManager.LoadScene(sceneToSwapTo);
    }
}
