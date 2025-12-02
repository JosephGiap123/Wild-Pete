using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
public class SceneDoorSwapper : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemSO requiredItem = null;
    [SerializeField] private string sceneToSwapTo = "";
    [SerializeField] private SwapSceneEventSO sceneSwapEventSO;

    public string InteractMessage()
    {
        return " to enter " + sceneToSwapTo;
    }
    public bool CanInteract()
    {
        if (requiredItem == null)
        {
            return true;
        }
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
        sceneSwapEventSO.RaiseEvent(sceneToSwapTo);
        StartCoroutine(SwapScene());
    }

    public IEnumerator SwapScene()
    {
        yield return new WaitForSecondsRealtime(1f);
        SceneManager.LoadScene(sceneToSwapTo);
    }
}
