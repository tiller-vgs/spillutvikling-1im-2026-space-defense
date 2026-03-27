using UnityEngine;
using System.Collections.Generic;

public enum TargetMode
{
    Closest,
    Strongest,
    MostHealth,
    LeastHealth,
    First,
    Last
}

// Gjør spesifike ting av et plssert, inklu aiming, skyte laser, gjør damage, and tracking upgrades
public class Tower : MonoBehaviour
{
    public TowerData data;
    public int level = 1;
    public const int MAX_LEVEL = 5;
    public string serverTowerId = "";
    public TargetMode targetMode = TargetMode.Closest;
    float fireTimer;
    LineRenderer laserLine;
    AudioSource audioSource;
    AudioClip laserSound;
    GameObject rangeCircle;
    GameObject gunObj;
    float baseScale = 2.5f;

    Sprite GetCustomSprite()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("Assets/Image/tower_new 1");
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
        rc.a = 0.05f;
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
        audioSource.pitch = 1.8f;
        audioSource.spatialBlend = 0f;

        if (data != null && data.id == "missile")
        {
            laserSound = Resources.Load<AudioClip>("Assets/Sound/bomb");
            audioSource.volume = 0.05f;
        }
        else
        {
            laserSound = Resources.Load<AudioClip>("Assets/Sound/Laserlyd");
            audioSource.volume = 0.015f;
        }
    }

    void Update()
    {
        fireTimer += Time.deltaTime;

        EnemyMovement target = FindEnemy();
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
                // Skalere skade basert på fiendens størrelse, større fienda tar mer skade, mindre tar mindre
                float baseDmg = data.damage + (level - 1) * data.upgradeDamage;
                float enemyScale = target.transform.localScale.x;
                float sizeMultiplier = 1f;
                if (enemyScale > 2.5f) sizeMultiplier = 1.5f;       // store fienda tar 50% mer
                else if (enemyScale > 1.5f) sizeMultiplier = 1.25f;  // medium store tar 25% mer
                else if (enemyScale < 0.8f) sizeMultiplier = 0.6f;   // veldi små fienda tar 40% mindre
                else if (enemyScale < 1.2f) sizeMultiplier = 0.8f;   // små fienda tar 20% mindre

                target.TakeDamage(baseDmg * sizeMultiplier);
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

    // Skann alle aktive fienda innenfor rekkevidde og velge det beste målet basert på valgt targetMode strategi
    EnemyMovement FindEnemy()
    {
        var enemies = Object.FindObjectsByType<EnemyMovement>(FindObjectsSortMode.None);
        EnemyMovement best = null;
        float bestValue = 0f;
        bool first = true;

        foreach (var e in enemies)
        {
            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist > data.range) continue;

            switch (targetMode)
            {
                case TargetMode.Closest:
                    if (first || dist < bestValue) { best = e; bestValue = dist; }
                    break;
                case TargetMode.Strongest:
                    float dmg = e.maxHealth;
                    if (first || dmg > bestValue) { best = e; bestValue = dmg; }
                    break;
                case TargetMode.MostHealth:
                    float hp = e.GetCurrentHealth();
                    if (first || hp > bestValue) { best = e; bestValue = hp; }
                    break;
                case TargetMode.LeastHealth:
                    float lhp = e.GetCurrentHealth();
                    if (first || lhp < bestValue) { best = e; bestValue = lhp; }
                    break;
                case TargetMode.First:
                    float prog = e.GetProgress();
                    if (first || prog > bestValue) { best = e; bestValue = prog; }
                    break;
                case TargetMode.Last:
                    float progL = e.GetProgress();
                    if (first || progL < bestValue) { best = e; bestValue = progL; }
                    break;
            }
            first = false;
        }
        return best;
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

    public string GetTargetModeLabel()
    {
        switch (targetMode)
        {
            case TargetMode.Closest: return "Closest";
            case TargetMode.Strongest: return "Strongest";
            case TargetMode.MostHealth: return "Most HP";
            case TargetMode.LeastHealth: return "Least HP";
            case TargetMode.First: return "First";
            case TargetMode.Last: return "Last";
            default: return "Closest";
        }
    }

    public void CycleTargetMode()
    {
        int next = ((int)targetMode + 1) % 6;
        targetMode = (TargetMode)next;
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
                float alpha = Mathf.Max(0, (1f - dist));
                pixels[y * size + x] = new Color(1, 1, 1, alpha);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    void SetupGun()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("Assets/Image/gun 1");
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
