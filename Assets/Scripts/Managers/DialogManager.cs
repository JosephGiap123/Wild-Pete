using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance { get; private set; }

    [SerializeField] public GameObject dialoguePanel;
    [SerializeField] public TMP_Text dialogueText;
    [SerializeField] public TMP_Text nameText;
    [SerializeField] public Image portraitImage;

    //holds refs to the dialogue panel, text, name, and portrait for dialog npcs to use

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
}
