using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    private GameObject pauseButton;
    private GameObject pausePanel;
    private GameObject settingsPanel;
    private bool isPaused = false;

    void Start()
    {
        BuildUI();
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    void BuildUI()
    {
        GameObject canvasObj = new GameObject("PauseCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Pause button (top right)
        pauseButton = new GameObject("PauseBtn");
        pauseButton.transform.SetParent(canvasObj.transform, false);
        var btnImg = pauseButton.AddComponent<Image>();
        btnImg.color = new Color(0.15f, 0.15f, 0.25f, 0.8f);
        var btnRect = pauseButton.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1, 1);
        btnRect.anchorMax = new Vector2(1, 1);
        btnRect.pivot = new Vector2(1, 1);
        btnRect.anchoredPosition = new Vector2(-70, -10);
        btnRect.sizeDelta = new Vector2(50, 50);

        var btn = pauseButton.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.25f, 0.25f, 0.4f, 0.9f);
        colors.pressedColor = new Color(0.1f, 0.1f, 0.2f, 0.9f);
        btn.colors = colors;
        btn.onClick.AddListener(() => TogglePause());

        GameObject pauseIcon = new GameObject("PauseIcon");
        pauseIcon.transform.SetParent(pauseButton.transform, false);
        Text iconText = pauseIcon.AddComponent<Text>();
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

        // Pause panel (fullscreen overlay)
        pausePanel = new GameObject("PausePanel");
        pausePanel.transform.SetParent(canvasObj.transform, false);
        var panelImg = pausePanel.AddComponent<Image>();
        panelImg.color = new Color(0.02f, 0.01f, 0.06f, 0.85f);
        var panelRect = pausePanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        CreateText(pausePanel.transform, "PAUSED", 56,
            new Vector2(0.5f, 0.7f), new Color(0.3f, 0.7f, 1f));

        CreateButton(pausePanel.transform, "RESUME", new Vector2(0.5f, 0.55f),
            new Vector2(260, 55), new Color(0.1f, 0.5f, 0.2f), () => Resume());

        CreateButton(pausePanel.transform, "SETTINGS", new Vector2(0.5f, 0.43f),
            new Vector2(260, 55), new Color(0.2f, 0.2f, 0.5f), () => OpenSettings());

        CreateButton(pausePanel.transform, "EXIT TO MENU", new Vector2(0.5f, 0.31f),
            new Vector2(260, 55), new Color(0.4f, 0.15f, 0.15f), () => ExitToMenu());

        // Settings panel (within pause)
        BuildSettingsPanel(canvasObj.transform);
    }

    void BuildSettingsPanel(Transform parent)
    {
        settingsPanel = new GameObject("PauseSettingsPanel");
        settingsPanel.transform.SetParent(parent, false);
        var panelImg = settingsPanel.AddComponent<Image>();
        panelImg.color = new Color(0.03f, 0.02f, 0.08f, 0.95f);
        var panelRect = settingsPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        CreateText(settingsPanel.transform, "SETTINGS", 48,
            new Vector2(0.5f, 0.82f), new Color(0.3f, 0.7f, 1f));

        // Auto-start rounds toggle
        CreateText(settingsPanel.transform, "Auto-Start Rounds", 22,
            new Vector2(0.35f, 0.62f), Color.white);

        GameObject autoToggleObj = new GameObject("AutoStartToggle");
        autoToggleObj.transform.SetParent(settingsPanel.transform, false);
        var autoToggleRect = autoToggleObj.AddComponent<RectTransform>();
        autoToggleRect.anchorMin = new Vector2(0.6f, 0.61f);
        autoToggleRect.anchorMax = new Vector2(0.6f, 0.63f);
        autoToggleRect.sizeDelta = new Vector2(40, 40);

        var autoToggleBg = autoToggleObj.AddComponent<Image>();
        autoToggleBg.color = new Color(0.2f, 0.2f, 0.3f);

        var autoToggle = autoToggleObj.AddComponent<Toggle>();
        bool currentAutoStart = RoundManager.instance != null && RoundManager.instance.autoStart;
        autoToggle.isOn = currentAutoStart;

        GameObject autoCheckObj = new GameObject("Checkmark");
        autoCheckObj.transform.SetParent(autoToggleObj.transform, false);
        var autoCheckImg = autoCheckObj.AddComponent<Image>();
        autoCheckImg.color = new Color(0.3f, 0.9f, 0.4f);
        var autoCheckRect = autoCheckObj.GetComponent<RectTransform>();
        autoCheckRect.anchorMin = new Vector2(0.15f, 0.15f);
        autoCheckRect.anchorMax = new Vector2(0.85f, 0.85f);
        autoCheckRect.offsetMin = Vector2.zero;
        autoCheckRect.offsetMax = Vector2.zero;
        autoToggle.graphic = autoCheckImg;

        autoToggle.onValueChanged.AddListener((val) => {
            if (RoundManager.instance != null)
                RoundManager.instance.autoStart = val;
        });

        // FPS counter toggle
        CreateText(settingsPanel.transform, "FPS Counter", 22,
            new Vector2(0.35f, 0.50f), Color.white);

        GameObject fpsToggleObj = new GameObject("FPSToggle");
        fpsToggleObj.transform.SetParent(settingsPanel.transform, false);
        var fpsToggleRect = fpsToggleObj.AddComponent<RectTransform>();
        fpsToggleRect.anchorMin = new Vector2(0.6f, 0.49f);
        fpsToggleRect.anchorMax = new Vector2(0.6f, 0.51f);
        fpsToggleRect.sizeDelta = new Vector2(40, 40);

        var fpsToggleBg = fpsToggleObj.AddComponent<Image>();
        fpsToggleBg.color = new Color(0.2f, 0.2f, 0.3f);

        var fpsToggle = fpsToggleObj.AddComponent<Toggle>();
        fpsToggle.isOn = false;

        GameObject fpsCheckObj = new GameObject("Checkmark");
        fpsCheckObj.transform.SetParent(fpsToggleObj.transform, false);
        var fpsCheckImg = fpsCheckObj.AddComponent<Image>();
        fpsCheckImg.color = new Color(0.3f, 0.9f, 0.4f);
        var fpsCheckRect = fpsCheckObj.GetComponent<RectTransform>();
        fpsCheckRect.anchorMin = new Vector2(0.15f, 0.15f);
        fpsCheckRect.anchorMax = new Vector2(0.85f, 0.85f);
        fpsCheckRect.offsetMin = Vector2.zero;
        fpsCheckRect.offsetMax = Vector2.zero;
        fpsToggle.graphic = fpsCheckImg;

        fpsToggle.onValueChanged.AddListener((val) => {
            if (FPSCounter.instance != null)
                FPSCounter.instance.Toggle(val);
        });

        // Back button
        CreateButton(settingsPanel.transform, "BACK", new Vector2(0.5f, 0.25f),
            new Vector2(200, 50), new Color(0.4f, 0.15f, 0.15f), () => CloseSettings());
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

    void CreateButton(Transform parent, string label, Vector2 anchor, Vector2 size, Color bgColor,
        UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject("Btn_" + label);
        btnObj.transform.SetParent(parent, false);
        var img = btnObj.AddComponent<Image>();
        img.color = bgColor;
        var b = btnObj.AddComponent<Button>();
        var c = b.colors;
        c.highlightedColor = bgColor * 1.3f;
        c.pressedColor = bgColor * 0.7f;
        b.colors = c;
        b.onClick.AddListener(onClick);

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;

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
