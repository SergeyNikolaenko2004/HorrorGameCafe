using UnityEngine;

public interface ICameraShake
{
    void StartShake();
    void StopShake();
    bool IsShaking { get; }
}

[System.Serializable]
public class ShakeSettings
{
    [field: SerializeField] public float Intensity { get; private set; } = 0.1f;
    [field: SerializeField] public float Frequency { get; private set; } = 2f;
    [field: SerializeField] public float ReturnSpeed { get; private set; } = 3f;
}

public class CameraShake : MonoBehaviour, ICameraShake
{
    [Header("Shake Settings")]
    [SerializeField] private ShakeSettings settings = new ShakeSettings();

    private Vector3 _originalPosition;
    private bool _isShaking;
    private IShakeCalculator _shakeCalculator;

    public bool IsShaking => _isShaking;

    private void Awake()
    {
        _originalPosition = transform.localPosition;
        _shakeCalculator = new PerlinShakeCalculator();
    }

    private void Update()
    {
        if (_isShaking)
        {
            ApplyShake();
        }
        else
        {
            ReturnToOriginalPosition();
        }
    }

    public void StartShake()
    {
        _isShaking = true;
    }

    public void StopShake()
    {
        _isShaking = false;
    }

    private void ApplyShake()
    {
        Vector3 shakeOffset = _shakeCalculator.CalculateShake(Time.time, settings.Frequency, settings.Intensity);
        transform.localPosition = _originalPosition + shakeOffset;
    }

    private void ReturnToOriginalPosition()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, _originalPosition, Time.deltaTime * settings.ReturnSpeed);
    }
}

public interface IShakeCalculator
{
    Vector3 CalculateShake(float time, float frequency, float intensity);
}

public class PerlinShakeCalculator : IShakeCalculator
{
    public Vector3 CalculateShake(float time, float frequency, float intensity)
    {
        float x = (Mathf.PerlinNoise(time * frequency, 0) - 0.5f) * 2f;
        float y = (Mathf.PerlinNoise(0, time * frequency) - 0.5f) * 2f;

        return new Vector3(x, y, 0) * intensity;
    }
}