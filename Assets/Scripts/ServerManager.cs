using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Text;
using System.Collections;

public class ServerManager : MonoBehaviour
{
    public static ServerManager instance;

    public string serverUrl = "http://65.108.254.225:3000";
    public string sessionId = "";
    public bool connected = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        StartCoroutine(StartSession());
    }

    // SESSION

    IEnumerator StartSession()
    {
        var req = new UnityWebRequest(serverUrl + "/api/session/start", "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes("{}"));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            var resp = JsonUtility.FromJson<SessionStartResponse>(req.downloadHandler.text);
            if (resp.ok)
            {
                sessionId = resp.sessionId;
                connected = true;
                Debug.Log("[Server] Connected. Session: " + sessionId);

                if (resp.state != null)
                    ApplyState(resp.state);
            }
        }
        else
        {
            Debug.LogWarning("[Server] Could not connect: " + req.error);
        }
    }

    // TOWER

    public void PlaceTower(string towerId, float x, float y, Action<TowerPlaceResponse> callback)
    {
        if (!connected) { callback?.Invoke(null); return; }
        StartCoroutine(PlaceTowerRequest(towerId, x, y, callback));
    }

    IEnumerator PlaceTowerRequest(string towerId, float x, float y, Action<TowerPlaceResponse> callback)
    {
        var body = JsonUtility.ToJson(new TowerPlaceRequest { sessionId = sessionId, towerId = towerId, x = x, y = y });
        var req = PostRequest("/api/tower/place", body);
        yield return req.SendWebRequest();

        TowerPlaceResponse resp = null;
        if (req.result == UnityWebRequest.Result.Success)
        {
            resp = JsonUtility.FromJson<TowerPlaceResponse>(req.downloadHandler.text);
            if (resp.ok) SyncMoney(resp.money);
        }
        callback?.Invoke(resp);
    }

    public void UpgradeTower(string serverId, Action<TowerUpgradeResponse> callback)
    {
        if (!connected) { callback?.Invoke(null); return; }
        StartCoroutine(UpgradeTowerRequest(serverId, callback));
    }

    IEnumerator UpgradeTowerRequest(string serverId, Action<TowerUpgradeResponse> callback)
    {
        var body = JsonUtility.ToJson(new TowerActionRequest { sessionId = sessionId, serverId = serverId });
        var req = PostRequest("/api/tower/upgrade", body);
        yield return req.SendWebRequest();

        TowerUpgradeResponse resp = null;
        if (req.result == UnityWebRequest.Result.Success)
        {
            resp = JsonUtility.FromJson<TowerUpgradeResponse>(req.downloadHandler.text);
            if (resp.ok) SyncMoney(resp.money);
        }
        callback?.Invoke(resp);
    }

    public void SellTower(string serverId, Action<TowerSellResponse> callback)
    {
        if (!connected) { callback?.Invoke(null); return; }
        StartCoroutine(SellTowerRequest(serverId, callback));
    }

    IEnumerator SellTowerRequest(string serverId, Action<TowerSellResponse> callback)
    {
        var body = JsonUtility.ToJson(new TowerActionRequest { sessionId = sessionId, serverId = serverId });
        var req = PostRequest("/api/tower/sell", body);
        yield return req.SendWebRequest();

        TowerSellResponse resp = null;
        if (req.result == UnityWebRequest.Result.Success)
        {
            resp = JsonUtility.FromJson<TowerSellResponse>(req.downloadHandler.text);
            if (resp.ok) SyncMoney(resp.money);
        }
        callback?.Invoke(resp);
    }

    // ROUND

    public void StartRound(Action<RoundStartResponse> callback)
    {
        if (!connected) { callback?.Invoke(null); return; }
        StartCoroutine(StartRoundRequest(callback));
    }

    IEnumerator StartRoundRequest(Action<RoundStartResponse> callback)
    {
        var body = JsonUtility.ToJson(new SessionRequest { sessionId = sessionId });
        var req = PostRequest("/api/round/start", body);
        yield return req.SendWebRequest();

        RoundStartResponse resp = null;
        if (req.result == UnityWebRequest.Result.Success)
            resp = JsonUtility.FromJson<RoundStartResponse>(req.downloadHandler.text);
        callback?.Invoke(resp);
    }

    public void ReportEnemyKilled(string enemyServerId, Action<EnemyKilledResponse> callback)
    {
        if (!connected) { callback?.Invoke(null); return; }
        StartCoroutine(EnemyKilledRequest(enemyServerId, callback));
    }

    IEnumerator EnemyKilledRequest(string enemyServerId, Action<EnemyKilledResponse> callback)
    {
        var body = JsonUtility.ToJson(new EnemyActionRequest { sessionId = sessionId, enemyServerId = enemyServerId });
        var req = PostRequest("/api/round/enemy-killed", body);
        yield return req.SendWebRequest();

        EnemyKilledResponse resp = null;
        if (req.result == UnityWebRequest.Result.Success)
        {
            resp = JsonUtility.FromJson<EnemyKilledResponse>(req.downloadHandler.text);
            if (resp.ok) SyncMoney(resp.money);
        }
        callback?.Invoke(resp);
    }

    public void ReportEnemyLeaked(string enemyServerId, Action<EnemyLeakedResponse> callback)
    {
        if (!connected) { callback?.Invoke(null); return; }
        StartCoroutine(EnemyLeakedRequest(enemyServerId, callback));
    }

    IEnumerator EnemyLeakedRequest(string enemyServerId, Action<EnemyLeakedResponse> callback)
    {
        var body = JsonUtility.ToJson(new EnemyActionRequest { sessionId = sessionId, enemyServerId = enemyServerId });
        var req = PostRequest("/api/round/enemy-leaked", body);
        yield return req.SendWebRequest();

        EnemyLeakedResponse resp = null;
        if (req.result == UnityWebRequest.Result.Success)
        {
            resp = JsonUtility.FromJson<EnemyLeakedResponse>(req.downloadHandler.text);
            if (resp.ok) SyncHealth(resp.health);
        }
        callback?.Invoke(resp);
    }

    UnityWebRequest PostRequest(string path, string jsonBody)
    {
        var req = new UnityWebRequest(serverUrl + path, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        return req;
    }

    void SyncMoney(int serverMoney)
    {
        if (CurrencyManager.instance != null)
        {
            CurrencyManager.instance.dollars = serverMoney;
            CurrencyManager.instance.ForceUpdateDisplay();
        }
    }

    void SyncHealth(int serverHealth)
    {
        var ph = FindFirstObjectByType<PlayerHealth>();
        if (ph != null)
        {
            ph.currentHealth = serverHealth;
            ph.ForceUpdateDisplay();
        }
    }

    void ApplyState(GameState state)
    {
        SyncMoney(state.money);
        SyncHealth(state.health);
    }

    [Serializable] public class SessionRequest { public string sessionId; }
    [Serializable] public class SessionStartResponse { public bool ok; public string sessionId; public GameState state; }
    [Serializable] public class GameState { public int money; public int health; public int currentRound; public bool roundActive; public bool gameOver; }

    [Serializable] public class TowerPlaceRequest { public string sessionId; public string towerId; public float x; public float y; }
    [Serializable] public class TowerPlaceResponse { public bool ok; public string error; public string serverId; public int money; }

    [Serializable] public class TowerActionRequest { public string sessionId; public string serverId; }
    [Serializable] public class TowerUpgradeResponse { public bool ok; public string error; public int money; }
    [Serializable] public class TowerSellResponse { public bool ok; public string error; public int money; public int sellValue; }

    [Serializable] public class RoundStartResponse { public bool ok; public string error; public int round; public int totalEnemies; public RoundEnemy[] enemies; }
    [Serializable] public class RoundEnemy { public string serverId; public string type; public string name; public float maxHealth; public float speed; public int reward; }

    [Serializable] public class EnemyActionRequest { public string sessionId; public string enemyServerId; }
    [Serializable] public class EnemyKilledResponse { public bool ok; public string error; public int reward; public int money; public bool roundComplete; public int roundBonus; }
    [Serializable] public class EnemyLeakedResponse { public bool ok; public string error; public int health; public int damage; public bool gameOver; public bool roundComplete; public int roundBonus; public int money; }
}
