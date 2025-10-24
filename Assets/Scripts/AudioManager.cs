using UnityEngine;

[System.Serializable]
public class AudioSettings
{
    [Range(0f, 1f)] public float backgroundVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 0.7f;
    [Range(0f, 1f)] public float heartbeatVolume = 0.8f;
    public bool loopBackgroundMusic = true;
}

[System.Serializable]
public class AudioClips
{
    public AudioClip backgroundMusic;
    public AudioClip chaseBackgroundMusic;
    public AudioClip coffeeBrewSound;
    public AudioClip dialogueSound;
    public AudioClip chaseSound;
    public AudioClip heartbeatSound;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Clips")]
    [SerializeField] private AudioClips audioClips = new AudioClips();

    [Header("Audio Settings")]
    [SerializeField] private AudioSettings settings = new AudioSettings();

    private AudioSource backgroundSource;
    private AudioSource sfxSource;
    private AudioSource chaseSource;
    private AudioSource heartbeatSource;
    private bool isChasing = false;

    void Awake()
    {
        InitializeSingleton();
        CreateAudioSources();
        SetupAudioSources();
        PlayBackgroundMusic();
    }

    void Update()
    {
        UpdateBackgroundMusic();
        UpdateHeartbeat();
    }

    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void CreateAudioSources()
    {
        backgroundSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();
        chaseSource = gameObject.AddComponent<AudioSource>();
        heartbeatSource = gameObject.AddComponent<AudioSource>();
    }

    private void SetupAudioSources()
    {
        ConfigureAudioSource(backgroundSource, settings.loopBackgroundMusic, settings.backgroundVolume);
        ConfigureAudioSource(sfxSource, false, settings.sfxVolume);
        ConfigureAudioSource(chaseSource, true, 0f);
        ConfigureAudioSource(heartbeatSource, true, 0f);

        if (audioClips.heartbeatSound != null)
        {
            heartbeatSource.clip = audioClips.heartbeatSound;
        }
    }

    private void ConfigureAudioSource(AudioSource source, bool loop, float volume)
    {
        source.loop = loop;
        source.volume = volume;
        source.spatialBlend = 0f;
    }

    private void UpdateBackgroundMusic()
    {
        float targetBackgroundVolume = isChasing ? 0f : settings.backgroundVolume;
        float targetChaseVolume = isChasing ? settings.backgroundVolume : 0f;

        backgroundSource.volume = Mathf.Lerp(backgroundSource.volume, targetBackgroundVolume, Time.deltaTime * 2f);
        chaseSource.volume = Mathf.Lerp(chaseSource.volume, targetChaseVolume, Time.deltaTime * 2f);
    }

    private void UpdateHeartbeat()
    {
        if (isChasing)
        {
            heartbeatSource.volume = Mathf.Lerp(heartbeatSource.volume, settings.heartbeatVolume, Time.deltaTime * 3f);
            heartbeatSource.pitch = Mathf.Lerp(heartbeatSource.pitch, 1.2f, Time.deltaTime * 0.5f);
        }
        else
        {
            heartbeatSource.volume = Mathf.Lerp(heartbeatSource.volume, 0f, Time.deltaTime * 2f);

            if (heartbeatSource.volume < 0.1f)
            {
                heartbeatSource.pitch = 1.0f;
            }
        }
    }

    private void PlayBackgroundMusic()
    {
        PlayClipSafe(backgroundSource, audioClips.backgroundMusic);
        PlayClipSafe(chaseSource, audioClips.chaseBackgroundMusic);
        chaseSource.volume = 0f;
    }

    private void PlayClipSafe(AudioSource source, AudioClip clip)
    {
        if (source != null && clip != null)
        {
            source.clip = clip;
            source.Play();
        }
    }

    public void StartChase()
    {
        if (isChasing) return;

        isChasing = true;
        PlaySFX(audioClips.chaseSound);
        StartHeartbeat();
    }

    public void StopChase()
    {
        if (!isChasing) return;

        isChasing = false;
    }

    public void PlayCoffeeBrewSound() => PlaySFX(audioClips.coffeeBrewSound);
    public void PlayDialogueSound() => PlaySFX(audioClips.dialogueSound);

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, settings.sfxVolume);
        }
    }

    private void StartHeartbeat()
    {
        if (heartbeatSource != null && audioClips.heartbeatSound != null)
        {
            heartbeatSource.clip = audioClips.heartbeatSound;
            heartbeatSource.volume = 0f;
            heartbeatSource.Play();
        }
    }
    public void SetBackgroundVolume(float volume)
    {
        settings.backgroundVolume = Mathf.Clamp01(volume);
        if (!isChasing)
        {
            backgroundSource.volume = settings.backgroundVolume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        settings.sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = settings.sfxVolume;
    }

    public void SetHeartbeatVolume(float volume) => settings.heartbeatVolume = Mathf.Clamp01(volume);

    public void SetHeartbeatIntensity(float intensity)
    {
        float clampedIntensity = Mathf.Clamp(intensity, 0.1f, 1.0f);
        settings.heartbeatVolume = clampedIntensity * 0.8f;
        heartbeatSource.pitch = 0.8f + clampedIntensity * 0.4f;
    }

    public bool IsChasing => isChasing;
    public float HeartbeatVolume => heartbeatSource.volume;

    public void StopAllSounds()
    {
        backgroundSource?.Stop();
        chaseSource?.Stop();
        heartbeatSource?.Stop();
        sfxSource?.Stop();
    }
}