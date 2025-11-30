using UnityEngine;

public class PlayerDashGhost : MonoBehaviour
{
    void Awake()
    {
        Destroy(gameObject, 0.5f);
    }
}
