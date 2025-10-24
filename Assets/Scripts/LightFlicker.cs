using UnityEngine;
using System.Collections;

public class LightFlicker : MonoBehaviour
{
    [Header("Мигание света")]
    public float minIntensity = 0.2f;
    public float maxIntensity = 1f;
    public float flickerSpeed = 10f;

    [Header("Цвета")]
    public Color normalColor = Color.white;
    public Color fearColor = Color.red; // Красный цвет для страха
    public float colorChangeSpeed = 2f;

    private Light lightSource;
    private float originalIntensity;
    private Color originalColor;
    private bool isFlickering = false;
    private bool isRedColor = false;

    void Start()
    {
        lightSource = GetComponent<Light>();
        if (lightSource != null)
        {
            originalIntensity = lightSource.intensity;
            originalColor = lightSource.color;
        }
    }

    void Update()
    {
        if (isFlickering && lightSource != null)
        {
            // Случайное мигание света
            float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0);
            lightSource.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);

            // Плавное изменение цвета на красный
            if (isRedColor)
            {
                lightSource.color = Color.Lerp(lightSource.color, fearColor, Time.deltaTime * colorChangeSpeed);
            }
        }
    }

    public void StartFlicker()
    {
        isFlickering = true;
        isRedColor = true; // Включаем красный цвет
    }

    public void StopFlicker()
    {
        isFlickering = false;
        isRedColor = false; // Выключаем красный цвет

        if (lightSource != null)
        {
            lightSource.intensity = originalIntensity;
            lightSource.color = originalColor; // Возвращаем оригинальный цвет
        }
    }

    // Дополнительный метод для быстрого переключения цвета
    public void SetRedColor(bool red)
    {
        isRedColor = red;
        if (!red && lightSource != null)
        {
            lightSource.color = originalColor;
        }
    }
}