using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBarScript : MonoBehaviour
{
    [SerializeField] private Slider curHealthSlider;
    [SerializeField] private Slider chipAwaySlider;
    private BasePlayerMovement2D playerMovement;
    public Gradient gradient;
    public Image fill;
    
    [Header("Chip Away Settings")]
    [SerializeField] private float chipDelay = 0.3f;
    [SerializeField] private float chipSpeed = 0.5f;
    
    private Coroutine chipRoutine;
    
    private void OnEnable()
    {
        GameManager.OnPlayerSet += HandlePlayerSet;
    }
    
    private void OnDisable()
    {
        GameManager.OnPlayerSet -= HandlePlayerSet;
        if (playerMovement != null)
        {
            playerMovement.OnHealthChanged -= UpdateHealthBar;
            playerMovement.OnMaxHealthChanged -= UpdateMaxHealth;
        }
    }
    
    private void HandlePlayerSet(GameObject player)
    {
        if (playerMovement != null)
        {
            playerMovement.OnHealthChanged -= UpdateHealthBar;
            playerMovement.OnMaxHealthChanged -= UpdateMaxHealth;
        }
        
        playerMovement = player.GetComponent<BasePlayerMovement2D>();
        if (playerMovement == null)
        {
            Debug.LogError("HealthBarScript: Player missing BasePlayerMovement2D component!");
            return;
        }
        
        SetMaxHealth(playerMovement.maxHealth);
        SetHealth(playerMovement.health);
        
        playerMovement.OnHealthChanged += UpdateHealthBar;
        playerMovement.OnMaxHealthChanged += UpdateMaxHealth;
    }
    
    private void SetMaxHealth(int health)
    {
        curHealthSlider.maxValue = health;
        chipAwaySlider.maxValue = health;
        curHealthSlider.value = health;
        chipAwaySlider.value = health;
        fill.color = gradient.Evaluate(1f);
    }
    
    private void SetHealth(int health)
    {
        curHealthSlider.value = health;
        chipAwaySlider.value = health;
    }
    
    private void UpdateHealthBar(int current, int max)
    {
        curHealthSlider.maxValue = max;
        chipAwaySlider.maxValue = max;
        
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
    }
    
    private void UpdateMaxHealth(int max)
    {
        curHealthSlider.maxValue = max;
        chipAwaySlider.maxValue = max;
        fill.color = gradient.Evaluate(curHealthSlider.normalizedValue);
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