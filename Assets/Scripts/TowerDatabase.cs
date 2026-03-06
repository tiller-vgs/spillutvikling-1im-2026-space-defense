using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TowerColor
{
    public float r, g, b;
    public Color ToColor() { return new Color(r, g, b); }
}

[System.Serializable]
public class TowerData
{
    public string id;
    public string name;
    public int damage;
    public float range;
    public float fireRate;
    public int cost;
    public int upgradeCost;
    public int upgradeDamage;
    public TowerColor color;
    public string description;
}

[System.Serializable]
public class TowerDataList
{
    public TowerData[] towers;
}

public class TowerDatabase : MonoBehaviour
{
    public static TowerDatabase instance;
    private Dictionary<string, TowerData> towerDict = new Dictionary<string, TowerData>();
    private TowerData[] allTowers;

    void Awake()
    {
        instance = this;
        TextAsset json = Resources.Load<TextAsset>("towers");
        if (json != null)
        {
            TowerDataList list = JsonUtility.FromJson<TowerDataList>(json.text);
            allTowers = list.towers;
            foreach (var t in allTowers)
                towerDict[t.id] = t;
        }
    }

    public TowerData GetTower(string id)
    {
        if (towerDict.ContainsKey(id)) return towerDict[id];
        return null;
    }

    public TowerData[] GetAll()
    {
        return allTowers;
    }
}
