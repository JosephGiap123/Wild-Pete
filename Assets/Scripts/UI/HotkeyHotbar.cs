using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
public class HotkeyHotbar : MonoBehaviour
{
    [SerializeField] private GameObject hotkeyHotbar;
    [SerializeField] private GameObject[] hotKeyPanel;
    // [SerializeField] private GameObject[] hotKeyIcon;
    [SerializeField] private TMP_Text[] hotKeyText;
    [SerializeField] private TMP_Text[] hotKeyButtonText;
    //assume im the goat and all of the arrays are same length (3) and match

    [SerializeField] private InputEvent controlChangedEventSO;
    [SerializeField] private VoidEvents inventoryChangedEventSO;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (controlChangedEventSO != null)
            controlChangedEventSO.onEventRaised.AddListener(UpdateHotkeyHotbar);
        if (inventoryChangedEventSO != null)
            inventoryChangedEventSO.onEventRaised.AddListener(UpdateInventoryHotkeyText);
        UpdateAllHotkeyText();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (controlChangedEventSO != null)
            controlChangedEventSO.onEventRaised.RemoveListener(UpdateHotkeyHotbar);
        if (inventoryChangedEventSO != null)
            inventoryChangedEventSO.onEventRaised.RemoveListener(UpdateInventoryHotkeyText);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.Contains("Menu"))
        {
            hotkeyHotbar.SetActive(false);
        }
        else
        {
            hotkeyHotbar.SetActive(true);
            UpdateAllHotkeyText();
        }
    }
    void UpdateAllHotkeyText()
    {
        if (ControlManager.instance == null || PlayerInventory.instance == null)
        {
            Debug.LogWarning("HotkeyHotbar: ControlManager or PlayerInventory instance is null!");
            return;
        }

        if (hotKeyButtonText == null || hotKeyButtonText.Length < 3)
        {
            Debug.LogWarning("HotkeyHotbar: hotKeyButtonText array is null or too short!");
            return;
        }

        if (hotKeyText == null || hotKeyText.Length < 3)
        {
            Debug.LogWarning("HotkeyHotbar: hotKeyText array is null or too short!");
            return;
        }

        if (hotKeyButtonText[0] != null)
            hotKeyButtonText[0].text = ControlManager.instance.inputMapping[PlayerControls.Throw].ToString();
        if (hotKeyButtonText[1] != null)
            hotKeyButtonText[1].text = ControlManager.instance.inputMapping[PlayerControls.Hotkey1].ToString();
        if (hotKeyButtonText[2] != null)
            hotKeyButtonText[2].text = ControlManager.instance.inputMapping[PlayerControls.Hotkey2].ToString();

        if (hotKeyText[0] != null)
            hotKeyText[0].text = PlayerInventory.instance.HasItem("Dynamite").ToString();
        if (hotKeyText[1] != null)
            hotKeyText[1].text = PlayerInventory.instance.HasItem("Bandaid").ToString();
        if (hotKeyText[2] != null)
            hotKeyText[2].text = PlayerInventory.instance.HasItem("Medkit").ToString();
    }
    void UpdateHotkeyHotbar(string inpStringName, PlayerControls inputName, KeyCode newKeyCode)
    {
        if (inputName == PlayerControls.Throw)
        {
            hotKeyButtonText[0].text = newKeyCode.ToString();
        }
        else if (inputName == PlayerControls.Hotkey1)
        {
            hotKeyButtonText[1].text = newKeyCode.ToString();
        }
        else if (inputName == PlayerControls.Hotkey2)
        {
            hotKeyButtonText[2].text = newKeyCode.ToString();
        }
        for (int i = 0; i < hotKeyPanel.Length; i++)
        {
            hotKeyPanel[i].SetActive(true);
        }
    }

    void UpdateInventoryHotkeyText()
    {
        if (hotKeyText == null || hotKeyText.Length != 3)
        {
            Debug.LogWarning("HotkeyHotbar: hotKeyText array is null or too short!");
            return;
        }

        if (PlayerInventory.instance == null)
        {
            Debug.LogWarning("HotkeyHotbar: PlayerInventory.instance is null!");
            return;
        }

        if (hotKeyText[0] != null)
            hotKeyText[0].text = PlayerInventory.instance.HasItem("Dynamite").ToString();
        if (hotKeyText[1] != null)
            hotKeyText[1].text = PlayerInventory.instance.HasItem("Bandaid").ToString();
        if (hotKeyText[2] != null)
            hotKeyText[2].text = PlayerInventory.instance.HasItem("Medkit").ToString();
    }



}
