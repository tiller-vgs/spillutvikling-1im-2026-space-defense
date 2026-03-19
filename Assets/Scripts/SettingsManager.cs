using UnityEngine;

public static class SettingsManager
{
    public static bool fpsCapped = true;
    public static int fpsCap = 60;
    public static bool fpsCounterOn = false;
    public static bool musicOn = true;
    public static float musicVolume = 0.3f;

    public static void Load()
    {
        fpsCapped = PlayerPrefs.GetInt("fpsCapped", 1) == 1;
        fpsCap = PlayerPrefs.GetInt("fpsCap", 60);
        fpsCounterOn = PlayerPrefs.GetInt("fpsCounterOn", 0) == 1;
        musicOn = PlayerPrefs.GetInt("musicOn", 1) == 1;
        musicVolume = PlayerPrefs.GetFloat("musicVolume", 0.3f);
    }

    public static void Save()
    {
        PlayerPrefs.SetInt("fpsCapped", fpsCapped ? 1 : 0);
        PlayerPrefs.SetInt("fpsCap", fpsCap);
        PlayerPrefs.SetInt("fpsCounterOn", fpsCounterOn ? 1 : 0);
        PlayerPrefs.SetInt("musicOn", musicOn ? 1 : 0);
        PlayerPrefs.SetFloat("musicVolume", musicVolume);
        PlayerPrefs.Save();
    }

    public static void ApplyFPS()
    {
        // vsync må vær av for at target frame rate ska funk
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = fpsCapped ? fpsCap : -1;
        Save();
    }

    public static void ApplyFPSCounter()
    {
        if (FPSCounter.instance != null)
            FPSCounter.instance.Toggle(fpsCounterOn);
    }
}
