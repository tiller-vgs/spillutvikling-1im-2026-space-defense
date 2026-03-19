using UnityEngine;

public class GameMusic : MonoBehaviour
{
    public static GameMusic instance;
    AudioSource audioSource;
    bool hasStartedPlaying = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = Resources.Load<AudioClip>("Spilllot");
        audioSource.loop = true;
        audioSource.volume = SettingsManager.musicVolume;
        audioSource.playOnAwake = false;
    }

    public void StartMusic()
    {
        if (audioSource == null || audioSource.clip == null) return;
        
        // respect user settings
        if (!SettingsManager.musicOn) return;

        audioSource.Play();
        hasStartedPlaying = true;
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
