using UnityEngine;
using System.Collections.Generic;

public class Tower : MonoBehaviour
{
    public TowerData data;
    public int level = 1;
    public const int MAX_LEVEL = 5;
    private float fireTimer;
    private LineRenderer laserLine;

    public void Setup(TowerData towerData)
    {
        data = towerData;

        // build tower visual
        var sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = MakeTowerSprite();
        sr.color = data.color.ToColor();
        sr.sortingOrder = 8;

        transform.localScale = Vector3.one * 0.5f;

        // inner glow
        GameObject glow = new GameObject("Glow");
        glow.transform.parent = transform;
        glow.transform.localPosition = Vector3.zero;
        glow.transform.localScale = Vector3.one * 0.6f;
        var glowSr = glow.AddComponent<SpriteRenderer>();
        glowSr.sprite = MakeTowerSprite();
        Color gc = data.color.ToColor();
        gc = Color.Lerp(gc, Color.white, 0.5f);
        gc.a = 0.5f;
        glowSr.color = gc;
        glowSr.sortingOrder = 9;

        // range circle
        GameObject rangeObj = new GameObject("Range");
        rangeObj.transform.parent = transform;
        rangeObj.transform.localPosition = Vector3.zero;
        var rangeSr = rangeObj.AddComponent<SpriteRenderer>();
        rangeSr.sprite = MakeCircleSprite();
        Color rc = data.color.ToColor();
        rc.a = 0.08f;
        rangeSr.color = rc;
        rangeSr.sortingOrder = 1;
        rangeObj.transform.localScale = Vector3.one * data.range * 2f / 0.5f;

        // laser line for shooting
        laserLine = gameObject.AddComponent<LineRenderer>();
        laserLine.startWidth = 0.04f;
        laserLine.endWidth = 0.02f;
        laserLine.material = new Material(Shader.Find("Sprites/Default"));
        laserLine.startColor = data.color.ToColor();
        Color endC = data.color.ToColor();
        endC.a = 0.3f;
        laserLine.endColor = endC;
        laserLine.positionCount = 0;
        laserLine.sortingOrder = 7;
    }

    void Update()
    {
        fireTimer += Time.deltaTime;

        float interval = 1f / data.fireRate;
        if (fireTimer >= interval)
        {
            EnemyMovement target = FindClosestEnemy();
            if (target != null)
            {
                target.TakeDamage(data.damage + (level - 1) * data.upgradeDamage);
                ShowLaser(target.transform.position);
                fireTimer = 0;
            }
        }

        // fade laser
        if (laserLine != null && laserLine.positionCount > 0)
        {
            Color c = laserLine.startColor;
            c.a -= Time.deltaTime * 5f;
            if (c.a <= 0)
                laserLine.positionCount = 0;
            else
            {
                laserLine.startColor = c;
                Color ec = laserLine.endColor;
                ec.a = c.a * 0.3f;
                laserLine.endColor = ec;
            }
        }
    }

    void ShowLaser(Vector3 targetPos)
    {
        laserLine.positionCount = 2;
        laserLine.SetPosition(0, transform.position);
        laserLine.SetPosition(1, targetPos);
        Color c = data.color.ToColor();
        c.a = 1f;
        laserLine.startColor = c;
        Color ec = c;
        ec.a = 0.3f;
        laserLine.endColor = ec;
    }

    EnemyMovement FindClosestEnemy()
    {
        EnemyMovement[] enemies = Object.FindObjectsByType<EnemyMovement>(FindObjectsSortMode.None);
        EnemyMovement closest = null;
        float closestDist = data.range;

        foreach (var e in enemies)
        {
            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = e;
            }
        }
        return closest;
    }

    public bool CanUpgrade()
    {
        return level < MAX_LEVEL;
    }

    public void Upgrade()
    {
        if (!CanUpgrade()) return;
        level++;
        // visual feedback
        transform.localScale = Vector3.one * (0.5f + level * 0.05f);
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c = Color.Lerp(c, Color.white, 0.15f);
            sr.color = c;
        }
    }

    public int GetUpgradeCost()
    {
        return data.upgradeCost * level;
    }

    public int GetTotalInvested()
    {
        int total = data.cost;
        for (int i = 1; i < level; i++)
            total += data.upgradeCost * i;
        return total;
    }

    public int GetSellValue()
    {
        return Mathf.RoundToInt(GetTotalInvested() * 0.6f);
    }

    public int GetDamage()
    {
        return data.damage + (level - 1) * data.upgradeDamage;
    }

    Sprite MakeTowerSprite()
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
                // diamond/rhombus shape
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
