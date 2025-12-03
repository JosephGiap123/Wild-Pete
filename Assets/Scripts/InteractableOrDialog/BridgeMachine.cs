using UnityEngine;

public class BridgeMachine : MonoBehaviour, IInteractable
{

    public Bridge bridge;
    public Sprite brokenSprite;
    public Sprite repairedSprite;
    public ItemSO[] requiredItems; ///requires cog, wood and lever to repair bridge machine
    public bool isRepaired = false;
    public bool bridgeIsPulled = false;
    [Header("Audio")]
    [SerializeField] private AudioClip leverTurnClip;
    [SerializeField, Range(0f, 2f)] private float leverTurnVolume = 1f;
    [SerializeField] private AudioSource leverAudioSource;

    public void Awake()
    {
        GetComponent<SpriteRenderer>().sprite = brokenSprite;
        isRepaired = false;
        bridgeIsPulled = false;

        if (!leverAudioSource) leverAudioSource = GetComponent<AudioSource>();
        if (!leverAudioSource) leverAudioSource = gameObject.AddComponent<AudioSource>();
        leverAudioSource.playOnAwake = false;
        leverAudioSource.spatialBlend = 1f;
        leverAudioSource.rolloffMode = AudioRolloffMode.Linear;
    }
    public void Interact()
    {
        if (!isRepaired)
        {
            bool hasItems = true;
            for (int i = 0; i < requiredItems.Length; i++)
            {
                if (PlayerInventory.instance.HasItem(requiredItems[i].itemName) <= 0)
                {
                    hasItems = false;
                    break;
                }
            }
            if (hasItems)
            {
                isRepaired = true;
                GetComponent<SpriteRenderer>().sprite = repairedSprite;
                for (int i = 0; i < requiredItems.Length; i++)
                {
                    PlayerInventory.instance.UseItem(requiredItems[i].itemName, 1);
                }
                return;
            }
        }
        else
        {
            PlayLeverSound();
            bridge.RaiseBridge();
            bridgeIsPulled = true;
        }
    }

    public bool CanInteract()
    {
        if (!isRepaired)
        {
            bool hasItems = true;
            for (int i = 0; i < requiredItems.Length; i++)
            {
                if (PlayerInventory.instance.HasItem(requiredItems[i].itemName) <= 0)
                {
                    hasItems = false;
                    break;
                }
            }
            return hasItems;
        }
        else
        {
            return !bridgeIsPulled;
        }
    }

    public string InteractMessage()
    {
        if (!isRepaired)
        {
            return " to repair the bridge machine";
        }
        else
        {
            return " to pull the bridge";
        }
    }

    private void PlayLeverSound()
    {
        if (leverTurnClip == null || leverAudioSource == null) return;
        leverAudioSource.PlayOneShot(leverTurnClip, leverTurnVolume);
    }
}
