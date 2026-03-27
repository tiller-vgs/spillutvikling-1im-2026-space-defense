using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

// Håndterer brukerinnstillinger (volum, fps, etc) og mesteparten av UIen
public class MainMenu : MonoBehaviour
{
    GameObject menuPanel;
    GameObject settingsPanel;
    GameObject backgroundLayer;
    VideoPlayer videoPlayer;
    RawImage bgImage;
    Toggle fullscreenToggle;

    void Start()
    {
        SettingsManager.Load();
        if (GameMusic.instance != null)
        {
            GameMusic.instance.SetVolume(SettingsManager.musicVolume);
            GameMusic.instance.SetMusic(SettingsManager.musicOn);
        }
        SettingsManager.ApplyFPS();
        Time.timeScale = 0;
        BuildMenuUI();
        BuildSettingsUI();
        Invoke("ApplyFPSCounterState", 0.1f);
        
        if (GameMusic.instance != null) GameMusic.instance.PlayMenuMusic();
        SettingsManager.ApplyFullscreen();
        settingsPanel.SetActive(false);
    }

    void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.f11Key.wasPressedThisFrame)
        {
            SettingsManager.fullscreen = !Screen.fullScreen;
            SettingsManager.ApplyFullscreen();
            if (fullscreenToggle != null) fullscreenToggle.isOn = SettingsManager.fullscreen;
        }
    }

    void ApplyFPSCounterState()
    {
        SettingsManager.ApplyFPSCounter();
    }

    // Generere hele hovedmenyen uten å være avhengig av Unity Scene prefabs
    void BuildMenuUI()
    {
        var canvas = new GameObject("MenuCanvas");
        var c = canvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 200;
        var scaler = canvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvas.AddComponent<GraphicRaycaster>();

        backgroundLayer = new GameObject("BackgroundLayer");
        backgroundLayer.transform.SetParent(canvas.transform, false);
        backgroundLayer.transform.SetAsFirstSibling();
        bgImage = backgroundLayer.AddComponent<RawImage>();
        bgImage.color = Color.black;
        var bgRect = backgroundLayer.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        videoPlayer = backgroundLayer.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = true;
        videoPlayer.renderMode = VideoRenderMode.APIOnly;
        videoPlayer.timeUpdateMode = VideoTimeUpdateMode.UnscaledGameTime;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        videoPlayer.clip = Resources.Load<VideoClip>("Assets/Video/main_menu_background");
        videoPlayer.prepareCompleted += (p) => {
            bgImage.texture = p.texture;
            bgImage.color = Color.white;
            p.Play();
        };
        videoPlayer.Prepare();

        menuPanel = new GameObject("MenuPanel");
        menuPanel.transform.SetParent(canvas.transform, false);
        var panelRect = menuPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        var logoObj = new GameObject("Logo");
        logoObj.transform.SetParent(menuPanel.transform, false);
        var logoImg = logoObj.AddComponent<Image>();
        logoImg.preserveAspect = true;
        logoImg.type = Image.Type.Simple;
        var logoSprite = Resources.Load<Sprite>("Assets/Image/namelogo");
        if (logoSprite != null)
            logoImg.sprite = logoSprite;

        var logoRect = logoObj.GetComponent<RectTransform>();
        logoRect.anchorMin = new Vector2(0.5f, 0.85f);
        logoRect.anchorMax = new Vector2(0.5f, 0.85f);
        logoRect.anchoredPosition = Vector2.zero;
        logoRect.sizeDelta = new Vector2(1400, 250);

        CreateButton(menuPanel.transform, "PLAY", new Vector2(0.5f, 0.45f),
            new Vector2(300, 60), new Color(0.1f, 0.75f, 0.4f), () => PlayGame());

        CreateButton(menuPanel.transform, "SETTINGS", new Vector2(0.5f, 0.33f),
            new Vector2(300, 60), new Color(0.0f, 0.65f, 0.8f), () => OpenSettings());

        CreateButton(menuPanel.transform, "EXIT GAME", new Vector2(0.5f, 0.21f),
            new Vector2(300, 60), new Color(0.1f, 0.35f, 0.5f), () => ExitGame());

        settingsPanel = new GameObject("SettingsPanel");
        settingsPanel.transform.SetParent(canvas.transform, false);
        var spImg = settingsPanel.AddComponent<Image>();
        spImg.color = new Color(0.02f, 0.02f, 0.06f, 0.97f);
        var spRect = settingsPanel.GetComponent<RectTransform>();
        spRect.anchorMin = Vector2.zero;
        spRect.anchorMax = Vector2.one;
        spRect.offsetMin = Vector2.zero;
        spRect.offsetMax = Vector2.zero;
    }

    void BuildSettingsUI()
    {
        CreateText(settingsPanel.transform, "SETTINGS", 48,
            new Vector2(0.5f, 0.88f), new Color(0.0f, 0.85f, 0.9f));

        CreateText(settingsPanel.transform, "Music", 24,
            new Vector2(0.35f, 0.78f), Color.white);

        var musicToggleObj = new GameObject("MusicToggle");
        musicToggleObj.transform.SetParent(settingsPanel.transform, false);
        var musicToggleRect = musicToggleObj.AddComponent<RectTransform>();
        musicToggleRect.anchorMin = new Vector2(0.6f, 0.77f);
        musicToggleRect.anchorMax = new Vector2(0.6f, 0.79f);
        musicToggleRect.sizeDelta = new Vector2(40, 40);
        musicToggleObj.AddComponent<Image>().color = new Color(0.05f, 0.2f, 0.3f);

        var musicToggle = musicToggleObj.AddComponent<Toggle>();

        var musicCheckObj = new GameObject("Checkmark");
        musicCheckObj.transform.SetParent(musicToggleObj.transform, false);
        var musicCheckImg = musicCheckObj.AddComponent<Image>();
        musicCheckImg.color = new Color(0.1f, 0.9f, 0.5f);
        var musicCheckRect = musicCheckObj.GetComponent<RectTransform>();
        musicCheckRect.anchorMin = new Vector2(0.15f, 0.15f);
        musicCheckRect.anchorMax = new Vector2(0.85f, 0.85f);
        musicCheckRect.offsetMin = Vector2.zero;
        musicCheckRect.offsetMax = Vector2.zero;

        musicToggle.graphic = musicCheckImg;
        musicToggle.isOn = SettingsManager.musicOn;

        musicToggle.onValueChanged.AddListener((val) => {
            SettingsManager.musicOn = val;
            SettingsManager.Save();
            if (GameMusic.instance != null) GameMusic.instance.SetMusic(val);
        });

        CreateText(settingsPanel.transform, "Music Volume", 24,
            new Vector2(0.35f, 0.70f), Color.white);

        var volValObj = CreateText(settingsPanel.transform, Mathf.RoundToInt(SettingsManager.musicVolume * 100) + "%", 22,
            new Vector2(0.72f, 0.70f), new Color(0.1f, 0.9f, 0.5f));
        var volValueText = volValObj.GetComponent<Text>();

        Slider musicVolSlider = CreateSlider(settingsPanel.transform, new Vector2(0.3f, 0.63f), new Vector2(0.7f, 0.65f));
        musicVolSlider.minValue = 0;
        musicVolSlider.maxValue = 1;
        musicVolSlider.wholeNumbers = false;
        musicVolSlider.value = SettingsManager.musicVolume;

        musicVolSlider.onValueChanged.AddListener((val) => {
            if (GameMusic.instance != null) GameMusic.instance.SetVolume(val);
            if (volValueText != null) volValueText.text = Mathf.RoundToInt(val * 100) + "%";
        });

        CreateText(settingsPanel.transform, "Fullscreen", 24,
            new Vector2(0.35f, 0.58f), Color.white);

        var fullToggleObj = new GameObject("FullToggle");
        fullToggleObj.transform.SetParent(settingsPanel.transform, false);
        var fullToggleRect = fullToggleObj.AddComponent<RectTransform>();
        fullToggleRect.anchorMin = new Vector2(0.6f, 0.57f);
        fullToggleRect.anchorMax = new Vector2(0.6f, 0.59f);
        fullToggleRect.sizeDelta = new Vector2(40, 40);
        fullToggleObj.AddComponent<Image>().color = new Color(0.05f, 0.2f, 0.3f);

        fullscreenToggle = fullToggleObj.AddComponent<Toggle>();

        var fullCheckObj = new GameObject("Checkmark");
        fullCheckObj.transform.SetParent(fullToggleObj.transform, false);
        var fullCheckImg = fullCheckObj.AddComponent<Image>();
        fullCheckImg.color = new Color(0.1f, 0.9f, 0.5f);
        var fullCheckRect = fullCheckObj.GetComponent<RectTransform>();
        fullCheckRect.anchorMin = new Vector2(0.15f, 0.15f);
        fullCheckRect.anchorMax = new Vector2(0.85f, 0.85f);
        fullCheckRect.offsetMin = Vector2.zero;
        fullCheckRect.offsetMax = Vector2.zero;

        fullscreenToggle.graphic = fullCheckImg;
        fullscreenToggle.isOn = SettingsManager.fullscreen;

        fullscreenToggle.onValueChanged.AddListener((val) => {
            SettingsManager.fullscreen = val;
            SettingsManager.ApplyFullscreen();
        });

        CreateText(settingsPanel.transform, "FPS Counter", 24,
            new Vector2(0.35f, 0.48f), Color.white);

        var fpsToggleObj = new GameObject("FPSToggle");
        fpsToggleObj.transform.SetParent(settingsPanel.transform, false);
        var fpsToggleRect = fpsToggleObj.AddComponent<RectTransform>();
        fpsToggleRect.anchorMin = new Vector2(0.6f, 0.47f);
        fpsToggleRect.anchorMax = new Vector2(0.6f, 0.49f);
        fpsToggleRect.sizeDelta = new Vector2(40, 40);
        fpsToggleObj.AddComponent<Image>().color = new Color(0.05f, 0.2f, 0.3f);

        var fpsToggle = fpsToggleObj.AddComponent<Toggle>();

        var checkObj = new GameObject("Checkmark");
        checkObj.transform.SetParent(fpsToggleObj.transform, false);
        var checkImg = checkObj.AddComponent<Image>();
        checkImg.color = new Color(0.1f, 0.9f, 0.5f);
        var checkRect = checkObj.GetComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0.15f, 0.15f);
        checkRect.anchorMax = new Vector2(0.85f, 0.85f);
        checkRect.offsetMin = Vector2.zero;
        checkRect.offsetMax = Vector2.zero;

        fpsToggle.graphic = checkImg;
        fpsToggle.isOn = SettingsManager.fpsCounterOn;

        fpsToggle.onValueChanged.AddListener((val) => {
            SettingsManager.fpsCounterOn = val;
            SettingsManager.Save();
            if (FPSCounter.instance != null) FPSCounter.instance.Toggle(val);
        });

        CreateText(settingsPanel.transform, "Cap FPS", 24,
            new Vector2(0.35f, 0.38f), Color.white);

        var capToggleObj = new GameObject("CapToggle");
        capToggleObj.transform.SetParent(settingsPanel.transform, false);
        var capToggleRect = capToggleObj.AddComponent<RectTransform>();
        capToggleRect.anchorMin = new Vector2(0.6f, 0.37f);
        capToggleRect.anchorMax = new Vector2(0.6f, 0.39f);
        capToggleRect.sizeDelta = new Vector2(40, 40);
        capToggleObj.AddComponent<Image>().color = new Color(0.05f, 0.2f, 0.3f);

        var capToggle = capToggleObj.AddComponent<Toggle>();

        var capCheckObj = new GameObject("Checkmark");
        capCheckObj.transform.SetParent(capToggleObj.transform, false);
        var capCheckImg = capCheckObj.AddComponent<Image>();
        capCheckImg.color = new Color(0.1f, 0.9f, 0.5f);
        var capCheckRect = capCheckObj.GetComponent<RectTransform>();
        capCheckRect.anchorMin = new Vector2(0.15f, 0.15f);
        capCheckRect.anchorMax = new Vector2(0.85f, 0.85f);
        capCheckRect.offsetMin = Vector2.zero;
        capCheckRect.offsetMax = Vector2.zero;

        capToggle.graphic = capCheckImg;
        capToggle.isOn = SettingsManager.fpsCapped;

        CreateText(settingsPanel.transform, "FPS Limit", 24,
            new Vector2(0.35f, 0.28f), Color.white);

        var capValObj = CreateText(settingsPanel.transform, SettingsManager.fpsCap + " FPS", 22,
            new Vector2(0.72f, 0.28f), new Color(0.1f, 0.9f, 0.5f));
        var capValueText = capValObj.GetComponent<Text>();

        var sliderObj = new GameObject("FPSSlider");
        sliderObj.transform.SetParent(settingsPanel.transform, false);
        var sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.3f, 0.21f);
        sliderRect.anchorMax = new Vector2(0.7f, 0.23f);
        sliderRect.offsetMin = Vector2.zero;
        sliderRect.offsetMax = Vector2.zero;
        sliderObj.AddComponent<Image>().color = new Color(0.05f, 0.15f, 0.2f);

        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        var fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        var fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        fill.AddComponent<Image>().color = new Color(0.0f, 0.65f, 0.8f);
        var fillR = fill.GetComponent<RectTransform>();
        fillR.anchorMin = Vector2.zero;
        fillR.anchorMax = Vector2.one;
        fillR.offsetMin = Vector2.zero;
        fillR.offsetMax = Vector2.zero;

        var handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObj.transform, false);
        var handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(10, 0);
        handleAreaRect.offsetMax = new Vector2(-10, 0);

        var handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        handle.AddComponent<Image>().color = new Color(0.1f, 0.9f, 0.5f);
        var handleRect = handle.GetComponent<RectTransform>();
        handleRect.anchorMin = new Vector2(0, 0);
        handleRect.anchorMax = new Vector2(0, 1);
        handleRect.sizeDelta = new Vector2(20, 0);

        var slider = sliderObj.AddComponent<Slider>();
        slider.fillRect = fillR;
        slider.handleRect = handleRect;
        slider.minValue = 15;
        slider.maxValue = 240;
        slider.wholeNumbers = true;
        slider.value = SettingsManager.fpsCap;
        slider.interactable = SettingsManager.fpsCapped;

        Text localCapText = capValueText;
        Slider localSlider = slider;

        slider.onValueChanged.AddListener((val) => {
            SettingsManager.fpsCap = (int)val;
            if (localCapText != null) localCapText.text = SettingsManager.fpsCap + " FPS";
            SettingsManager.ApplyFPS();
        });

        capToggle.onValueChanged.AddListener((val) => {
            SettingsManager.fpsCapped = val;
            SettingsManager.ApplyFPS();
            localSlider.interactable = val;
        });

        CreateButton(settingsPanel.transform, "BACK", new Vector2(0.5f, 0.12f),
            new Vector2(200, 50), new Color(0.1f, 0.35f, 0.5f), () => CloseSettings());
    }

    void PlayGame()
    {
        menuPanel.SetActive(false);
        if (backgroundLayer != null) backgroundLayer.SetActive(false);
        Time.timeScale = 1;
        if (GameMusic.instance != null) GameMusic.instance.PlayGameMusic();
    }

    void OpenSettings()
    {
        menuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    void CloseSettings()
    {
        settingsPanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    void ExitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void ShowMenu()
    {
        menuPanel.SetActive(true);
        if (backgroundLayer != null) backgroundLayer.SetActive(true);
        settingsPanel.SetActive(false);
        Time.timeScale = 0;

        if (GameMusic.instance != null) GameMusic.instance.PlayMenuMusic();

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.Prepare();
        }
    }

    Slider CreateSlider(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        var sliderObj = new GameObject("Slider");
        sliderObj.transform.SetParent(parent, false);
        var sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = anchorMin;
        sliderRect.anchorMax = anchorMax;
        sliderRect.offsetMin = Vector2.zero;
        sliderRect.offsetMax = Vector2.zero;
        sliderObj.AddComponent<Image>().color = new Color(0.05f, 0.15f, 0.2f);

        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        var fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        var fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        fill.AddComponent<Image>().color = new Color(0.0f, 0.65f, 0.8f);
        var fillR = fill.GetComponent<RectTransform>();
        fillR.anchorMin = Vector2.zero;
        fillR.anchorMax = Vector2.one;
        fillR.offsetMin = Vector2.zero;
        fillR.offsetMax = Vector2.zero;

        var handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObj.transform, false);
        var handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(10, 0);
        handleAreaRect.offsetMax = new Vector2(-10, 0);

        var handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        handle.AddComponent<Image>().color = new Color(0.1f, 0.9f, 0.5f);
        var handleRect = handle.GetComponent<RectTransform>();
        handleRect.anchorMin = new Vector2(0, 0);
        handleRect.anchorMax = new Vector2(0, 1);
        handleRect.sizeDelta = new Vector2(20, 0);

        var slider = sliderObj.AddComponent<Slider>();
        slider.fillRect = fillR;
        slider.handleRect = handleRect;
        return slider;
    }

    GameObject CreateText(Transform parent, string content, int size, Vector2 anchor, Color color)
    {
        var obj = new GameObject("Text_" + content);
        obj.transform.SetParent(parent, false);
        var txt = obj.AddComponent<Text>();
        txt.text = content;
        txt.fontSize = size;
        txt.color = color;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.sizeDelta = new Vector2(500, 80);
        rt.anchoredPosition = Vector2.zero;
        return obj;
    }

    void CreateButton(Transform parent, string label, Vector2 anchor, Vector2 size, Color bgColor,
        UnityEngine.Events.UnityAction onClick)
    {
        var btnObj = new GameObject("Btn_" + label);
        btnObj.transform.SetParent(parent, false);

        btnObj.AddComponent<Image>().color = new Color(0.05f, 0.15f, 0.2f);

        var innerObj = new GameObject("Inner");
        innerObj.transform.SetParent(btnObj.transform, false);
        var innerImg = innerObj.AddComponent<Image>();
        innerImg.color = bgColor;
        var innerRect = innerObj.GetComponent<RectTransform>();
        innerRect.anchorMin = Vector2.zero;
        innerRect.anchorMax = Vector2.one;
        innerRect.offsetMin = new Vector2(5, 5);
        innerRect.offsetMax = new Vector2(-5, -5);

        var highlightObj = new GameObject("Highlight");
        highlightObj.transform.SetParent(innerObj.transform, false);
        highlightObj.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.15f);
        var highlightRect = highlightObj.GetComponent<RectTransform>();
        highlightRect.anchorMin = new Vector2(0, 0.5f);
        highlightRect.anchorMax = new Vector2(1, 1);
        highlightRect.offsetMin = Vector2.zero;
        highlightRect.offsetMax = Vector2.zero;

        var btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = innerImg;
        var colors = btn.colors;
        colors.highlightedColor = bgColor * 1.3f;
        colors.pressedColor = bgColor * 0.6f;
        btn.colors = colors;
        btn.onClick.AddListener(onClick);

        var rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;

        var textObj = new GameObject("Label");
        textObj.transform.SetParent(innerObj.transform, false);
        var txt = textObj.AddComponent<Text>();
        txt.text = label;
        txt.fontSize = 26;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontStyle = FontStyle.Bold;
        var trt = textObj.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
    }
}
