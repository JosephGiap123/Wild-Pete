using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [SerializeField] public List<TutorialSlide> tutorialSlides;
    [SerializeField] public TutorialSlide currentTutorial;
    public IntEventSO tutorialIndexEvent;
    public VoidEvents tutorialCompletedEvent;
    [SerializeField] private VoidEvents prisonCutsceneEndEvent;
    [SerializeField] private VoidEvents tutorialFullyCompletedEvent;

    void Awake()
    {
        //will destroy itself on loading a new scene.
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        tutorialCompletedEvent.onEventRaised.AddListener(OnTutorialCompleted);

        Instance = this;
    }

    void OnEnable()
    {
        if (prisonCutsceneEndEvent != null)
        {
            prisonCutsceneEndEvent.onEventRaised.AddListener(OnPrisonCutsceneEnd);
        }
    }

    void OnDisable()
    {
        if (prisonCutsceneEndEvent != null)
        {
            prisonCutsceneEndEvent.onEventRaised.RemoveListener(OnPrisonCutsceneEnd);
        }
    }



    public void StartTutorial()
    {
        if (tutorialSlides == null || tutorialSlides.Count == 0)
        {
            Debug.LogWarning("TutorialManager: No tutorial slides assigned!");
            return;
        }

        currentTutorial = tutorialSlides[0];
        if (currentTutorial != null)
        {
            currentTutorial.ActivateTutorial();
        }
        else
        {
            Debug.LogWarning("TutorialManager: First tutorial slide is null!");
        }
    }

    void OnTutorialCompleted()
    {
        int currentIndex = tutorialSlides.IndexOf(currentTutorial);
        if (currentIndex < 0)
        {
            Debug.LogWarning("TutorialManager: Current tutorial not found in list!");
            return;
        }
        Debug.Log(currentIndex);
        tutorialIndexEvent.RaiseEvent(currentIndex);

        // Don't raise fully completed event here - wait for NextTutorial or EndTutorial to handle it
    }

    public void NextTutorial()
    {
        if (tutorialSlides == null || tutorialSlides.Count == 0)
        {
            Debug.LogWarning("TutorialManager: No tutorial slides assigned!");
            return;
        }

        if (currentTutorial == null)
        {
            Debug.LogWarning("TutorialManager: Current tutorial is null!");
            return;
        }
        int currentIndex = tutorialSlides.IndexOf(currentTutorial);
        Debug.Log("Current tutorial: " + currentTutorial.name + " going to next tutorial" + tutorialSlides[currentIndex + 1].name);
        if (currentIndex < 0)
        {
            Debug.LogWarning("TutorialManager: Current tutorial not found in list!");
            return;
        }

        int nextIndex = currentIndex + 1;
        if (nextIndex >= tutorialSlides.Count - 1)
        {
            Debug.LogWarning("TutorialManager: No more tutorials available!");
            // This was the last tutorial, wait for it to fully complete before raising event
            StartCoroutine(RaiseTutorialFullyCompletedAfterDelay());
            return;
        }

        currentTutorial = tutorialSlides[nextIndex];
        if (currentTutorial != null)
        {
            currentTutorial.ActivateTutorial();
        }
        else
        {
            Debug.LogWarning($"TutorialManager: Tutorial slide at index {nextIndex} is null!");
        }
    }

    public void StartSpecificTutorial(TutorialSlide tutorial)
    {
        currentTutorial = tutorial;
        currentTutorial.ActivateTutorial();
    }

    public void EndTutorial()
    {
        // Check if this was the last tutorial before ending
        int currentIndex = tutorialSlides.IndexOf(currentTutorial);
        bool wasLastTutorial = currentIndex >= tutorialSlides.Count - 1;

        currentTutorial = null;

        // If this was the last tutorial, raise the fully completed event after a delay
        if (wasLastTutorial)
        {
            StartCoroutine(RaiseTutorialFullyCompletedAfterDelay());
        }
    }

    private IEnumerator RaiseTutorialFullyCompletedAfterDelay()
    {
        // Wait a frame to ensure the tutorial panel is deactivated
        yield return null;

        // Wait for the tutorial to fully finish displaying
        // This ensures timed tutorials have completed their full duration
        // and any animations/transitions have finished
        yield return new WaitForSeconds(1f);

        if (tutorialFullyCompletedEvent != null)
        {
            tutorialFullyCompletedEvent.RaiseEvent();
        }
    }

    private void OnPrisonCutsceneEnd()
    {
        StartCoroutine(ShowTutorialAfterDelay());
    }

    private IEnumerator ShowTutorialAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        StartTutorial();
    }
}
