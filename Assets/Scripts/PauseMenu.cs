using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Video;

// Gjør pause menyen og innstillinger i spillet.
public class PauseMenu : MonoBehaviour
{
    GameObject pauseButton;
    GameObject pausePanel;
    GameObject settingsPanel;
    bool isPaused = false;

    void Start()
    {
        BuildUI();
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    void BuildUI()
    {
        var canvasObj = new GameObject("PauseCanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        pauseButton = new GameObject("PauseBtn");
        pauseButton.transform.SetParent(canvasObj.transform, false);
        pauseButton.AddComponent<Image>().color = new Color(0.05f, 0.15f, 0.2f);

        var btnInner = new GameObject("Inner");
        btnInner.transform.SetParent(pauseButton.transform, false);
        var btnInnerImg = btnInner.AddComponent<Image>();
        btnInnerImg.color = new Color(0.0f, 0.5f, 0.65f, 0.9f);
        var btnInnerRect = btnInner.GetComponent<RectTransform>();
        btnInnerRect.anchorMin = Vector2.zero;
        btnInnerRect.anchorMax = Vector2.one;
        btnInnerRect.offsetMin = new Vector2(3, 3);
        btnInnerRect.offsetMax = new Vector2(-3, -3);

        var btnRect = pauseButton.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1, 1);
        btnRect.anchorMax = new Vector2(1, 1);
        btnRect.pivot = new Vector2(1, 1);
        btnRect.anchoredPosition = new Vector2(-70, -10);
        btnRect.sizeDelta = new Vector2(50, 50);

        var btn = pauseButton.AddComponent<Button>();
        btn.targetGraphic = btnInnerImg;
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.0f, 0.65f, 0.8f);
        colors.pressedColor = new Color(0.0f, 0.35f, 0.5f);
        btn.colors = colors;
        btn.onClick.AddListener(() => TogglePause());

        var pauseIcon = new GameObject("PauseIcon");
        pauseIcon.transform.SetParent(btnInner.transform, false);
        var iconText = pauseIcon.AddComponent<Text>();
        iconText.text = "II";
        iconText.fontSize = 28;
        iconText.color = Color.white;
        iconText.alignment = TextAnchor.MiddleCenter;
        iconText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        iconText.fontStyle = FontStyle.Bold;
        var iconRect = pauseIcon.GetComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;

        pausePanel = new GameObject("PausePanel");
        pausePanel.transform.SetParent(canvasObj.transform, false);
        pausePanel.AddComponent<Image>().color = new Color(0.01f, 0.05f, 0.08f, 0.88f);
        var panelRect = pausePanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        CreateText(pausePanel.transform, "PAUSED", 56,
            new Vector2(0.5f, 0.72f), new Color(0.0f, 0.85f, 0.9f));

        CreateButton(pausePanel.transform, "RESUME", new Vector2(0.5f, 0.55f),
            new Vector2(300, 60), new Color(0.1f, 0.75f, 0.4f), () => Resume());

        CreateButton(pausePanel.transform, "SETTINGS", new Vector2(0.5f, 0.43f),
            new Vector2(300, 60), new Color(0.0f, 0.65f, 0.8f), () => OpenSettings());

        CreateButton(pausePanel.transform, "EXIT TO MENU", new Vector2(0.5f, 0.31f),
            new Vector2(300, 60), new Color(0.1f, 0.35f, 0.5f), () => ExitToMenu());

        BuildSettingsPanel(canvasObj.transform);
    }

