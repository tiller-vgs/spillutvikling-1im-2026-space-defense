using UnityEngine;

// Responsible for instantiating enemy GameObjects, assigning their visual sprites, and attaching their behavior scripts.
public class EnemySpawner : MonoBehaviour
{
    public EnemyPath path;

    public void SpawnEnemyOfType(string type, float healthMultiplier, int rewardMultiplier)
    {
        SpawnEnemyOfTypeWithServerId(type, healthMultiplier, rewardMultiplier, "");
    }

    public void SpawnEnemyOfTypeWithServerId(string type, float healthMultiplier, int rewardMultiplier, string serverId)
    {
        if (path == null)
        {
            path = Object.FindFirstObjectByType<EnemyPath>();
            if (path == null) return;
        }
        
        if (EnemyDatabase.instance == null) return;

        EnemyData data = EnemyDatabase.instance.GetEnemy(type);

        var enemy = new GameObject(data.name);

        var visuals = new GameObject("Visuals");
        visuals.transform.parent = enemy.transform;
        visuals.transform.localPosition = new Vector3(0, 0.1f, 0);

        var sr = visuals.AddComponent<SpriteRenderer>();

        Sprite[] sprites = Resources.LoadAll<Sprite>("Assets/Image/OterSheet");
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
            sr.sprite = MakeSquareSprite();
        }

        var animator = visuals.AddComponent<Animator>();
        RuntimeAnimatorController animCtrl = Resources.Load<RuntimeAnimatorController>("Oter_0");
        if (animCtrl != null)
            animator.runtimeAnimatorController = animCtrl;

        if (enemySprite == null)
            sr.color = data.color.ToColor();
        else
            sr.color = Color.white;

        sr.sortingOrder = 5;

        var hbObj = new GameObject("HealthBar");
        hbObj.transform.parent = enemy.transform;
        hbObj.transform.localPosition = new Vector3(0, 0.5f, 0);
        hbObj.AddComponent<HealthBar>();

        var movement = enemy.AddComponent<EnemyMovement>();
        movement.Setup(data, path.waypoints, healthMultiplier, rewardMultiplier);

        if (serverId != "")
            movement.serverEnemyId = serverId;
    }

    // Hjelpefunksjon som generere en firkant sprite tekstur i reserve hvis ingen sprite blir funnet
    Sprite MakeSquareSprite()
    {
        int size = 32;
        var tex = new Texture2D(size, size);
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
