using UnityEngine;



public class cinecamerainstance : MonoBehaviour
{
    public static cinecamerainstance instance;
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // ✅ prevents duplicates
            return;
        }
        instance = this;
    }
}
