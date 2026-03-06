using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    private GameObject menuPanel;
    private GameObject settingsPanel;

    // settings values
    private bool fpsCounterOn = false;
    private bool fpsCapped = true;
    private int fpsCap = 60;

    void Start()
    {
        Application.targetFrameRate = fpsCap;
        Time.timeScale = 0;
        BuildMenuUI();
        BuildSettingsUI();
        settingsPanel.SetActive(false);
    }

    void BuildMenuUI()
    {
        GameObject canvas = new GameObject("MenuCanvas");
        Canvas c = canvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 200;
        var scaler = canvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvas.AddComponent<GraphicRaycaster>();

        menuPanel = new GameObject("MenuPanel");
        menuPanel.transform.SetParent(canvas.transform, false);
        var panelImg = menuPanel.AddComponent<Image>();
        panelImg.color = new Color(0.02f, 0.01f, 0.06f, 0.95f);
        var panelRect = menuPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // title
        CreateText(menuPanel.transform, "SPACE DEFENSE", 64,
            new Vector2(0.5f, 0.75f), new Color(0.3f, 0.7f, 1f));

        // subtitle
        CreateText(menuPanel.transform, "defend the galaxy", 22,
            new Vector2(0.5f, 0.67f), new Color(0.5f, 0.5f, 0.7f));

        // play button
        CreateButton(menuPanel.transform, "PLAY", new Vector2(0.5f, 0.48f),
            new Vector2(280, 55), new Color(0.1f, 0.5f, 0.2f), () => PlayGame());

        // settings button
        CreateButton(menuPanel.transform, "SETTINGS", new Vector2(0.5f, 0.38f),
            new Vector2(280, 55), new Color(0.2f, 0.2f, 0.5f), () => OpenSettings());

        // exit button
        CreateButton(menuPanel.transform, "EXIT GAME", new Vector2(0.5f, 0.28f),
            new Vector2(280, 55), new Color(0.5f, 0.1f, 0.1f), () => ExitGame());

        // settings panel (starts hidden)
        settingsPanel = new GameObject("SettingsPanel");
        settingsPanel.transform.SetParent(canvas.transform, false);
        var spImg = settingsPanel.AddComponent<Image>();
        spImg.color = new Color(0.05f, 0.03f, 0.12f, 0.97f);
        var spRect = settingsPanel.GetComponent<RectTransform>();
        spRect.anchorMin = Vector2.zero;
        spRect.anchorMax = Vector2.one;
        spRect.offsetMin = Vector2.zero;
        spRect.offsetMax = Vector2.zero;
    }

    void BuildSettingsUI()
    {
        CreateText(settingsPanel.transform, "SETTINGS", 48,
            new Vector2(0.5f, 0.82f), new Color(0.3f, 0.7f, 1f));

        // fps counter toggle
        CreateText(settingsPanel.transform, "FPS Counter", 24,
            new Vector2(0.35f, 0.65f), Color.white);

        GameObject fpsToggleObj = new GameObject("FPSToggle");
        fpsToggleObj.transform.SetParent(settingsPanel.transform, false);
        var fpsToggleRect = fpsToggleObj.AddComponent<RectTransform>();
        fpsToggleRect.anchorMin = new Vector2(0.6f, 0.64f);
        fpsToggleRect.anchorMax = new Vector2(0.6f, 0.66f);
        fpsToggleRect.sizeDelta = new Vector2(40, 40);

        var fpsToggleBg = fpsToggleObj.AddComponent<Image>();
        fpsToggleBg.color = new Color(0.2f, 0.2f, 0.3f);

        var fpsToggle = fpsToggleObj.AddComponent<Toggle>();
        fpsToggle.isOn = fpsCounterOn;

        // checkmark
        GameObject checkObj = new GameObject("Checkmark");
        checkObj.transform.SetParent(fpsToggleObj.transform, false);
        var checkImg = checkObj.AddComponent<Image>();
        checkImg.color = new Color(0.3f, 0.9f, 0.4f);
        var checkRect = checkObj.GetComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0.15f, 0.15f);
        checkRect.anchorMax = new Vector2(0.85f, 0.85f);
        checkRect.offsetMin = Vector2.zero;
        checkRect.offsetMax = Vector2.zero;
        fpsToggle.graphic = checkImg;

        fpsToggle.onValueChanged.AddListener((val) => {
            fpsCounterOn = val;
            if (FPSCounter.instance != null)
                FPSCounter.instance.Toggle(val);
        });

        // fps cap toggle
        CreateText(settingsPanel.transform, "Cap FPS", 24,
            new Vector2(0.35f, 0.52f), Color.white);

        GameObject capToggleObj = new GameObject("CapToggle");
        capToggleObj.transform.SetParent(settingsPanel.transform, false);
        var capToggleRect = capToggleObj.AddComponent<RectTransform>();
        capToggleRect.anchorMin = new Vector2(0.6f, 0.51f);
        capToggleRect.anchorMax = new Vector2(0.6f, 0.53f);
        capToggleRect.sizeDelta = new Vector2(40, 40);

        var capToggleBg = capToggleObj.AddComponent<Image>();
        capToggleBg.color = new Color(0.2f, 0.2f, 0.3f);

        var capToggle = capToggleObj.AddComponent<Toggle>();
        capToggle.isOn = fpsCapped;

        GameObject capCheckObj = new GameObject("Checkmark");
        capCheckObj.transform.SetParent(capToggleObj.transform, false);
        var capCheckImg = capCheckObj.AddComponent<Image>();
        capCheckImg.color = new Color(0.3f, 0.9f, 0.4f);
        var capCheckRect = capCheckObj.GetComponent<RectTransform>();
        capCheckRect.anchorMin = new Vector2(0.15f, 0.15f);
        capCheckRect.anchorMax = new Vector2(0.85f, 0.85f);
        capCheckRect.offsetMin = Vector2.zero;
        capCheckRect.offsetMax = Vector2.zero;
        capToggle.graphic = capCheckImg;

        // fps cap slider
        CreateText(settingsPanel.transform, "FPS Limit", 24,
            new Vector2(0.35f, 0.40f), Color.white);

        Text capValueText = null;
        GameObject capValObj = CreateText(settingsPanel.transform, fpsCap + " FPS", 22,
            new Vector2(0.72f, 0.40f), new Color(0.3f, 0.9f, 0.4f));
        capValueText = capValObj.GetComponent<Text>();

        // slider bg
        GameObject sliderObj = new GameObject("FPSSlider");
        sliderObj.transform.SetParent(settingsPanel.transform, false);
        var sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.3f, 0.33f);
        sliderRect.anchorMax = new Vector2(0.7f, 0.35f);
        sliderRect.offsetMin = Vector2.zero;
        sliderRect.offsetMax = Vector2.zero;

        var sliderBgImg = sliderObj.AddComponent<Image>();
        sliderBgImg.color = new Color(0.15f, 0.15f, 0.25f);

        // fill area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        var fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.2f, 0.5f, 0.9f);
        var fillR = fill.GetComponent<RectTransform>();
        fillR.anchorMin = Vector2.zero;
        fillR.anchorMax = Vector2.one;
        fillR.offsetMin = Vector2.zero;
        fillR.offsetMax = Vector2.zero;

        // handle
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObj.transform, false);
        var handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = Vector2.zero;
        handleAreaRect.offsetMax = Vector2.zero;

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        var handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;
        var handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(16, 0);

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.fillRect = fillR;
        slider.handleRect = handleRect;
        slider.minValue = 15;
        slider.maxValue = 240;
        slider.wholeNumbers = true;
        slider.value = fpsCap;

        Text localCapText = capValueText;
        Slider localSlider = slider;

        slider.onValueChanged.AddListener((val) => {
            fpsCap = (int)val;
            if (localCapText != null)
                localCapText.text = fpsCap + " FPS";
            if (fpsCapped)
                Application.targetFrameRate = fpsCap;
        });

        capToggle.onValueChanged.AddListener((val) => {
            fpsCapped = val;
            if (fpsCapped)
                Application.targetFrameRate = fpsCap;
            else
                Application.targetFrameRate = -1;
            localSlider.interactable = val;
        });

        // back button
        CreateButton(settingsPanel.transform, "BACK", new Vector2(0.5f, 0.18f),
            new Vector2(200, 50), new Color(0.4f, 0.15f, 0.15f), () => CloseSettings());
    }

    void PlayGame()
    {
        menuPanel.SetActive(false);
        Time.timeScale = 1;
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
        settingsPanel.SetActive(false);
        Time.timeScale = 0;
    }

    // helper to create text
    GameObject CreateText(Transform parent, string content, int size, Vector2 anchor, Color color)
    {
        GameObject obj = new GameObject("Text_" + content);
        obj.transform.SetParent(parent, false);
        Text txt = obj.AddComponent<Text>();
        txt.text = content;
        txt.fontSize = size;
        txt.color = color;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.sizeDelta = new Vector2(500, 80);
        rt.anchoredPosition = Vector2.zero;
        return obj;
    }

    // helper to create buttons
    void CreateButton(Transform parent, string label, Vector2 anchor, Vector2 size, Color bgColor,
        UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject("Btn_" + label);
        btnObj.transform.SetParent(parent, false);

        var btnImg = btnObj.AddComponent<Image>();
        btnImg.color = bgColor;

        var btn = btnObj.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = bgColor * 1.3f;
        colors.pressedColor = bgColor * 0.7f;
        btn.colors = colors;
        btn.onClick.AddListener(onClick);

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;

        // label
        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(btnObj.transform, false);
        Text txt = textObj.AddComponent<Text>();
        txt.text = label;
        txt.fontSize = 24;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        RectTransform trt = textObj.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
    }
}
