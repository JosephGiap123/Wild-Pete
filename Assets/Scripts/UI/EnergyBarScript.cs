using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnergyBarScript : MonoBehaviour
{
    [SerializeField] private Slider curEnergySlider;
    public Image fill;

    public void SetMaxEnergy(float energy)
    {
        curEnergySlider.maxValue = energy;
        curEnergySlider.value = energy;
    }

    public void SetEnergy(float energy)
    {
        curEnergySlider.value = energy;
    }

    public void UpdateEnergyBar(float current, float max)
    {
        curEnergySlider.maxValue = max;


        curEnergySlider.value = current;

    }

    public void UpdateMaxEnergy(float max)
    {
        curEnergySlider.maxValue = max;


    }
}