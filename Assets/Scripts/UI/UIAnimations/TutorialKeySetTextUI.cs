using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class TutorialKeySetTextUI : MonoBehaviour
{
    [SerializeField] protected PlayerControls inputCmd;
    [SerializeField] protected Image keyText;

    private void SetText(KeyCode mappedKeyCode)
    {
        Debug.Log(mappedKeyCode);
        Sprite keyCodeSprite = ControlManager.instance.spriteMapping[mappedKeyCode];
        keyText.sprite = keyCodeSprite;

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
        ControlManager.instance.ChangedInput += ChangeInputSprite;
        Debug.Log("found control manager");
        KeyCode mappedKeyCode = ControlManager.instance.inputMapping[inputCmd];
        SetText(mappedKeyCode);
    }

    public void OnDisable()
    {
        ControlManager.instance.ChangedInput -= ChangeInputSprite;
    }

    protected void ChangeInputSprite(PlayerControls inputName, KeyCode newKeyCode)
    {
        if (inputName == inputCmd)
        {
            SetText(newKeyCode);
        }
    }
}
