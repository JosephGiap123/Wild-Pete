using UnityEngine;
using TMPro;
public class BossHPBarInteractor : MonoBehaviour
{
    [SerializeField] BossHealthBarScript hpBar;
    [SerializeField] WardenAI wardenAI;
    public void Start()
    {
        hpBar.GetComponentInChildren<TMP_Text>().text = gameObject.name;
        hpBar.SetMaxHealth(wardenAI.GetMaxHealth());
        hpBar.SetHealth(wardenAI.GetHealth());
    }

    public void UpdateHealthVisual()
    {
        hpBar.UpdateHealthBar(wardenAI.GetHealth(), wardenAI.GetMaxHealth());
    }
    public void ShowHealthBar(bool shown)
    {
        hpBar.ActivateBossHPBar(shown);
    }
}
