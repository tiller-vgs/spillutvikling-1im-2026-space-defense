using UnityEngine;
using System.Collections.Generic;

public class Tower : MonoBehaviour
{
    public TowerData data;
    public int level = 1;
    public const int MAX_LEVEL = 5;
    public string serverTowerId = "";
    float fireTimer;
    LineRenderer laserLine;
    AudioSource audioSource;
    AudioClip laserSound;
    GameObject rangeCircle;
    GameObject gunObj;
    float baseScale = 2.5f;

    Sprite GetCustomSprite()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("tower_new 1");
        string targetRef = "";
        
        switch (data.id)
        {
            case "laser": targetRef = "tower_new 1_2"; break;
            case "cannon": targetRef = "tower_new 1_3"; break;
            case "sniper": targetRef = "tower_new 1_6"; break;
            case "frost": targetRef = "tower_new 1_4"; break;
            case "missile": targetRef = "tower_new 1_5"; break;
        }

        foreach (var s in sprites)
        {
            if (s.name.Equals(targetRef, System.StringComparison.OrdinalIgnoreCase)) return s;
        }
        return null;
    }

    public void Setup(TowerData towerData)
    {
        data = towerData;
        baseScale = 2.5f;

        var sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = GetCustomSprite();
        sr.color = Color.white;
        sr.sortingOrder = 8;
        transform.localScale = Vector3.one * baseScale;

        rangeCircle = new GameObject("Range");
        rangeCircle.transform.parent = transform;
        rangeCircle.transform.localPosition = Vector3.zero;
        var rangeSr = rangeCircle.AddComponent<SpriteRenderer>();
        rangeSr.sprite = MakeCircleSprite();
        Color rc = data.color.ToColor();
        rc.a = 0.05f; // Slightly lower alpha for better transparency
        rangeSr.color = rc;
        rangeSr.sortingOrder = 1;
        rangeCircle.transform.localScale = Vector3.one * (data.range * 2f / baseScale);
        rangeCircle.SetActive(false);

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

        SetupGun();

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = 0.015f;
        audioSource.pitch = 1.8f;
        audioSource.spatialBlend = 0f;

        laserSound = Resources.Load<AudioClip>("Laserlyd");
    }

    void Update()
    {
        fireTimer += Time.deltaTime;

        EnemyMovement target = FindClosestEnemy();
        if (target != null && gunObj != null)
        {
            Vector3 dir = target.transform.position - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            gunObj.transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        }

        float interval = 1f / data.fireRate;
        if (fireTimer >= interval)
        {
            if (target != null)
            {
                target.TakeDamage(data.damage + (level - 1) * data.upgradeDamage);
                if (data.slowAmount > 0)
                {
                    float currentSlow = data.slowAmount + (level - 1) * data.upgradeSlowAmount;
                    target.ApplySlow(currentSlow, 0.5f);
                }
                ShowLaser(target.transform.position);
                if (laserSound != null && audioSource != null)
                    audioSource.PlayOneShot(laserSound);
                fireTimer = 0;
            }
        }

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
        // simplistic distance check
        var enemies = Object.FindObjectsByType<EnemyMovement>(FindObjectsSortMode.None);
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
        transform.localScale = Vector3.one * (baseScale + level * (baseScale * 0.1f));
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c = Color.Lerp(c, Color.white, 0.15f);
            sr.color = c;
        }
    }

    public void SetRangeVisible(bool visible)
    {
        if (rangeCircle != null)
            rangeCircle.SetActive(visible);
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

    Sprite MakeCircleSprite()
    {
        int size = 64;
        var tex = new Texture2D(size, size);
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
                float alpha = Mathf.Max(0, (1f - dist)); // Smoother linear falloff
                pixels[y * size + x] = new Color(1, 1, 1, alpha);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    void SetupGun()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("gun 1");
        Sprite rawSprite = null;
        foreach (var s in sprites)
        {
            if (s.name.Equals("gun 1_16", System.StringComparison.OrdinalIgnoreCase))
            {
                rawSprite = s;
                break;
            }
        }

        if (rawSprite != null)
        {
            Sprite gunSprite = Sprite.Create(rawSprite.texture, rawSprite.rect, new Vector2(0.5f, 0.5f), rawSprite.pixelsPerUnit);

            gunObj = new GameObject("Gun");
            gunObj.transform.parent = transform;
            gunObj.transform.localPosition = new Vector3(0, 0, -0.1f);
            
            float gunScale = 1.5f;
            gunObj.transform.localScale = Vector3.one * gunScale;
            
            var sr = gunObj.AddComponent<SpriteRenderer>();
            sr.sprite = gunSprite;
            sr.color = Color.white;
            sr.sortingOrder = 11;
        }
    }
}
