using UnityEngine;

public class CheatManager : MonoBehaviour
{
    public static CheatManager Instance { get; private set; }

    [Tooltip("If true, player takes no damage.")]
    public bool invulnerable = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ToggleInvulnerability()
    {
        invulnerable = !invulnerable;
        Debug.Log($"CheatMode Invulnerable = {invulnerable}");
    }
}

