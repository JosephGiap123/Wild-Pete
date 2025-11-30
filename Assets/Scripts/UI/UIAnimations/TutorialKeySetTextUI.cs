using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class TutorialKeySetTextUI : MonoBehaviour
{
    [SerializeField] protected PlayerControls inputCmd;
    [SerializeField] protected Image keyText;
    [SerializeField] private InputEvent inputEvent;
    private void SetText(KeyCode mappedKeyCode)
    {
        Sprite keyCodeSprite = ControlManager.instance.spriteMapping[mappedKeyCode];
        keyText.sprite = keyCodeSprite;
        if (mappedKeyCode == KeyCode.Tab)
        {
            keyText.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(64f, 36f);
            keyText.rectTransform.sizeDelta = new Vector2(36f, 12f);
        }
        else
        {
            keyText.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(36f, 36f);
            keyText.rectTransform.sizeDelta = new Vector2(12f, 12f);
        }

    }
    public void Initialize(PlayerControls inputName)
    {
        inputCmd = inputName;
        KeyCode mappedKeyCode = ControlManager.instance.inputMapping[inputCmd];
        SetText(mappedKeyCode);
    }

    public void OnEnable()
    {
        StartCoroutine(WaitForControlManager());
    }

    private IEnumerator WaitForControlManager()
    {
        yield return new WaitWhile(() => !ControlManager.instance);
        inputEvent.onEventRaised.AddListener(ChangeInputSprite);
        Debug.Log("found control manager");
        KeyCode mappedKeyCode = ControlManager.instance.inputMapping[inputCmd];
        SetText(mappedKeyCode);
    }

    public void OnDisable()
    {
        inputEvent.onEventRaised.RemoveListener(ChangeInputSprite);
    }

    protected void ChangeInputSprite(string inpStringName, PlayerControls inputName, KeyCode newKeyCode)
    {
        if (inputName == inputCmd)
        {
            SetText(newKeyCode);
        }
    }
}
