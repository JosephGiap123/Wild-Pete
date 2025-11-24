using UnityEngine;
using TMPro;
public class BossHPBarInteractor : MonoBehaviour
{
    [SerializeField] BossHealthBarScript hpBar;
    [SerializeField] EnemyBase bossAI;
    public void Start()
    {
        hpBar.GetComponentInChildren<TMP_Text>().text = gameObject.name;
        hpBar.SetMaxHealth(bossAI.GetMaxHealth());
        hpBar.SetHealth(bossAI.GetHealth());
    }

    public void UpdateHealthVisual()
    {
        hpBar.UpdateHealthBar(bossAI.GetHealth(), bossAI.GetMaxHealth());
    }
    public void ShowHealthBar(bool shown)
    {
        hpBar.ActivateBossHPBar(shown);
    }
}
