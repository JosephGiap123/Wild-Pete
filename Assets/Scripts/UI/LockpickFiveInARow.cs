using UnityEngine;
using UnityEngine.UI;
using TMPro; // If you don't use TextMeshPro, remove this line and TMP fields.

public class LockpickFiveInARow : MonoBehaviour
{
    public static bool IsLockpickActive { get; private set; }
    [Header("UI References")]
    [SerializeField] private Image targetArc;          // Image: Type=Filled, Method=Radial360
    [SerializeField] private RectTransform needle;     // Pivot=(0.5,0.0)
    [SerializeField] private TextMeshProUGUI keyText;  // Optional
    [SerializeField] private TextMeshProUGUI roundText;// Optional
    [SerializeField] private TextMeshProUGUI missText;
    [SerializeField] private LockPickAudioManager audioManager;


    [Header("Input")]
    [SerializeField] private KeyCode hitKey = KeyCode.E;
    [SerializeField] private KeyCode cancelKey = KeyCode.Escape;

    [Header("Rounds")]
    [SerializeField] private int roundsRequired = 5;
    [SerializeField] private float[] sliceSizesDeg = { 180f, 130f, 100f, 70f, 50f };   // smaller each round
    [SerializeField] private float[] speedsDegPerSec = { 180f, 240f, 300f, 360f, 440f }; // faster each round

    public System.Action<bool> OnComplete; // true = success

    private int roundIndex;
    private float sliceCenterDeg;
    private float sliceSize;
    private float spinSpeed;
    private float needleDeg;
    private bool playing;
    private float inputLockout;

    void Awake()
    {
        IsLockpickActive = true;
        PauseController.SetPause(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (!audioManager)
        {
            audioManager = GetComponent<LockPickAudioManager>();
        }
        if (!audioManager)
        {
            audioManager = GetComponentInChildren<LockPickAudioManager>();
        }
        if (!audioManager)
        {
#if UNITY_2022_1_OR_NEWER
            audioManager = FindAnyObjectByType<LockPickAudioManager>(FindObjectsInactive.Include);
#else
            audioManager = FindObjectOfType<LockPickAudioManager>(true);
#endif
        }
        if (!audioManager)
        {
            Debug.LogWarning("LockpickFiveInARow: No LockPickAudioManager found in scene. Assign one in the prefab or scene to hear lockpick SFX.");
        }

        StartRound(0);
        playing = true;
    }

    void OnDestroy()
    {
        IsLockpickActive = false;
        if (PauseController.IsGamePaused)
            PauseController.SetPause(false);
    }

    void Update()
    {
        if (!playing) return;

        // Rotate needle
        needleDeg = (needleDeg + spinSpeed * Time.unscaledDeltaTime) % 360f;
        if (needle) needle.localEulerAngles = new Vector3(0, 0, -needleDeg);

        if (inputLockout > 0f) inputLockout -= Time.unscaledDeltaTime;

        if (Input.GetKeyDown(cancelKey))
        {
            Finish(false);
            return;
        }

        if (inputLockout <= 0f && Input.GetKeyDown(hitKey))
        {
            inputLockout = 0.12f;
            TryHit();
        }
    }

    void StartRound(int idx)
    {
        roundIndex = Mathf.Clamp(idx, 0, roundsRequired - 1);

        sliceSize = sliceSizesDeg[roundIndex];
        spinSpeed = speedsDegPerSec[roundIndex];

        sliceCenterDeg = Random.Range(0f, 360f);
        needleDeg = Random.Range(0f, 360f);

        if (targetArc)
        {
            targetArc.type = Image.Type.Filled;
            targetArc.fillMethod = Image.FillMethod.Radial360;
            targetArc.fillAmount = Mathf.Clamp01(sliceSize / 360f);
            float startFromTop = sliceCenterDeg - sliceSize * 0.5f;
            targetArc.rectTransform.localEulerAngles = new Vector3(0, 0, -startFromTop);
        }

        if (roundText) roundText.text = $"Round {roundIndex + 1} / {roundsRequired}";
        if (keyText)
        {
            keyText.text = hitKey.ToString();
            keyText.color = Color.white;
        }
    }

    void TryHit()
    {
        if (InsideSlice(needleDeg, sliceCenterDeg, sliceSize))
        {
            audioManager?.PlaySuccessSound();
            int next = roundIndex + 1;
            if (next >= roundsRequired)
                Finish(true);
            else
                StartRound(next);
        }
        else
        {
            audioManager?.PlayFailSound();
            StartRound(0);
            if (keyText)
                keyText.text = "E"; // reset E text
            if (missText)
            {
                missText.gameObject.SetActive(true);
                Invoke(nameof(HideMissText), 0.7f); // hides after 0.7s
            }
        }
    }

    static bool InsideSlice(float angle, float center, float width)
    {
        float start = center - width * 0.5f;
        float end = center + width * 0.5f;
        angle = Normalize(angle); start = Normalize(start); end = Normalize(end);
        if (start <= end) return angle >= start && angle <= end;
        return angle >= start || angle <= end;
    }

    static float Normalize(float a)
    {
        a %= 360f;
        if (a < 0f) a += 360f;
        return a;
    }

    void Finish(bool success)
    {
        playing = false;
        PauseController.SetPause(false);
        if (success)
            audioManager?.PlayCompletedSound(detach: true);
        else
            audioManager?.PlayFailSound(detach: true);
        OnComplete?.Invoke(success);
        IsLockpickActive = false;
        Destroy(gameObject);
    }
    private void HideMissText()
    {
        if (missText)
            missText.gameObject.SetActive(false);
    }
}

