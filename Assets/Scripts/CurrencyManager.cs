using UnityEngine;
using UnityEngine.UI;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager instance;
    
    public int dollars = 255; // Spiller start pæng

    Text dollarText;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        BuildUI();
        UpdateDisplay();
    }

    void BuildUI()
    {
        var canvas = new GameObject("CurrencyCanvas");
        var c = canvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 100;
        var scaler = canvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        var bg = new GameObject("DollarBG");
        bg.transform.SetParent(canvas.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.08f, 0.08f, 0.15f, 0.8f);
        var bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 1);
        bgRect.anchorMax = new Vector2(0, 1);
        bgRect.pivot = new Vector2(0, 1);
        bgRect.anchoredPosition = new Vector2(15, -15);
        bgRect.sizeDelta = new Vector2(180, 40);

        var textObj = new GameObject("DollarText");
        textObj.transform.SetParent(bg.transform, false);
        dollarText = textObj.AddComponent<Text>();
        dollarText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        dollarText.fontSize = 22;
        dollarText.fontStyle = FontStyle.Bold;
        dollarText.color = new Color(0.3f, 1f, 0.4f);
        dollarText.alignment = TextAnchor.MiddleCenter;
        var trt = textObj.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
    }

    public void AddMoney(int amount)
    {
        dollars += amount;
        UpdateDisplay();
    }

    // Prøver å ta penger. Return false hvis spiller ikke har spenn
    public bool SpendMoney(int amount)
    {
        if (dollars < amount) return false;
        
        dollars -= amount;
        UpdateDisplay();
        return true;
    }

    public void ForceUpdateDisplay()
    {
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (dollarText != null)
            dollarText.text = "$ " + dollars;
    }
}
