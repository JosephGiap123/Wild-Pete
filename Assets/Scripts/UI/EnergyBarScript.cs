using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class EnergyBarScript : MonoBehaviour
{
    [SerializeField] private Slider curEnergySlider;
    public Image fill;

    void Awake()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.Contains("Menu"))
        {
            this.gameObject.SetActive(false);
            return;
        }
        else
        {
            this.gameObject.SetActive(true);
        }
    }

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