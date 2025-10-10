using UnityEngine;

public class DummyAnimRelay : MonoBehaviour
{
    [SerializeField] Dummy dummyScript;

    public void CallEndHurtState(){
        dummyScript.EndHurtState();
    }
}
