using System.Collections;
using UnityEngine;

public class TutorialKeySetText : MonoBehaviour
{
    [SerializeField] protected PlayerControls inputCmd;
    [SerializeField] protected GameObject keyText;

    private void SetText(KeyCode mappedKeyCode)
    {
        Debug.Log(mappedKeyCode);
        Sprite keyCodeSprite = ControlManager.instance.spriteMapping[mappedKeyCode];
        keyText.GetComponent<SpriteRenderer>().sprite = keyCodeSprite;
        if (mappedKeyCode == KeyCode.Tab)
        {
            GetComponent<RectTransform>().sizeDelta = new Vector2(64f, 36f);
            keyText.GetComponent<RectTransform>().sizeDelta = new Vector2(36f, 36f);
        }
        else
        {
            GetComponent<RectTransform>().sizeDelta = new Vector2(36f, 36f);
            keyText.GetComponent<RectTransform>().sizeDelta = new Vector2(12f, 12f);
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
