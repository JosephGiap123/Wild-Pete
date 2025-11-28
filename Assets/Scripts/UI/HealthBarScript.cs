using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class HealthBarScript : MonoBehaviour
{
    [SerializeField] private Slider curHealthSlider;
    [SerializeField] private Slider chipAwaySlider;
    [SerializeField] private TMP_Text healthText;

    private int currentHealth = 0;
    private int maxHealth = 0;
    public Gradient gradient;
    public Image fill;

    [Header("Chip Away Settings")]
    [SerializeField] private float chipDelay = 0.3f;
    [SerializeField] private float chipSpeed = 0.5f;

    private Coroutine chipRoutine;

    void Awake()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.Contains("Menu"))
        {
            this.gameObject.SetActive(false);
            return;
        }
        else
        {
            this.gameObject.SetActive(true);
        }
    }
    public void SetMaxHealth(int health)
    {
        curHealthSlider.maxValue = health;
        chipAwaySlider.maxValue = health;
        curHealthSlider.value = health;
        chipAwaySlider.value = health;
        maxHealth = health;
        fill.color = gradient.Evaluate(1f);
        UpdateHealthText(currentHealth, maxHealth);
    }

    public void SetHealth(int health)
    {
        curHealthSlider.value = health;
        chipAwaySlider.value = health;
        currentHealth = health;
    }

    public void UpdateHealthBar(int current, int max)
    {
        curHealthSlider.maxValue = max;
        chipAwaySlider.maxValue = max;
        currentHealth = current;
        maxHealth = max;

        // Stop previous animation if it's still running
        if (chipRoutine != null)
            StopCoroutine(chipRoutine);

        // Taking damage: current health drops immediately, chip bar follows slowly
        if (chipAwaySlider.value > current)
        {
            // Update current health bar immediately
            curHealthSlider.value = current;
            fill.color = gradient.Evaluate(curHealthSlider.normalizedValue);

            // Animate chip away bar to catch up
            chipRoutine = StartCoroutine(AnimateChipAway(current));
        }
        // Healing: both bars update instantly
        else
        {
            curHealthSlider.value = current;
            chipAwaySlider.value = current;
            fill.color = gradient.Evaluate(curHealthSlider.normalizedValue);
        }
        UpdateHealthText(currentHealth, maxHealth);
    }

    public void UpdateMaxHealth(int max)
    {
        curHealthSlider.maxValue = max;
        chipAwaySlider.maxValue = max;
        fill.color = gradient.Evaluate(curHealthSlider.normalizedValue);
        maxHealth = max;
    }

    public void UpdateHealthText(int current, int max)
    {
        healthText.text = current.ToString() + "/" + max.ToString();
    }

    private IEnumerator AnimateChipAway(int targetValue)
    {
        yield return new WaitForSeconds(chipDelay);

        float startValue = chipAwaySlider.value;
        float elapsed = 0f;

        while (elapsed < chipSpeed)
        {
            elapsed += Time.deltaTime;
            chipAwaySlider.value = Mathf.Lerp(startValue, targetValue, elapsed / chipSpeed);
            yield return null;
        }

        chipAwaySlider.value = targetValue;
    }
}