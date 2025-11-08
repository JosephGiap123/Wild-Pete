using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(CinemachineImpulseSource))]
public class TestShakeOnKey : MonoBehaviour
{
    CinemachineImpulseSource source;

    void Awake()
    {
        source = GetComponent<CinemachineImpulseSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))   // press T to test
            source.GenerateImpulse();
    }
}
