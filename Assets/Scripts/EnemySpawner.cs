using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public EnemyPath path;

    // Called by RoundManager to spawn a specific enemy type with scaling
    public void SpawnEnemyOfType(string type, float healthMultiplier, int rewardMultiplier)
    {
        if (path == null)
        {
            path = Object.FindFirstObjectByType<EnemyPath>();
            if (path == null) return;
        }
        if (EnemyDatabase.instance == null) return;

        EnemyData data = EnemyDatabase.instance.GetEnemy(type);

        // build the enemy from scratch
        GameObject enemy = new GameObject(data.name);

        // square sprite
        var sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = MakeSquareSprite();
        sr.color = data.color.ToColor();
        sr.sortingOrder = 5;

        // inner glow sprite
        GameObject glow = new GameObject("InnerGlow");
        glow.transform.parent = enemy.transform;
        glow.transform.localPosition = Vector3.zero;
        glow.transform.localScale = Vector3.one * 0.6f;
        var glowSr = glow.AddComponent<SpriteRenderer>();
        glowSr.sprite = MakeSquareSprite();
        Color glowColor = data.color.ToColor();
        glowColor = Color.Lerp(glowColor, Color.white, 0.5f);
        glowColor.a = 0.6f;
        glowSr.color = glowColor;
        glowSr.sortingOrder = 6;

        // health bar above the enemy
        GameObject hbObj = new GameObject("HealthBar");
        hbObj.transform.parent = enemy.transform;
        hbObj.transform.localPosition = new Vector3(0, 0.5f, 0);
        hbObj.AddComponent<HealthBar>();

        // trail effect
        var trail = enemy.AddComponent<TrailRenderer>();
        trail.time = 0.3f;
        trail.startWidth = data.size * 0.3f;
        trail.endWidth = 0f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        Color trailColor = data.color.ToColor();
        trailColor.a = 0.4f;
        trail.startColor = trailColor;
        trailColor.a = 0f;
        trail.endColor = trailColor;
        trail.sortingOrder = 4;

        // movement script with scaled stats
        var movement = enemy.AddComponent<EnemyMovement>();
        movement.Setup(data, path.waypoints, healthMultiplier, rewardMultiplier);
    }

    Sprite MakeSquareSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x - size / 2f) / (size / 2f);
                float dy = Mathf.Abs(y - size / 2f) / (size / 2f);
                float edge = Mathf.Max(dx, dy);
                float alpha = Mathf.Clamp01((1f - edge) * 6f);
                pixels[y * size + x] = new Color(1, 1, 1, alpha);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
