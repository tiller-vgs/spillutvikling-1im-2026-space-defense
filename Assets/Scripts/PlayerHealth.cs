using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    private Image healthFill;
    private Text healthText;

    void Awake()
    {
        currentHealth = maxHealth;
        BuildUI();
    }

    void BuildUI()
    {
        GameObject canvas = new GameObject("HealthCanvas");
        Canvas c = canvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 100;
        var scaler = canvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // background bar - bottom center
        GameObject bgObj = new GameObject("HealthBG");
        bgObj.transform.SetParent(canvas.transform, false);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.08f, 0.08f, 0.15f, 0.85f);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 0);
        bgRect.anchorMax = new Vector2(0.5f, 0);
        bgRect.pivot = new Vector2(0.5f, 0);
        bgRect.anchoredPosition = new Vector2(0, 15);
        bgRect.sizeDelta = new Vector2(320, 38);

        // fill bar
        GameObject fillObj = new GameObject("HealthFill");
        fillObj.transform.SetParent(bgObj.transform, false);
        healthFill = fillObj.AddComponent<Image>();
        healthFill.color = new Color(0.2f, 0.9f, 0.3f);
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.pivot = new Vector2(0, 0.5f);
        fillRect.offsetMin = new Vector2(4, 4);
        fillRect.offsetMax = new Vector2(-4, -4);

        // HP label above bar
        GameObject labelObj = new GameObject("HPLabel");
        labelObj.transform.SetParent(bgObj.transform, false);
        Text label = labelObj.AddComponent<Text>();
        label.text = "HP";
        label.fontSize = 16;
        label.fontStyle = FontStyle.Bold;
        label.color = new Color(0.7f, 0.7f, 0.9f);
        label.alignment = TextAnchor.MiddleCenter;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 1);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.pivot = new Vector2(0.5f, 0);
        labelRect.anchoredPosition = new Vector2(0, 2);
        labelRect.sizeDelta = new Vector2(0, 22);

        // health text on the bar
        GameObject textObj = new GameObject("HealthText");
        textObj.transform.SetParent(bgObj.transform, false);
        healthText = textObj.AddComponent<Text>();
        healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        healthText.fontSize = 20;
        healthText.fontStyle = FontStyle.Bold;
        healthText.color = Color.white;
        healthText.alignment = TextAnchor.MiddleCenter;
        healthText.text = currentHealth + " / " + maxHealth;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        float fill = (float)currentHealth / maxHealth;

        if (healthFill != null)
        {
            healthFill.rectTransform.anchorMax = new Vector2(fill, 1);
            healthFill.color = Color.Lerp(new Color(0.9f, 0.2f, 0.2f), new Color(0.2f, 0.9f, 0.3f), fill);
        }
        if (healthText != null)
            healthText.text = currentHealth + " / " + maxHealth;

        if (currentHealth <= 0)
        {
            GameOverUI gameOver = Object.FindFirstObjectByType<GameOverUI>();
            if (gameOver != null)
                gameOver.Show();
        }
    }
}
