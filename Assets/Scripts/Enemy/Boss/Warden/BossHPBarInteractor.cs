using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class BossHPBarInteractor : MonoBehaviour
{
    [SerializeField] BossHealthBarScript hpBar; // Can be assigned in Inspector, but will also be found dynamically
    [SerializeField] EnemyBase bossAI;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-find the health bar when scene loads (in case it's in a DontDestroyOnLoad object)
        // Use a coroutine to wait a frame for DontDestroyOnLoad objects to be fully initialized
        StartCoroutine(FindHealthBarAfterSceneLoad());
    }

    private System.Collections.IEnumerator FindHealthBarAfterSceneLoad()
    {
        // Wait a frame to ensure DontDestroyOnLoad objects are fully initialized
        yield return null;
        FindHealthBar();
    }

    private void FindHealthBar()
    {
        // If hpBar is null or destroyed, try to find it
        if (hpBar == null || (hpBar.gameObject == null))
        {
            // FindObjectsByType searches all loaded scenes including the persistent scene (where DontDestroyOnLoad objects live)
            // Include inactive objects in case the health bar is temporarily disabled
            BossHealthBarScript[] allBars = FindObjectsByType<BossHealthBarScript>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (allBars != null && allBars.Length > 0)
            {
                // Use the first valid health bar found
                foreach (BossHealthBarScript bar in allBars)
                {
                    if (bar != null && bar.gameObject != null)
                    {
                        hpBar = bar;
                        Debug.Log($"BossHPBarInteractor: Found BossHealthBarScript dynamically: {hpBar.name}");
                        return;
                    }
                }
            }

            if (hpBar == null)
            {
                Debug.LogWarning($"BossHPBarInteractor: Could not find BossHealthBarScript. It may not exist yet or may not be marked as DontDestroyOnLoad.");
            }
        }
    }

    private BossHealthBarScript GetHealthBar()
    {
        // Try to find health bar if it's null
        if (hpBar == null || hpBar.gameObject == null)
        {
            FindHealthBar();
        }
        return hpBar;
    }

    public void Start()
    {
        // Try to find health bar if not assigned
        FindHealthBar();

        BossHealthBarScript healthBar = GetHealthBar();
        if (healthBar != null)
        {
            TMP_Text textComponent = healthBar.GetComponentInChildren<TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = gameObject.name;
            }

            if (bossAI != null)
            {
                healthBar.SetMaxHealth(bossAI.GetMaxHealth());
                healthBar.SetHealth(bossAI.GetHealth());
            }
        }
        else
        {
            Debug.LogWarning($"BossHPBarInteractor: Could not find BossHealthBarScript for {gameObject.name}. It may not exist yet or may be in a DontDestroyOnLoad object.");
        }
    }

    public void UpdateHealthVisual()
    {
        BossHealthBarScript healthBar = GetHealthBar();
        if (healthBar != null && bossAI != null)
        {
            healthBar.UpdateHealthBar(bossAI.GetHealth(), bossAI.GetMaxHealth());
        }
    }

    public void ShowHealthBar(bool shown)
    {
        BossHealthBarScript healthBar = GetHealthBar();
        if (healthBar != null)
        {
            healthBar.ActivateBossHPBar(shown);
        }
        else
        {
            Debug.LogWarning($"BossHPBarInteractor: Could not find BossHealthBarScript for {gameObject.name}, cannot show/hide health bar");
        }
    }
}
