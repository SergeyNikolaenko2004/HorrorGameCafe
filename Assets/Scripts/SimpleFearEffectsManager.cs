using UnityEngine;
using System.Collections;

public class SimpleFearEffectsManager : MonoBehaviour
{
    public static SimpleFearEffectsManager Instance;

    [Header("Эффекты для погони")]
    public CameraShake cameraShake;
    public LightFlicker[] lights; // Все лампы в кафе

    private bool isChasing = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        // Автоматически находим компоненты если не назначены
        if (cameraShake == null)
        {
            cameraShake = FindObjectOfType<CameraShake>();
        }

        // Автоматически находим все лампы с тегом "Light"
        if (lights == null || lights.Length == 0)
        {
            GameObject[] lightObjects = GameObject.FindGameObjectsWithTag("Light");
            lights = new LightFlicker[lightObjects.Length];
            for (int i = 0; i < lightObjects.Length; i++)
            {
                // Добавляем компонент LightFlicker если его нет
                LightFlicker flicker = lightObjects[i].GetComponent<LightFlicker>();
                if (flicker == null)
                {
                    flicker = lightObjects[i].AddComponent<LightFlicker>();
                }
                lights[i] = flicker;
            }
        }
    }

    public void StartChaseEffects()
    {
        if (!isChasing)
        {
            isChasing = true;
            Debug.Log("Запуск визуальных эффектов погони");

            // Включаем дрожание камеры
            if (cameraShake != null)
            {
                cameraShake.StartShake();
            }

            // Включаем мигание всех ламп
            foreach (LightFlicker light in lights)
            {
                if (light != null)
                {
                    light.StartFlicker();
                }
            }
        }
    }

    public void StopChaseEffects()
    {
        if (isChasing)
        {
            isChasing = false;
            Debug.Log("Остановка визуальных эффектов погони");

            // Выключаем дрожание камеры
            if (cameraShake != null)
            {
                cameraShake.StopShake();
            }

            // Выключаем мигание всех ламп
            foreach (LightFlicker light in lights)
            {
                if (light != null)
                {
                    light.StopFlicker();
                }
            }
        }
    }
}