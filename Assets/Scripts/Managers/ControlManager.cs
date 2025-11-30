using UnityEngine;
using System.Collections.Generic;
using System;

public enum PlayerControls
{
    Melee,
    Reload,
    Ranged,
    Inventory,
    Dash,
    Unequip,
    Setting,
    Throw,
    Interact,
    Up,
    Down,
    Left,
    Right,
    Hotkey1,
    Hotkey2

}
public class ControlManager : MonoBehaviour
{
    public Dictionary<PlayerControls, KeyCode> inputMapping;
    public Dictionary<KeyCode, Sprite> spriteMapping;
    [SerializeField] public List<KeyCode> keyCodeConnection;
    [SerializeField] public List<Sprite> keyCodeTextSprite;
    public static ControlManager instance;

    [SerializeField] private InputEvent controlChangedEventSO;

    // public event Action<PlayerControls, KeyCode> ChangedInput;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeInputMapping();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeInputMapping()
    {
        // Initialize the dictionary
        inputMapping = new Dictionary<PlayerControls, KeyCode>
                {
					// Add default key mappings
					{ PlayerControls.Melee, KeyCode.E },
                    { PlayerControls.Reload, KeyCode.R },
                    { PlayerControls.Ranged, KeyCode.F },
                    { PlayerControls.Inventory, KeyCode.Tab },
                    { PlayerControls.Dash, KeyCode.Q },
                    { PlayerControls.Unequip, KeyCode.Z },
                    { PlayerControls.Setting, KeyCode.Y },
                    { PlayerControls.Throw, KeyCode.X },
                    { PlayerControls.Interact, KeyCode.I },
                    { PlayerControls.Up, KeyCode.W },
                    { PlayerControls.Down, KeyCode.S },
                    { PlayerControls.Left, KeyCode.A },
                    { PlayerControls.Right, KeyCode.D },
                    { PlayerControls.Hotkey1, KeyCode.C},
                    { PlayerControls.Hotkey2, KeyCode.V},
                };
        spriteMapping = new Dictionary<KeyCode, Sprite>();
        for (int i = 0; i < keyCodeConnection.Count; i++)
        {
            spriteMapping.Add(keyCodeConnection[i], keyCodeTextSprite[i]);
        }

    }

    public void ChangeInput(PlayerControls input, KeyCode changedKeyCode)
    {
        if (CheckInputExists(changedKeyCode))
        {
            inputMapping[input] = changedKeyCode;
            // ChangedInput?.Invoke(input, changedKeyCode);
            controlChangedEventSO.RaiseEvent(input.ToString(), input, changedKeyCode);
            return;
        }
        Debug.Log("Input already exists!");
        return;
    }

    public bool CheckInputExists(KeyCode changedKey)
    {
        List<KeyCode> existingInputs = new List<KeyCode> { KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow };
        foreach (KeyValuePair<PlayerControls, KeyCode> entry in inputMapping)
        {
            existingInputs.Add(entry.Value);
        }
        if (existingInputs.Contains(changedKey))
        {
            return false;
        }
        return true;
    }
}