    void BuildSettingsPanel(Transform parent)
    {
        settingsPanel = new GameObject("PauseSettingsPanel");
        settingsPanel.transform.SetParent(parent, false);
        settingsPanel.AddComponent<Image>().color = new Color(0.01f, 0.04f, 0.08f, 0.95f);
        var panelRect = settingsPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        CreateText(settingsPanel.transform, "SETTINGS", 48,
            new Vector2(0.5f, 0.85f), new Color(0.0f, 0.85f, 0.9f));

        CreateText(settingsPanel.transform, "Auto-Start Rounds", 22,
            new Vector2(0.35f, 0.72f), Color.white);
        CreateToggle(settingsPanel.transform, new Vector2(0.6f, 0.71f),
            RoundManager.instance != null && RoundManager.instance.autoStart,
            (val) => { if (RoundManager.instance != null) RoundManager.instance.autoStart = val; });

        CreateText(settingsPanel.transform, "FPS Counter", 22,
            new Vector2(0.35f, 0.62f), Color.white);
        CreateToggle(settingsPanel.transform, new Vector2(0.6f, 0.61f),
            SettingsManager.fpsCounterOn,
            (val) => {
                SettingsManager.fpsCounterOn = val;
                SettingsManager.Save();
                if (FPSCounter.instance != null) FPSCounter.instance.Toggle(val);
            });

        CreateText(settingsPanel.transform, "Music", 22,
            new Vector2(0.35f, 0.52f), Color.white);
        CreateToggle(settingsPanel.transform, new Vector2(0.6f, 0.51f),
            SettingsManager.musicOn,
            (val) => {
                if (GameMusic.instance != null) GameMusic.instance.SetMusic(val);
            });

        CreateText(settingsPanel.transform, "Music Volume", 22,
            new Vector2(0.35f, 0.44f), Color.white);

        var pauseVolValObj = CreateText(settingsPanel.transform, Mathf.RoundToInt(SettingsManager.musicVolume * 100) + "%", 22,
            new Vector2(0.72f, 0.44f), new Color(0.1f, 0.9f, 0.5f));
        var pauseVolText = pauseVolValObj.GetComponent<Text>();

        var musicVolSliderObj = new GameObject("MusicVolSlider");
        musicVolSliderObj.transform.SetParent(settingsPanel.transform, false);
        var musicVolSliderRect = musicVolSliderObj.AddComponent<RectTransform>();
        musicVolSliderRect.anchorMin = new Vector2(0.3f, 0.38f);
        musicVolSliderRect.anchorMax = new Vector2(0.7f, 0.40f);
        musicVolSliderRect.offsetMin = Vector2.zero;
        musicVolSliderRect.offsetMax = Vector2.zero;
        musicVolSliderObj.AddComponent<Image>().color = new Color(0.05f, 0.15f, 0.2f);

        var mvFillArea = new GameObject("Fill Area");
        mvFillArea.transform.SetParent(musicVolSliderObj.transform, false);
        var mvFillAreaRect = mvFillArea.AddComponent<RectTransform>();
        mvFillAreaRect.anchorMin = Vector2.zero;
        mvFillAreaRect.anchorMax = Vector2.one;
        mvFillAreaRect.offsetMin = Vector2.zero;
        mvFillAreaRect.offsetMax = Vector2.zero;

        var mvFill = new GameObject("Fill");
        mvFill.transform.SetParent(mvFillArea.transform, false);
        mvFill.AddComponent<Image>().color = new Color(0.0f, 0.65f, 0.8f);
        var mvFillR = mvFill.GetComponent<RectTransform>();
        mvFillR.anchorMin = Vector2.zero;
        mvFillR.anchorMax = Vector2.one;
        mvFillR.offsetMin = Vector2.zero;
        mvFillR.offsetMax = Vector2.zero;

        var mvHandleArea = new GameObject("Handle Slide Area");
        mvHandleArea.transform.SetParent(musicVolSliderObj.transform, false);
        var mvHandleAreaRect = mvHandleArea.AddComponent<RectTransform>();
        mvHandleAreaRect.anchorMin = Vector2.zero;
        mvHandleAreaRect.anchorMax = Vector2.one;
        mvHandleAreaRect.offsetMin = new Vector2(10, 0);
        mvHandleAreaRect.offsetMax = new Vector2(-10, 0);

        var mvHandle = new GameObject("Handle");
        mvHandle.transform.SetParent(mvHandleArea.transform, false);
        mvHandle.AddComponent<Image>().color = new Color(0.1f, 0.9f, 0.5f);
        var mvHandleRect = mvHandle.GetComponent<RectTransform>();
        mvHandleRect.anchorMin = new Vector2(0, 0);
        mvHandleRect.anchorMax = new Vector2(0, 1);
        mvHandleRect.sizeDelta = new Vector2(20, 0);

        var musicVolSlider = musicVolSliderObj.AddComponent<Slider>();
        musicVolSlider.fillRect = mvFillR;
        musicVolSlider.handleRect = mvHandleRect;
        musicVolSlider.minValue = 0;
        musicVolSlider.maxValue = 1;
        musicVolSlider.wholeNumbers = false;
        musicVolSlider.value = SettingsManager.musicVolume;

        musicVolSlider.onValueChanged.AddListener((val) => {
            if (GameMusic.instance != null) GameMusic.instance.SetVolume(val);
            if (pauseVolText != null) pauseVolText.text = Mathf.RoundToInt(val * 100) + "%";
        });

        CreateText(settingsPanel.transform, "Cap FPS", 22,
            new Vector2(0.35f, 0.30f), Color.white);
        Toggle capToggle = CreateToggle(settingsPanel.transform, new Vector2(0.6f, 0.29f),
            SettingsManager.fpsCapped, null);

        CreateText(settingsPanel.transform, "FPS Limit", 22,
            new Vector2(0.35f, 0.22f), Color.white);

        var capValObj = CreateText(settingsPanel.transform, SettingsManager.fpsCap + " FPS", 22,
            new Vector2(0.72f, 0.22f), new Color(0.1f, 0.9f, 0.5f));
        var capValueText = capValObj.GetComponent<Text>();

        var sliderObj = new GameObject("FPSSlider");
        sliderObj.transform.SetParent(settingsPanel.transform, false);
        var sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.3f, 0.16f);
        sliderRect.anchorMax = new Vector2(0.7f, 0.18f);
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

