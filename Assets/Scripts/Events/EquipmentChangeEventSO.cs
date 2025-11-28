using UnityEngine;
using UnityEngine.Events;
[CreateAssetMenu(fileName = "EquipmentChangeEventSO", menuName = "Events/EquipmentChangeEventSO")]
public class EquipmentChangeEventSO : ScriptableObject
{
    public UnityEvent<EquipmentSO> onEventRaised;

    public void RaiseEvent(EquipmentSO equipment)
    {
        onEventRaised?.Invoke(equipment);
    }
}
