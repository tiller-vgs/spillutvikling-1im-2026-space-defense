using UnityEngine;

// Kontrollere bakgrunnsmusikk og overlever sceneinnlastingam lytte te SettingsManager for volum
public class GameMusic : MonoBehaviour
{
    public static GameMusic instance;
    AudioSource audioSource;
    bool hasStartedPlaying = false;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = SettingsManager.musicVolume;
    }

    public void PlayMenuMusic()
    {
        PlayClip("Assets/Sound/menumusic");
    }

    public void PlayGameMusic()
    {
        PlayClip("Assets/Sound/Spilllot");
    }

    void PlayClip(string clipName)
    {
        if (audioSource == null) return;
        
        AudioClip clip = Resources.Load<AudioClip>(clipName);
        if (clip == null || (audioSource.clip != null && audioSource.clip.name == clip.name && audioSource.isPlaying)) return;

        audioSource.Stop();
        audioSource.clip = clip;
        
        if (SettingsManager.musicOn)
        {
            audioSource.Play();
            hasStartedPlaying = true;
        }
    }

    public void StartMusic()
    {
        PlayGameMusic();
    }

    public void SetMusic(bool on)
    {
        SettingsManager.musicOn = on;
        SettingsManager.Save();

        if (audioSource == null) return;

        if (on && hasStartedPlaying)
            audioSource.UnPause();
        else if (!on)
            audioSource.Pause();
    }

    public void SetVolume(float vol)
    {
        SettingsManager.musicVolume = vol;
        SettingsManager.Save();

        if (audioSource != null)
            audioSource.volume = vol;
    }

    public void StopMusic()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            hasStartedPlaying = false;
        }
    }
}
