using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class TowerShop : MonoBehaviour
{
    public static TowerShop instance;
    GameObject shopPanel;
    string selectedTowerId = null;
    Text selectedLabel;
    TowerPlacement placement;
    RectTransform panelRect;
    Text shopTabText;
    Image shopTabImg;
    GameObject placementHud;
    float currentX = 200f;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        placement = Object.FindFirstObjectByType<TowerPlacement>();
        if (placement == null)
            placement = gameObject.AddComponent<TowerPlacement>();
        BuildShopUI();
    }

    void BuildShopUI()
    {
        var canvasObj = new GameObject("ShopCanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        shopPanel = new GameObject("ShopPanel");
        shopPanel.transform.SetParent(canvasObj.transform, false);
        var panelImg = shopPanel.AddComponent<Image>();
        panelImg.color = new Color(0.04f, 0.03f, 0.1f, 0.95f);
        panelRect = shopPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1, 0);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.pivot = new Vector2(1, 0.5f);
        panelRect.anchoredPosition = new Vector2(currentX, 0);
        panelRect.sizeDelta = new Vector2(240, 0);

        var arrowObj = new GameObject("ShopTab");
        arrowObj.transform.SetParent(shopPanel.transform, false);
        shopTabImg = arrowObj.AddComponent<Image>();
        shopTabImg.color = new Color(0.1f, 0.4f, 0.8f, 0.6f);
        var arRect = arrowObj.GetComponent<RectTransform>();
        arRect.anchorMin = new Vector2(0, 0.5f);
        arRect.anchorMax = new Vector2(0, 0.5f);
        arRect.pivot = new Vector2(1, 0.5f);
        arRect.anchoredPosition = Vector2.zero;
        arRect.sizeDelta = new Vector2(25, 600);

        var shopTextObj = new GameObject("ShopText");
        shopTextObj.transform.SetParent(arrowObj.transform, false);
        shopTabText = shopTextObj.AddComponent<Text>();
        shopTabText.text = "S\n\nH\n\nO\n\nP";
        shopTabText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        shopTabText.fontSize = 18;
        shopTabText.color = new Color(1, 1, 1, 0.9f);
        shopTabText.fontStyle = FontStyle.Bold;
        shopTabText.alignment = TextAnchor.MiddleCenter;
        shopTabText.lineSpacing = 0.6f;
        var stRect = shopTextObj.GetComponent<RectTransform>();
        stRect.anchorMin = Vector2.zero;
        stRect.anchorMax = Vector2.one;
        stRect.sizeDelta = Vector2.zero;

        placementHud = new GameObject("PlacementHUD");
        placementHud.transform.SetParent(canvasObj.transform, false);
        var hudImg = placementHud.AddComponent<Image>();
        hudImg.color = new Color(0, 0, 0, 0.7f);
        var hRect = placementHud.GetComponent<RectTransform>();
        hRect.anchorMin = new Vector2(0.5f, 1);
        hRect.anchorMax = new Vector2(0.5f, 1);
        hRect.pivot = new Vector2(0.5f, 1);
        hRect.anchoredPosition = new Vector2(0, -20);
        hRect.sizeDelta = new Vector2(400, 50);

        var hTextObj = new GameObject("HUDText");
        hTextObj.transform.SetParent(placementHud.transform, false);
        var hText = hTextObj.AddComponent<Text>();
        hText.text = "Right-click to cancel placement";
        hText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hText.fontSize = 20;
        hText.color = new Color(0.3f, 0.8f, 1f, 0.95f);
        hText.alignment = TextAnchor.MiddleCenter;
        hText.fontStyle = FontStyle.Bold;
        var htRect = hTextObj.GetComponent<RectTransform>();
        htRect.anchorMin = Vector2.zero;
        htRect.anchorMax = Vector2.one;
        htRect.sizeDelta = Vector2.zero;
        placementHud.SetActive(false);

        CreateLabel(shopPanel.transform, "TOWERS", 24, new Vector2(100, -25),
            new Color(0.3f, 0.7f, 1f), FontStyle.Bold);

        Invoke("PopulateTowers", 0.1f);
    }

    void PopulateTowers()
    {
        if (TowerDatabase.instance == null) return;

        TowerData[] towers = TowerDatabase.instance.GetAll();
        if (towers == null) return;

        float yPos = -70;
        foreach (var t in towers)
        {
            CreateTowerButton(t, yPos);
            yPos -= 140;
        }

        var selObj = new GameObject("SelectedInfo");
        selObj.transform.SetParent(shopPanel.transform, false);
        selectedLabel = selObj.AddComponent<Text>();
        selectedLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        selectedLabel.fontSize = 14;
        selectedLabel.color = new Color(0.7f, 0.7f, 0.8f);
        selectedLabel.alignment = TextAnchor.UpperCenter;
        selectedLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
        selectedLabel.verticalOverflow = VerticalWrapMode.Overflow;
        var slRect = selObj.GetComponent<RectTransform>();
        slRect.anchorMin = new Vector2(0.5f, 1);
        slRect.anchorMax = new Vector2(0.5f, 1);
        slRect.pivot = new Vector2(0.5f, 1);
        slRect.anchoredPosition = new Vector2(0, yPos);
        slRect.sizeDelta = new Vector2(180, 80);
        selectedLabel.text = "Click tower\nthen click map";
    }

    void CreateTowerButton(TowerData t, float yPos)
    {
        var btnObj = new GameObject("TowerBtn_" + t.id);
        btnObj.transform.SetParent(shopPanel.transform, false);
        var img = btnObj.AddComponent<Image>();
        img.color = new Color(0.1f, 0.08f, 0.18f, 0.95f);
        var rect = btnObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1);
        rect.anchorMax = new Vector2(0.5f, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.anchoredPosition = new Vector2(0, yPos);
        rect.sizeDelta = new Vector2(175, 125);

        var btn = btnObj.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.15f, 0.12f, 0.25f);
        colors.pressedColor = new Color(0.08f, 0.06f, 0.15f);
        colors.selectedColor = new Color(0.15f, 0.15f, 0.3f);
        btn.colors = colors;

        string towerId = t.id;
        btn.onClick.AddListener(() => SelectTower(towerId));

        CreateLabel(btnObj.transform, t.name, 16, new Vector2(87, -12), t.color.ToColor(), FontStyle.Bold);
        CreateLabel(btnObj.transform, "$" + t.cost, 18, new Vector2(87, -34), new Color(0.3f, 1f, 0.4f), FontStyle.Bold);
        CreateLabel(btnObj.transform, "DMG: " + t.damage, 13, new Vector2(87, -56), new Color(0.9f, 0.6f, 0.3f), FontStyle.Normal);
        CreateLabel(btnObj.transform, "RNG: " + t.range, 13, new Vector2(87, -72), new Color(0.6f, 0.8f, 1f), FontStyle.Normal);
        CreateLabel(btnObj.transform, "SPD: " + t.fireRate + "/s", 13, new Vector2(87, -88), new Color(0.8f, 0.8f, 0.5f), FontStyle.Normal);
        CreateLabel(btnObj.transform, t.description, 11, new Vector2(87, -106), new Color(0.5f, 0.5f, 0.6f), FontStyle.Italic);
    }

    void SelectTower(string id)
    {
        TowerData t = TowerDatabase.instance.GetTower(id);
        if (t == null) return;

        // sjekk om du har nokk pæng
        if (CurrencyManager.instance != null && CurrencyManager.instance.dollars < t.cost)
        {
            if (selectedLabel != null)
                selectedLabel.text = "Not enough $!";
            return;
        }

        selectedTowerId = id;
        if (placement != null)
            placement.StartPlacing(t);

        if (selectedLabel != null)
            selectedLabel.text = t.name + "\nClick to place\nRight-click cancel";
    }

    public void CancelSelection()
    {
        selectedTowerId = null;
        if (selectedLabel != null)
            selectedLabel.text = "Click tower\nthen click map";
    }

    void Update()
    {
        float targetX = 200f;
        Vector2 mousePos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        
        bool isOverVisibleTab = mousePos.x > Screen.width - (240f - currentX);
        bool isPlacing = (selectedTowerId != null);
        if (placementHud != null) placementHud.SetActive(isPlacing);

        if (isOverVisibleTab && !isPlacing)
            targetX = 0f;

        currentX = Mathf.Lerp(currentX, targetX, Time.deltaTime * 8f);
        if (panelRect != null)
            panelRect.anchoredPosition = new Vector2(currentX, 0);

        if (shopTabText != null)
        {
            float alpha = Mathf.Clamp01((currentX - 50f) / 100f);
            shopTabText.color = new Color(1, 1, 1, alpha * 0.9f);
            if (shopTabImg != null) shopTabImg.color = new Color(0.1f, 0.4f, 0.8f, alpha * 0.6f);
        }

        if (UnityEngine.InputSystem.Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (placement != null)
                placement.CancelPlacing();
            CancelSelection();
        }
    }

    void CreateLabel(Transform parent, string text, int size, Vector2 pos, Color color, FontStyle style)
    {
        var obj = new GameObject("Lbl_" + text);
        obj.transform.SetParent(parent, false);
        var txt = obj.AddComponent<Text>();
        txt.text = text;
        txt.fontSize = size;
        txt.color = color;
        txt.fontStyle = style;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1);
        rt.anchorMax = new Vector2(0.5f, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(pos.x - 87, pos.y);
        rt.sizeDelta = new Vector2(175, 20);
    }
}
