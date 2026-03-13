using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class TowerPlacement : MonoBehaviour
{
    private TowerData currentTower;
    private GameObject preview;
    private SpriteRenderer previewSr;
    private bool isPlacing = false;
    private EnemyPath enemyPath;

    void Start()
    {
        enemyPath = Object.FindFirstObjectByType<EnemyPath>();
    }

    public void StartPlacing(TowerData data)
    {
        CancelPlacing();
        currentTower = data;
        isPlacing = true;

        // create preview
        preview = new GameObject("TowerPreview");
        previewSr = preview.AddComponent<SpriteRenderer>();
        previewSr.sprite = MakePreviewSprite();
        previewSr.sortingOrder = 20;
        preview.transform.localScale = Vector3.one * 0.5f;

        // range circle preview
        GameObject rangeObj = new GameObject("RangePreview");
        rangeObj.transform.parent = preview.transform;
        rangeObj.transform.localPosition = Vector3.zero;
        var rangeSr = rangeObj.AddComponent<SpriteRenderer>();
        rangeSr.sprite = MakeCircleSprite();
        rangeSr.sortingOrder = 19;
        rangeObj.transform.localScale = Vector3.one * data.range * 2f / 0.5f;
    }

    public void CancelPlacing()
    {
        isPlacing = false;
        currentTower = null;
        if (preview != null)
            Destroy(preview);
    }

    void Update()
    {
        if (!isPlacing || currentTower == null || preview == null) return;

        // follow mouse
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorld.z = 0;
        preview.transform.position = mouseWorld;

        bool onPath = IsOnPath(mouseWorld);
        bool canAfford = CurrencyManager.instance != null && CurrencyManager.instance.dollars >= currentTower.cost;
        bool tooClose = IsTooCloseToTower(mouseWorld);

        bool canPlace = !onPath && canAfford && !tooClose;

        // color preview
        if (canPlace)
        {
            Color c = currentTower.color.ToColor();
            c.a = 0.7f;
            previewSr.color = c;
            // update range circle
            var rangeRend = preview.GetComponentsInChildren<SpriteRenderer>();
            if (rangeRend.Length > 1)
            {
                Color rc = currentTower.color.ToColor();
                rc.a = 0.15f;
                rangeRend[1].color = rc;
            }
        }
        else
        {
            previewSr.color = new Color(1f, 0.2f, 0.2f, 0.6f);
            var rangeRend = preview.GetComponentsInChildren<SpriteRenderer>();
            if (rangeRend.Length > 1)
                rangeRend[1].color = new Color(1f, 0.2f, 0.2f, 0.1f);
        }

        // click to place
        if (Mouse.current.leftButton.wasPressedThisFrame && !IsPointerOverUI())
        {
            if (canPlace)
                PlaceTower(mouseWorld);
        }
    }

    bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    bool IsOnPath(Vector3 pos)
    {
        if (enemyPath == null)
        {
            enemyPath = Object.FindFirstObjectByType<EnemyPath>();
            if (enemyPath == null) return false;
        }

        Transform[] wps = enemyPath.waypoints;
        if (wps == null) return false;

        float pathWidth = 0.35f;

        for (int i = 0; i < wps.Length - 1; i++)
        {
            Vector3 a = wps[i].position;
            Vector3 b = wps[i + 1].position;
            float dist = DistToSegment(pos, a, b);
            if (dist < pathWidth) return true;
        }
        return false;
    }

    bool IsTooCloseToTower(Vector3 pos)
    {
        Tower[] towers = Object.FindObjectsByType<Tower>(FindObjectsSortMode.None);
        foreach (var t in towers)
        {
            if (Vector3.Distance(pos, t.transform.position) < 0.6f)
                return true;
        }
        return false;
    }

    float DistToSegment(Vector3 p, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        Vector3 ap = p - a;
        float t = Mathf.Clamp01(Vector3.Dot(ap, ab) / Vector3.Dot(ab, ab));
        Vector3 closest = a + ab * t;
        return Vector3.Distance(p, closest);
    }

    void PlaceTower(Vector3 pos)
    {
        if (CurrencyManager.instance == null || !CurrencyManager.instance.SpendMoney(currentTower.cost))
            return;

        GameObject towerObj = new GameObject(currentTower.name);
        towerObj.transform.position = pos;
        Tower tower = towerObj.AddComponent<Tower>();
        tower.Setup(currentTower);

        // add click handler for upgrades
        var col = towerObj.AddComponent<CircleCollider2D>();
        col.radius = 0.3f;
        var upgrader = towerObj.AddComponent<TowerUpgrader>();

        CancelPlacing();
        TowerShop shop = Object.FindFirstObjectByType<TowerShop>();
        if (shop != null) shop.CancelSelection();
    }

    Sprite MakePreviewSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[size * size];
        float center = size / 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x - center) / center;
                float dy = Mathf.Abs(y - center) / center;
                float dist = dx + dy;
                float alpha = Mathf.Clamp01((1f - dist) * 4f);
                pixels[y * size + x] = new Color(1, 1, 1, alpha);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    Sprite MakeCircleSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[size * size];
        float center = size / 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - center) / center;
                float dy = (y - center) / center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01((1f - dist) * 3f);
                pixels[y * size + x] = new Color(1, 1, 1, alpha);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
