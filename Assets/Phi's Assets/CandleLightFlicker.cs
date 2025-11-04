using UnityEngine;
using UnityEngine.Rendering.Universal; // Required for Light2D

[RequireComponent(typeof(Light2D))]
public class CandleLightFlicker : MonoBehaviour
{
    [Header("Flicker Settings")]
    [Tooltip("Base light intensity")]
    public float baseIntensity = 1f;

    [Tooltip("How much intensity can vary (+/-)")]
    public float flickerAmount = 0.2f;

    [Tooltip("Speed of flickering transitions")]
    public float flickerSpeed = 10f;

    [Header("Falloff / Radius Randomization")]
    [Tooltip("Base outer radius of the light")]
    public float baseOuterRadius = 5f;

    [Tooltip("How much the radius can vary (+/-)")]
    public float radiusVariation = 0.3f;

    private Light2D light2D;
    private float targetIntensity;
    private float targetRadius;

    void Start()
    {
        light2D = GetComponent<Light2D>();

        if (baseIntensity <= 0f) baseIntensity = light2D.intensity;
        if (baseOuterRadius <= 0f) baseOuterRadius = light2D.pointLightOuterRadius;

        targetIntensity = baseIntensity;
        targetRadius = baseOuterRadius;
    }

    void Update()
    {
        if (Random.value < 0.1f)
        {
            targetIntensity = baseIntensity + Random.Range(-flickerAmount, flickerAmount);
            targetRadius = baseOuterRadius + Random.Range(-radiusVariation, radiusVariation);
        }

        light2D.intensity = Mathf.Lerp(light2D.intensity, targetIntensity, Time.deltaTime * flickerSpeed);
        light2D.pointLightOuterRadius = Mathf.Lerp(light2D.pointLightOuterRadius, targetRadius, Time.deltaTime * flickerSpeed);
    }
}