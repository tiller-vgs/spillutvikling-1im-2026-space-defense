using UnityEngine;

public class HealthBar : MonoBehaviour
{
    private Transform bar;
    private SpriteRenderer barRenderer;
    private SpriteRenderer bgRenderer;
    private float targetFill = 1f;
    private float currentFill = 1f;

    void Awake()
    {
        Sprite cleanSprite = MakeCleanSprite();

        // background
        GameObject bgObj = new GameObject("HealthBG");
        bgObj.transform.parent = transform;
        bgObj.transform.localPosition = Vector3.zero;
        bgObj.transform.localScale = new Vector3(0.6f, 0.08f, 1f);
        bgRenderer = bgObj.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = cleanSprite;
        bgRenderer.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
        bgRenderer.sortingOrder = 10;

        // fill
        GameObject barObj = new GameObject("HealthFill");
        barObj.transform.parent = transform;
        barObj.transform.localPosition = Vector3.zero;
        barObj.transform.localScale = new Vector3(0.6f, 0.06f, 1f);
        barRenderer = barObj.AddComponent<SpriteRenderer>();
        barRenderer.sprite = cleanSprite;
        barRenderer.color = Color.green;
        barRenderer.sortingOrder = 11;
        bar = barObj.transform;
    }

    public void SetHealth(float normalized)
    {
        targetFill = Mathf.Clamp01(normalized);
    }

    void Update()
    {
        currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * 10f);

        if (bar != null)
        {
            bar.localScale = new Vector3(0.6f * currentFill, 0.06f, 1f);
            float offset = (1f - currentFill) * 0.3f;
            bar.localPosition = new Vector3(-offset, 0, 0);
        }

        if (barRenderer != null)
            barRenderer.color = Color.Lerp(Color.red, Color.green, currentFill);

        transform.rotation = Quaternion.identity;
    }

    Sprite MakeCleanSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // smooth rounded edges
                float dx = Mathf.Abs(x - size / 2f) / (size / 2f);
                float dy = Mathf.Abs(y - size / 2f) / (size / 2f);
                float dist = Mathf.Max(dx, dy);
                float alpha = Mathf.Clamp01((1f - dist) * 8f);
                pixels[y * size + x] = new Color(1, 1, 1, alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
