using UnityEngine;
using System.Collections.Generic;
using System;
public class TutorialSlide : MonoBehaviour
{
    private GameObject tutorialPanel;

    public enum TutorialType
    {
        PressKey,
        PressInput,
        Timed,
        Dismiss,
        ItemPickUp
    }

    public bool goToNextAfterComplete = false;
    public string itemNameToPickUp;
    public TutorialType tutorialType;
    [SerializeField] private List<KeyCode> originalListenForKeys;
    [SerializeField] private List<string> originalListenForInputs;

    public float tutorialTimeLimit = 5f;
    private float tutorialTime = 0f;

    private bool tutorialComplete = false;

    // Store original lists for reset
    private List<KeyCode> listenForKeys;
    private List<string> listenForInputs;

    public VoidEvents tutorialCompleteEvent;

    public InputEvent inputEvent;
    public ItemPickUpEvent itemPickUpEvent;


    void Awake()
    {
        // Set tutorialPanel in Awake so it's available even when GameObject is inactive
        tutorialPanel = this.gameObject;
    }

    public void OnEnable()
    {
        if (inputEvent != null)
        {
            Debug.Log("TutorialSlide " + name + ": Adding input event listener");
            inputEvent.onEventRaised.AddListener(OnInputUsed);
        }
        if (itemPickUpEvent != null)
        {
            Debug.Log("TutorialSlide " + name + ": Adding item pick up event listener");
            itemPickUpEvent.onEventRaised.AddListener(OnItemPickUp);
        }

        // Store original lists for reset
        listenForKeys = new List<KeyCode>(originalListenForKeys);
        listenForInputs = new List<string>(originalListenForInputs);
    }

    void OnDestroy()
    {
        if (inputEvent != null)
        {
            inputEvent.onEventRaised.RemoveListener(OnInputUsed);
        }
    }

    void OnInputUsed(string inputName, PlayerControls playerControls, KeyCode keyCode)
    {
        if (tutorialComplete || tutorialType != TutorialType.PressInput) return;

        if (listenForInputs.Contains(inputName))
        {
            listenForInputs.Remove(inputName);
        }
        if (listenForInputs.Count == 0)
        {
            CompleteTutorial();
        }
    }

    public void ActivateTutorial()
    {
        if (tutorialPanel == null)
        {
            tutorialPanel = this.gameObject;
        }

        // Reset tutorial state
        tutorialComplete = false;
        tutorialTime = 0f;

        // Restore original lists
        listenForKeys = new List<KeyCode>(originalListenForKeys);
        listenForInputs = new List<string>(originalListenForInputs);

        // Activate the panel
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
        }
    }

    void Update()
    {
        if (tutorialComplete) return;

        if (tutorialType == TutorialType.Timed)
        {
            tutorialTime += Time.deltaTime;
            if (tutorialTime >= tutorialTimeLimit)
            {
                //fire some event to say that this tutorial is complete
                CompleteTutorial();
            }
            return;
        }

        if (tutorialType == TutorialType.PressKey)
        {
            for (int i = listenForKeys.Count - 1; i >= 0; i--)
            {
                if (Input.GetKeyDown(listenForKeys[i]))
                {
                    listenForKeys.RemoveAt(i);
                    if (listenForKeys.Count == 0)
                    {
                        CompleteTutorial();
                    }
                }
            }
            return;
        }
    }
    void OnItemPickUp(Item item)
    {
        if (tutorialComplete || tutorialType != TutorialType.ItemPickUp) return;
        if (item.itemName == itemNameToPickUp)
        {
            CompleteTutorial();
        }
    }

    public void CompleteTutorial()
    {
        tutorialComplete = true;
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
        if (goToNextAfterComplete)
        {
            TutorialManager.Instance.NextTutorial();
        }
        else
        {
            TutorialManager.Instance.EndTutorial();
        }
        tutorialCompleteEvent.RaiseEvent();
    }
}