        CreateButton(settingsPanel.transform, "BACK", new Vector2(0.5f, 0.06f),
            new Vector2(240, 55), new Color(0.1f, 0.35f, 0.5f), () => CloseSettings());
    }

    Toggle CreateToggle(Transform parent, Vector2 anchor, bool defaultVal,
        UnityEngine.Events.UnityAction<bool> onChange)
    {
        var toggleObj = new GameObject("Toggle");
        toggleObj.transform.SetParent(parent, false);
        toggleObj.AddComponent<Image>().color = new Color(0.05f, 0.15f, 0.2f);

        var toggleRect = toggleObj.GetComponent<RectTransform>();
        toggleRect.anchorMin = anchor;
        toggleRect.anchorMax = new Vector2(anchor.x, anchor.y + 0.02f);
        toggleRect.sizeDelta = new Vector2(44, 44);

        var innerBg = new GameObject("InnerBg");
        innerBg.transform.SetParent(toggleObj.transform, false);
        innerBg.AddComponent<Image>().color = new Color(0.05f, 0.2f, 0.3f);
        var innerBgRect = innerBg.GetComponent<RectTransform>();
        innerBgRect.anchorMin = Vector2.zero;
        innerBgRect.anchorMax = Vector2.one;
        innerBgRect.offsetMin = new Vector2(3, 3);
        innerBgRect.offsetMax = new Vector2(-3, -3);

        var toggle = toggleObj.AddComponent<Toggle>();

        var checkObj = new GameObject("Checkmark");
        checkObj.transform.SetParent(innerBg.transform, false);
        var checkImg = checkObj.AddComponent<Image>();
        checkImg.color = new Color(0.1f, 0.9f, 0.5f);
        var checkRect = checkObj.GetComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0.2f, 0.2f);
        checkRect.anchorMax = new Vector2(0.8f, 0.8f);
        checkRect.offsetMin = Vector2.zero;
        checkRect.offsetMax = Vector2.zero;

        toggle.graphic = checkImg;
        toggle.isOn = defaultVal;

        if (onChange != null)
            toggle.onValueChanged.AddListener(onChange);

        return toggle;
    }

    void OpenSettings()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    void CloseSettings()
    {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    void TogglePause()
    {
        // Spillets tidsskala mellom 1 og 0 mens menyen vises
        if (isPaused) Resume();
        else Pause();
    }

    void Pause()
    {
        isPaused = true;
        Time.timeScale = 0;
        pausePanel.SetActive(true);
        settingsPanel.SetActive(false);
        pauseButton.SetActive(false);
    }

    void Resume()
    {
        isPaused = false;
        Time.timeScale = 1;
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        pauseButton.SetActive(true);
    }

    void ExitToMenu()
    {
        Time.timeScale = 1;
        foreach (var vp in Object.FindObjectsByType<VideoPlayer>(FindObjectsSortMode.None))
        {
            vp.Stop();
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            TogglePause();
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
        txt.fontStyle = FontStyle.Bold;
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
        highlightObj.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.12f);
        var highlightRect = highlightObj.GetComponent<RectTransform>();
        highlightRect.anchorMin = new Vector2(0, 0.5f);
        highlightRect.anchorMax = new Vector2(1, 1);
        highlightRect.offsetMin = Vector2.zero;
        highlightRect.offsetMax = Vector2.zero;

        var b = btnObj.AddComponent<Button>();
        b.targetGraphic = innerImg;
        var c = b.colors;
        c.highlightedColor = bgColor * 1.3f;
        c.pressedColor = bgColor * 0.6f;
        b.colors = c;
        b.onClick.AddListener(onClick);

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
