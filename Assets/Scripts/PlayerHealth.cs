using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    Image healthFill;
    Image healthBgBorder;
    Text healthText;
    Text healthLabel;

    void Awake()
    {
        currentHealth = maxHealth;
        BuildUI();
    }

    void BuildUI()
    {
        var canvas = new GameObject("HealthCanvas");
        var c = canvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 100;
        var scaler = canvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        var borderObj = new GameObject("HealthBorder");
        borderObj.transform.SetParent(canvas.transform, false);
        healthBgBorder = borderObj.AddComponent<Image>();
        healthBgBorder.color = new Color(0.2f, 0.6f, 0.3f, 0.4f);
        var borderRect = borderObj.GetComponent<RectTransform>();
        borderRect.anchorMin = new Vector2(0.5f, 1);
        borderRect.anchorMax = new Vector2(0.5f, 1);
        borderRect.pivot = new Vector2(0.5f, 1);
        borderRect.anchoredPosition = new Vector2(0, -8);
        borderRect.sizeDelta = new Vector2(424, 46);

        var bgObj = new GameObject("HealthBG");
        bgObj.transform.SetParent(canvas.transform, false);
        var bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.04f, 0.04f, 0.1f, 0.95f);
        var bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 1);
        bgRect.anchorMax = new Vector2(0.5f, 1);
        bgRect.pivot = new Vector2(0.5f, 1);
        bgRect.anchoredPosition = new Vector2(0, -10);
        bgRect.sizeDelta = new Vector2(420, 42);

        var fillObj = new GameObject("HealthFill");
        fillObj.transform.SetParent(bgObj.transform, false);
        healthFill = fillObj.AddComponent<Image>();
        healthFill.color = new Color(0.2f, 0.9f, 0.3f);
        var fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.pivot = new Vector2(0, 0.5f);
        fillRect.offsetMin = new Vector2(4, 4);
        fillRect.offsetMax = new Vector2(-4, -4);


        var labelObj = new GameObject("HPLabel");
        labelObj.transform.SetParent(bgObj.transform, false);
        healthLabel = labelObj.AddComponent<Text>();
        healthLabel.text = "HP";
        healthLabel.fontSize = 16;
        healthLabel.fontStyle = FontStyle.Bold;
        healthLabel.color = new Color(0.9f, 0.9f, 1f, 0.7f);
        healthLabel.alignment = TextAnchor.MiddleLeft;
        healthLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(0, 1);
        labelRect.pivot = new Vector2(1, 0.5f);
        labelRect.anchoredPosition = new Vector2(-8, 0);
        labelRect.sizeDelta = new Vector2(40, 0);

        var textObj = new GameObject("HealthText");
        textObj.transform.SetParent(bgObj.transform, false);
        healthText = textObj.AddComponent<Text>();
        healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        healthText.fontSize = 22;
        healthText.fontStyle = FontStyle.Bold;
        healthText.color = Color.white;
        healthText.alignment = TextAnchor.MiddleCenter;
        healthText.text = currentHealth + " / " + maxHealth;
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var shadow = textObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.8f);
        shadow.effectDistance = new Vector2(1, -1);
    }

    // Trekke fra liv og vise Game Over skjermen hvis due død
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        RefreshUI();

        if (currentHealth <= 0)
        {
            if (GameOverUI.instance != null)
                GameOverUI.instance.Show();
        }
    }

    public void ForceUpdateDisplay()
    {
        RefreshUI();
        if (currentHealth <= 0)
        {
            if (GameOverUI.instance != null)
                GameOverUI.instance.Show();
        }
    }

    // Oppdatere den visuelle grønn gul rød healthbar og teksten på skjermen
    void RefreshUI()
    {
        float fill = (float)currentHealth / maxHealth;

        if (healthFill != null)
        {
            healthFill.rectTransform.anchorMax = new Vector2(fill, 1);

            Color barColor;
            if (fill > 0.5f)
                barColor = Color.Lerp(new Color(1f, 0.9f, 0.2f), new Color(0.2f, 0.9f, 0.3f), (fill - 0.5f) * 2f);
            else
                barColor = Color.Lerp(new Color(0.9f, 0.15f, 0.15f), new Color(1f, 0.9f, 0.2f), fill * 2f);

            healthFill.color = barColor;
        }

        if (healthBgBorder != null)
        {
            Color borderColor;
            if (fill > 0.5f)
                borderColor = new Color(0.2f, 0.6f, 0.3f, 0.4f);
            else if (fill > 0.25f)
                borderColor = new Color(0.6f, 0.5f, 0.1f, 0.5f);
            else
                borderColor = new Color(0.7f, 0.15f, 0.1f, 0.6f);
            healthBgBorder.color = borderColor;
        }

        if (healthText != null)
            healthText.text = currentHealth + " / " + maxHealth;
    }
}
