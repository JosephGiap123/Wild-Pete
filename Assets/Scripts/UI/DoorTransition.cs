using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
public class DoorTransition : MonoBehaviour
{
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private VoidEvents onTransitionStart;
    [SerializeField] private GameObject doorTransitionCanvas;
    private CinemachineCamera cinemachineCam;
    private CinemachinePositionComposer positionComposer; // Cache the position composer
    private Vector3 originalDamping;
    private bool hasStoredDamping = false;

    void Awake()
    {
        doorTransitionCanvas.SetActive(false);
        onTransitionStart.onEventRaised.AddListener(StartTransition);
    }

    void OnDestroy()
    {
        onTransitionStart.onEventRaised.RemoveListener(StartTransition);
    }

    private void InitializeCamera()
    {
        if (cinemachineCam == null && GameManager.Instance != null)
        {
            cinemachineCam = GameManager.Instance.cinemachineCam;
            if (cinemachineCam != null)
            {
                positionComposer = cinemachineCam.GetComponent<CinemachinePositionComposer>();
            }
        }
    }

    public void StartTransition()
    {
        InitializeCamera();

        Debug.Log("StartTransition: CinemachineCamera: " + cinemachineCam + " PositionComposer: " + positionComposer);
        if (cinemachineCam == null || positionComposer == null)
        {
            Debug.LogWarning("DoorTransition: CinemachineCamera or CinemachinePositionComposer not found!");
            return;
        }

        doorTransitionCanvas.SetActive(true);
        doorAnimator.Play("StartTransition", 0);
        PauseController.SetPause(true);

        // Store original damping values BEFORE changing them
        if (!hasStoredDamping)
        {
            originalDamping = positionComposer.Damping;
            hasStoredDamping = true;
        }

        // Change damping to (1, 1, 1) for instant camera movement
        positionComposer.Damping = Vector3.one;

        StartCoroutine(TransitionCoroutine());
    }

    private IEnumerator TransitionCoroutine()
    {
        // Wait for transition duration (using unscaled time since game is paused)
        yield return new WaitForSecondsRealtime(0.5f);
        StartCoroutine(EndTransition());
    }

    public IEnumerator EndTransition()
    {
        Debug.Log("EndTransition: CinemachineCamera: " + cinemachineCam + " PositionComposer: " + positionComposer);
        // Reset damping back to original values
        if (positionComposer != null && hasStoredDamping)
        {
            positionComposer.Damping = originalDamping;
        }
        PauseController.SetPause(false);
        yield return new WaitForSecondsRealtime(0.1f);
        doorAnimator.Play("EndTransition", 0); //names are backwards in the doorAnimatorator


    }
}
