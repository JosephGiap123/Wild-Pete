using UnityEngine;
using UnityEngine.Events;
[CreateAssetMenu(fileName = "SceneSwapEventSO", menuName = "Events/CallSwapSceneSO")]
public class SwapSceneEventSO : ScriptableObject
{
	public UnityEvent<string> onEventRaised;
	public void RaiseEvent(string sceneName)
	{
		onEventRaised?.Invoke(sceneName);
	}
}
