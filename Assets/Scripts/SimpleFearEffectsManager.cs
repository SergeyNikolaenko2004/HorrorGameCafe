using UnityEngine;
using System.Collections;

public class SimpleFearEffectsManager : MonoBehaviour
{
    public static SimpleFearEffectsManager Instance;

    [Header("������� ��� ������")]
    public CameraShake cameraShake;
    public LightFlicker[] lights; // ��� ����� � ����

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
        // ������������� ������� ���������� ���� �� ���������
        if (cameraShake == null)
        {
            cameraShake = FindObjectOfType<CameraShake>();
        }

        // ������������� ������� ��� ����� � ����� "Light"
        if (lights == null || lights.Length == 0)
        {
            GameObject[] lightObjects = GameObject.FindGameObjectsWithTag("Light");
            lights = new LightFlicker[lightObjects.Length];
            for (int i = 0; i < lightObjects.Length; i++)
            {
                // ��������� ��������� LightFlicker ���� ��� ���
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
            Debug.Log("������ ���������� �������� ������");

            // �������� �������� ������
            if (cameraShake != null)
            {
                cameraShake.StartShake();
            }

            // �������� ������� ���� ����
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
            Debug.Log("��������� ���������� �������� ������");

            // ��������� �������� ������
            if (cameraShake != null)
            {
                cameraShake.StopShake();
            }

            // ��������� ������� ���� ����
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