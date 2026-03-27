using UnityEngine;

[System.Serializable]
public class EnemyColor
{
    public float r, g, b;
    public Color ToColor() { return new Color(r, g, b); }
}

[System.Serializable]
public class EnemyData
{
    public string id;
    public string name;
    public float health;
    public float speed;
    public float size;
    public EnemyColor color;
    public int reward;
}

[System.Serializable]
public class EnemyDataList
{
    public EnemyData[] enemies;
}

// Laster inn og lagre fiendestats og data fra JSON fil sånn at det er tilgjengelig i hele spillet
public class EnemyDatabase : MonoBehaviour
{
    public static EnemyDatabase instance;
    public EnemyData[] enemies;

    void Awake()
    {
        instance = this;
        var json = Resources.Load<TextAsset>("enemies");
        if (json != null)
        {
            var list = JsonUtility.FromJson<EnemyDataList>(json.text);
            enemies = list.enemies;
        }
    }

    public EnemyData GetEnemy(string id)
    {
        // søk etter matching enemy id fallback te første
        foreach (var e in enemies)
            if (e.id == id) return e;
            
        return enemies[0];
    }
}
