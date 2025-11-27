using UnityEngine;
using TMPro;
public class SceneTransitionScreen : MonoBehaviour
{
    [SerializeField] private SwapSceneEventSO sceneSwapEventSO;
    [SerializeField] private GameObject thisScreen;
    [SerializeField] private Animator thisScreenAnimator;
    [SerializeField] private Animator imageAnimator;
    [SerializeField] private TMP_Text textMeshProText;
    void Awake()
    {
        thisScreen = this.gameObject;
        thisScreenAnimator = thisScreen.GetComponent<Animator>();
        imageAnimator = thisScreen.GetComponentInChildren<Animator>();
        thisScreen.SetActive(false);
        sceneSwapEventSO.onEventRaised.AddListener(OnSceneSwap);
        if (GameManager.Instance.selectedCharacter == GameManager.Characters.Pete)
        {
            imageAnimator.Play("pete");
        }
        else
        {
            imageAnimator.Play("alice");
        }
    }
    void OnDestroy()
    {
        sceneSwapEventSO.onEventRaised.RemoveListener(OnSceneSwap);
    }

    void OnSceneSwap(string sceneName)
    {
        thisScreen.SetActive(true);
        thisScreenAnimator.Play("fadein");
        if (GameManager.Instance.selectedCharacter == GameManager.Characters.Pete)
        {
            imageAnimator.Play("pete");
        }
        else
        {
            imageAnimator.Play("alice");
        }
        textMeshProText.text = "Now going to " + sceneName;
    }

    void CallFadeOut()
    {
        thisScreenAnimator.Play("fadeout");
    }

    public void CallPause()
    {
        PauseController.SetPause(true);
    }

    public void CallUnpause()
    {
        PauseController.SetPause(false);
    }

    public void HideScreen()
    {
        thisScreen.SetActive(false);
    }
}
