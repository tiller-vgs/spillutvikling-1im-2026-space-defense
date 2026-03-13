using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    private GameObject panel;

    void Awake()
    {
        BuildUI();
        panel.SetActive(false);
    }

    void BuildUI()
    {
        GameObject canvas = new GameObject("GameOverCanvas");
        Canvas c = canvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 300;
        var scaler = canvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvas.AddComponent<GraphicRaycaster>();

        panel = new GameObject("GameOverPanel");
        panel.transform.SetParent(canvas.transform, false);
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.05f, 0f, 0f, 0.9f);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // big death text
        CreateText(panel.transform, "YOU DIED", 72,
            new Vector2(0.5f, 0.65f), new Color(0.9f, 0.15f, 0.15f));

        CreateText(panel.transform, "the otters got through...", 24,
            new Vector2(0.5f, 0.55f), new Color(0.6f, 0.3f, 0.3f));

        // restart
        CreateButton(panel.transform, "RESTART", new Vector2(0.5f, 0.4f),
            new Vector2(260, 55), new Color(0.15f, 0.4f, 0.15f), () => RestartGame());

        // exit to menu
        CreateButton(panel.transform, "EXIT TO MENU", new Vector2(0.5f, 0.28f),
            new Vector2(260, 55), new Color(0.4f, 0.15f, 0.15f), () => ExitToMenu());
    }

    public void Show()
    {
        panel.SetActive(true);
        Time.timeScale = 0;
    }

    void RestartGame()
    {
        Time.timeScale = 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    void ExitToMenu()
    {
        Time.timeScale = 1;
        foreach (var vp in Object.FindObjectsByType<UnityEngine.Video.VideoPlayer>(FindObjectsSortMode.None))
        {
            vp.Stop();
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
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
        rt.sizeDelta = new Vector2(600, 100);
        rt.anchoredPosition = Vector2.zero;
        return obj;
    }

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
