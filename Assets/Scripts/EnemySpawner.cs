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

        // build the enemy from scratch (root for movement)
        GameObject enemy = new GameObject(data.name);

        // Visuals child (for sprite and animation offset)
        GameObject visuals = new GameObject("Visuals");
        visuals.transform.parent = enemy.transform;
        
        // Push the visuals up slightly so Otters stand "on" the path line
        visuals.transform.localPosition = new Vector3(0, 0.1f, 0);

        // Load sprite from Resources instead of making a square
        var sr = visuals.AddComponent<SpriteRenderer>();
        
        // Unity Resources.Load automatically strips file extensions, but we must be exact about the name.
        Sprite[] sprites = Resources.LoadAll<Sprite>("OterSheet");
        Sprite enemySprite = null;
        
        if (sprites != null && sprites.Length > 0)
        {
            foreach (var s in sprites)
            {
                if (s.name == "Oter_0")
                    enemySprite = s;
            }
            if (enemySprite == null) enemySprite = sprites[0];
        }

        if (enemySprite != null)
        {
            sr.sprite = enemySprite;
        }
        else
        {
            Debug.LogError("Failed to load Oter sprite! Sprites array was null or empty: " + (sprites == null ? "null" : sprites.Length.ToString()));
            sr.sprite = MakeSquareSprite(); // fallback
        }
        var animator = visuals.AddComponent<Animator>();
        RuntimeAnimatorController animCtrl = Resources.Load<RuntimeAnimatorController>("Oter_0");
        if (animCtrl != null)
        {
            animator.runtimeAnimatorController = animCtrl;
        }

        // Set color only if it's the fallback square sprite to preserve the Otter's natural colors
        if (enemySprite == null)
        {
            sr.color = data.color.ToColor();
        }
        else
        {
            sr.color = Color.white; // Ensure original spritesheet colors show unaltered
        }
        sr.sortingOrder = 5;

        // inner glow sprite removed so it doesn't draw a box over the Otter

        // health bar above the enemy
        GameObject hbObj = new GameObject("HealthBar");
        hbObj.transform.parent = enemy.transform;
        hbObj.transform.localPosition = new Vector3(0, 0.5f, 0);
        hbObj.AddComponent<HealthBar>();

        // trail effect removed to avoid square trails

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
