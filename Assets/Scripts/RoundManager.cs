using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RoundManager : MonoBehaviour
{
    public static RoundManager instance;

    public int currentRound = 0;
    public bool autoStart = false;
    public bool roundActive = false;

    private int enemiesAlive = 0;
    private int enemiesSpawned = 0;
    private int enemiesToSpawn = 0;
    private float spawnTimer = 0;

    private Text roundText;
    private Text roundAnnouncerText;
    private GameObject startButton;
    private CanvasGroup announcerGroup;

    private EnemySpawner spawner;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        spawner = Object.FindFirstObjectByType<EnemySpawner>();
        BuildUI();
    }

    void BuildUI()
    {
        GameObject canvasObj = new GameObject("RoundCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 95;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Round counter - top left, below money
        GameObject roundBg = new GameObject("RoundBG");
        roundBg.transform.SetParent(canvasObj.transform, false);
        var bgImg = roundBg.AddComponent<Image>();
        bgImg.color = new Color(0.08f, 0.08f, 0.15f, 0.8f);
        var bgRect = roundBg.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 1);
        bgRect.anchorMax = new Vector2(0, 1);
        bgRect.pivot = new Vector2(0, 1);
        bgRect.anchoredPosition = new Vector2(15, -60);
        bgRect.sizeDelta = new Vector2(180, 35);

        GameObject roundObj = new GameObject("RoundText");
        roundObj.transform.SetParent(roundBg.transform, false);
        roundText = roundObj.AddComponent<Text>();
        roundText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        roundText.fontSize = 20;
        roundText.fontStyle = FontStyle.Bold;
        roundText.color = new Color(0.3f, 0.7f, 1f);
        roundText.alignment = TextAnchor.MiddleCenter;
        roundText.text = "ROUND 0";
        var rtRect = roundObj.GetComponent<RectTransform>();
        rtRect.anchorMin = Vector2.zero;
        rtRect.anchorMax = Vector2.one;
        rtRect.offsetMin = Vector2.zero;
        rtRect.offsetMax = Vector2.zero;

        // Start round button - bottom right (like Bloons)
        startButton = new GameObject("StartRoundBtn");
        startButton.transform.SetParent(canvasObj.transform, false);
        var startImg = startButton.AddComponent<Image>();
        startImg.color = new Color(0.1f, 0.5f, 0.15f, 0.9f);
        var startRect = startButton.GetComponent<RectTransform>();
        startRect.anchorMin = new Vector2(1, 0);
        startRect.anchorMax = new Vector2(1, 0);
        startRect.pivot = new Vector2(1, 0);
        startRect.anchoredPosition = new Vector2(-215, 15);
        startRect.sizeDelta = new Vector2(150, 50);

        var startBtn = startButton.AddComponent<Button>();
        var colors = startBtn.colors;
        colors.highlightedColor = new Color(0.15f, 0.6f, 0.2f);
        colors.pressedColor = new Color(0.08f, 0.35f, 0.1f);
        startBtn.colors = colors;
        startBtn.onClick.AddListener(() => StartNextRound());

        GameObject startLabel = new GameObject("Label");
        startLabel.transform.SetParent(startButton.transform, false);
        Text startText = startLabel.AddComponent<Text>();
        startText.text = "START ▶";
        startText.fontSize = 22;
        startText.fontStyle = FontStyle.Bold;
        startText.color = Color.white;
        startText.alignment = TextAnchor.MiddleCenter;
        startText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var stRect = startLabel.GetComponent<RectTransform>();
        stRect.anchorMin = Vector2.zero;
        stRect.anchorMax = Vector2.one;
        stRect.offsetMin = Vector2.zero;
        stRect.offsetMax = Vector2.zero;

        // Round announcer (center screen, fading)
        GameObject announcerObj = new GameObject("RoundAnnouncer");
        announcerObj.transform.SetParent(canvasObj.transform, false);
        announcerGroup = announcerObj.AddComponent<CanvasGroup>();
        announcerGroup.alpha = 0;
        var announcerRect = announcerObj.GetComponent<RectTransform>();
        announcerRect.anchorMin = new Vector2(0.5f, 0.5f);
        announcerRect.anchorMax = new Vector2(0.5f, 0.5f);
        announcerRect.sizeDelta = new Vector2(600, 120);
        announcerRect.anchoredPosition = new Vector2(-100, 50);

        GameObject announcerTextObj = new GameObject("AnnouncerText");
        announcerTextObj.transform.SetParent(announcerObj.transform, false);
        roundAnnouncerText = announcerTextObj.AddComponent<Text>();
        roundAnnouncerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        roundAnnouncerText.fontSize = 72;
        roundAnnouncerText.fontStyle = FontStyle.Bold;
        roundAnnouncerText.color = new Color(0.3f, 0.8f, 1f);
        roundAnnouncerText.alignment = TextAnchor.MiddleCenter;
        roundAnnouncerText.horizontalOverflow = HorizontalWrapMode.Overflow;
        var atRect = announcerTextObj.GetComponent<RectTransform>();
        atRect.anchorMin = Vector2.zero;
        atRect.anchorMax = Vector2.one;
        atRect.offsetMin = Vector2.zero;
        atRect.offsetMax = Vector2.zero;
    }

    public void StartNextRound()
    {
        if (roundActive) return;

        currentRound++;
        roundActive = true;
        enemiesSpawned = 0;
        spawnTimer = 0;

        // calculate enemies for this round
        enemiesToSpawn = GetEnemyCount(currentRound);
        enemiesAlive = 0;

        roundText.text = "ROUND " + currentRound;
        startButton.SetActive(false);

        StartCoroutine(ShowRoundAnnouncement("ROUND " + currentRound));
    }

    int GetEnemyCount(int round)
    {
        // starts at 5, grows each round
        return 5 + (round - 1) * 3;
    }

    string GetEnemyTypeForSpawn(int round, int index)
    {
        // every 5 rounds, the last enemy is a boss
        if (round >= 5 && round % 5 == 0 && index == enemiesToSpawn - 1)
            return "boss";

        if (round <= 2)
        {
            // early rounds: basic + some swarm
            return (index % 4 == 0) ? "swarm" : "basic";
        }

        if (round <= 4)
        {
            // mix basic, fast, and swarm
            if (index % 4 == 0) return "fast";
            if (index % 5 == 0) return "swarm";
            return "basic";
        }

        if (round <= 7)
        {
            // introduce stealth and tank
            if (index % 6 == 0) return "tank";
            if (index % 5 == 0) return "stealth";
            if (index % 3 == 0) return "fast";
            if (index % 7 == 0) return "swarm";
            return "basic";
        }

        if (round <= 12)
        {
            // heavy mix
            if (index % 4 == 0) return "tank";
            if (index % 3 == 0) return "stealth";
            if (index % 5 == 0) return "swarm";
            if (index % 2 == 0) return "fast";
            return "basic";
        }

        // round 13+: chaos mode, lots of everything
        int type = index % 6;
        switch (type)
        {
            case 0: return "tank";
            case 1: return "fast";
            case 2: return "stealth";
            case 3: return "swarm";
            case 4: return "basic";
            default: return "fast";
        }
    }

    float GetSpawnDelay(int round)
    {
        // spawns get faster each round
        float delay = 2.0f - (round - 1) * 0.08f;
        return Mathf.Max(delay, 0.4f);
    }

    float GetHealthMultiplier(int round)
    {
        // enemies get +15% health per round
        return 1f + (round - 1) * 0.15f;
    }

    int GetRewardMultiplier(int round)
    {
        // more money for harder rounds
        return 1 + (round - 1) / 3;
    }

    void Update()
    {
        if (!roundActive) return;

        if (enemiesSpawned < enemiesToSpawn)
        {
            spawnTimer += Time.deltaTime;
            float delay = GetSpawnDelay(currentRound);

            if (spawnTimer >= delay)
            {
                SpawnRoundEnemy();
                spawnTimer = 0;
            }
        }
        else
        {
            // all spawned, check if all dead
            EnemyMovement[] alive = Object.FindObjectsByType<EnemyMovement>(FindObjectsSortMode.None);
            if (alive.Length == 0)
            {
                RoundComplete();
            }
        }
    }

    void SpawnRoundEnemy()
    {
        if (spawner == null)
        {
            spawner = Object.FindFirstObjectByType<EnemySpawner>();
            if (spawner == null) return;
        }

        string type = GetEnemyTypeForSpawn(currentRound, enemiesSpawned);
        float healthMult = GetHealthMultiplier(currentRound);
        int rewardMult = GetRewardMultiplier(currentRound);

        spawner.SpawnEnemyOfType(type, healthMult, rewardMult);
        enemiesSpawned++;
    }

    void RoundComplete()
    {
        roundActive = false;

        // bonus money for completing a round
        int bonus = 20 + currentRound * 5;
        if (CurrencyManager.instance != null)
            CurrencyManager.instance.AddMoney(bonus);

        if (autoStart)
        {
            Invoke("StartNextRound", 2f);
        }
        else
        {
            startButton.SetActive(true);
        }
    }

    IEnumerator ShowRoundAnnouncement(string text)
    {
        roundAnnouncerText.text = text;

        // fade in
        float t = 0;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            announcerGroup.alpha = t / 0.3f;
            yield return null;
        }
        announcerGroup.alpha = 1;

        // hold
        yield return new WaitForSeconds(1.0f);

        // fade out
        t = 0;
        while (t < 0.8f)
        {
            t += Time.deltaTime;
            announcerGroup.alpha = 1f - (t / 0.8f);
            yield return null;
        }
        announcerGroup.alpha = 0;
    }
}
