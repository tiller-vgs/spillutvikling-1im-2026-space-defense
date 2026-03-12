using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TowerUpgrader : MonoBehaviour
{
    private Tower tower;
    private GameObject upgradePanel;
    private static TowerUpgrader activePanel;
    private bool justOpened = false;

    void Start()
    {
        tower = GetComponent<Tower>();
    }

    void Update()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mouseWorld.z = 0;

            float dist = Vector3.Distance(mouseWorld, transform.position);
            if (dist < 0.4f)
            {
                if (upgradePanel == null)
                {
                    if (activePanel != null && activePanel != this)
                        activePanel.HidePanel();
                    ShowUpgradePanel();
                }
            }
            else if (upgradePanel != null && activePanel == this && !justOpened)
            {
                HidePanel();
            }
        }
        justOpened = false;
    }

    void ShowUpgradePanel()
    {
        if (tower != null) tower.SetRangeVisible(true);
        activePanel = this;
        justOpened = true;

        upgradePanel = new GameObject("UpgradePanel");
        Canvas canvas = upgradePanel.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 160;
        var scaler = upgradePanel.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        upgradePanel.AddComponent<GraphicRaycaster>();

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        Vector2 canvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            upgradePanel.GetComponent<RectTransform>(), screenPos, null, out canvasPos);

        // panel bg
        GameObject bg = new GameObject("BG");
        bg.transform.SetParent(upgradePanel.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.05f, 0.03f, 0.12f, 0.95f);
        var bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchoredPosition = new Vector2(canvasPos.x, canvasPos.y + 80);
        bgRect.sizeDelta = new Vector2(200, 170);

        // tower name + level
        CreateText(bg.transform, tower.data.name + " Lv." + tower.level + "/" + Tower.MAX_LEVEL, 15,
            new Vector2(0, 65), Color.white);

        // damage
        CreateText(bg.transform, "DMG: " + tower.GetDamage(), 14,
            new Vector2(0, 45), new Color(0.9f, 0.6f, 0.3f));

        // range
        CreateText(bg.transform, "RNG: " + tower.data.range, 14,
            new Vector2(0, 28), new Color(0.6f, 0.8f, 1f));

        // upgrade button (or MAX LEVEL text)
        if (tower.CanUpgrade())
        {
            int cost = tower.GetUpgradeCost();
            bool canAfford = CurrencyManager.instance != null && CurrencyManager.instance.dollars >= cost;

            GameObject btnObj = new GameObject("UpgradeBtn");
            btnObj.transform.SetParent(bg.transform, false);
            var btnImg = btnObj.AddComponent<Image>();
            btnImg.color = canAfford ? new Color(0.1f, 0.5f, 0.2f) : new Color(0.3f, 0.3f, 0.3f);
            var btn = btnObj.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = canAfford ? new Color(0.15f, 0.6f, 0.25f) : new Color(0.35f, 0.35f, 0.35f);
            colors.pressedColor = canAfford ? new Color(0.08f, 0.4f, 0.15f) : new Color(0.25f, 0.25f, 0.25f);
            btn.colors = colors;
            var btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchoredPosition = new Vector2(0, 2);
            btnRect.sizeDelta = new Vector2(175, 35);

            btn.onClick.AddListener(() => DoUpgrade());
            CreateText(btnObj.transform, "UPGRADE $" + cost, 15, Vector2.zero, Color.white);
        }
        else
        {
            CreateText(bg.transform, "MAX LEVEL", 16, new Vector2(0, 2), new Color(1f, 0.85f, 0.2f));
        }

        // sell button
        int sellValue = tower.GetSellValue();
        GameObject sellObj = new GameObject("SellBtn");
        sellObj.transform.SetParent(bg.transform, false);
        var sellImg = sellObj.AddComponent<Image>();
        sellImg.color = new Color(0.6f, 0.15f, 0.1f);
        var sellBtn = sellObj.AddComponent<Button>();
        var sellColors = sellBtn.colors;
        sellColors.highlightedColor = new Color(0.7f, 0.2f, 0.15f);
        sellColors.pressedColor = new Color(0.4f, 0.1f, 0.08f);
        sellBtn.colors = sellColors;
        sellBtn.onClick.AddListener(() => SellTower());
        var sellRect = sellObj.GetComponent<RectTransform>();
        sellRect.anchoredPosition = new Vector2(0, -35);
        sellRect.sizeDelta = new Vector2(175, 35);

        CreateText(sellObj.transform, "SELL $" + sellValue, 15, Vector2.zero, Color.white);

        // close button
        GameObject closeObj = new GameObject("CloseBtn");
        closeObj.transform.SetParent(bg.transform, false);
        var closeImg = closeObj.AddComponent<Image>();
        closeImg.color = new Color(0.3f, 0.3f, 0.4f);
        var closeBtn = closeObj.AddComponent<Button>();
        var closeColors = closeBtn.colors;
        closeColors.highlightedColor = new Color(0.4f, 0.4f, 0.5f);
        closeBtn.colors = closeColors;
        closeBtn.onClick.AddListener(() => HidePanel());
        var closeRect = closeObj.GetComponent<RectTransform>();
        closeRect.anchoredPosition = new Vector2(85, 70);
        closeRect.sizeDelta = new Vector2(28, 28);

        CreateText(closeObj.transform, "X", 14, Vector2.zero, Color.white);
    }

    void DoUpgrade()
    {
        if (!tower.CanUpgrade()) return;
        int cost = tower.GetUpgradeCost();
        if (CurrencyManager.instance != null && CurrencyManager.instance.SpendMoney(cost))
        {
            tower.Upgrade();
            HidePanel();
            ShowUpgradePanel();
        }
    }

    void SellTower()
    {
        int sellValue = tower.GetSellValue();
        if (CurrencyManager.instance != null)
            CurrencyManager.instance.AddMoney(sellValue);

        HidePanel();
        Destroy(gameObject);
    }

    void HidePanel()
    {
        if (tower != null) tower.SetRangeVisible(false);
        if (upgradePanel != null)
            Destroy(upgradePanel);
        upgradePanel = null;
        if (activePanel == this)
            activePanel = null;
    }

    void CreateText(Transform parent, string content, int size, Vector2 pos, Color color)
    {
        GameObject obj = new GameObject("Text");
        obj.transform.SetParent(parent, false);
        Text txt = obj.AddComponent<Text>();
        txt.text = content;
        txt.fontSize = size;
        txt.color = color;
        txt.fontStyle = FontStyle.Bold;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        var rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(200, 25);
    }
}
