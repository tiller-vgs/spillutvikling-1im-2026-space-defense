using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public class GameInitializer : MonoBehaviour
{
    static bool isRegistered = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void RegisterSceneCallback()
    {
        if (isRegistered) return;
        isRegistered = true;

        SceneManager.sceneLoaded += (scene, mode) => {
            var initObj = new GameObject("GameInitializer");
            initObj.AddComponent<GameInitializer>().DoInit();
        };
    }

    void Awake() { }

    public void DoInit()
    {
        SetupMusic();

        string[] oldObjects = { "UIManager", "GameManager", "EnemySpawner", "BackgroundBase", "Background", "Stars", "Nebulas", "EnemyPath", "PathParticles", "EventSystem", "RoundCanvas", "SpawnDoor", "BaseDoor", "HealthCanvas" };
        foreach (string objName in oldObjects)
        {
            var old = GameObject.Find(objName);
            if (old != null) DestroyImmediate(old);
        }

        foreach (var enemy in Object.FindObjectsByType<EnemyMovement>(FindObjectsSortMode.None)) DestroyImmediate(enemy.gameObject);
        foreach (var tower in Object.FindObjectsByType<Tower>(FindObjectsSortMode.None)) DestroyImmediate(tower.gameObject);

        SetupCamera();
        CreateBackground();
        CreatePath();
        SetupSpawner();
        SetupDatabase();
        SetupUI();
    }

    void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        cam.backgroundColor = new Color(0.02f, 0.01f, 0.06f);
        cam.orthographicSize = 6;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.transform.position = new Vector3(0, 0, -10);
        SettingsManager.ApplyFullscreen();
    }

    void CreateBackground()
    {
        var bg = new GameObject("Background");
        var vp = bg.AddComponent<UnityEngine.Video.VideoPlayer>();
        vp.playOnAwake = true;
        vp.isLooping = true;
        vp.renderMode = UnityEngine.Video.VideoRenderMode.CameraFarPlane;
        vp.targetCamera = Camera.main;
        vp.clip = Resources.Load<UnityEngine.Video.VideoClip>("GameBk");
        vp.audioOutputMode = UnityEngine.Video.VideoAudioOutputMode.None;
    }



    void CreatePath()
    {
        Vector3[] positions = new Vector3[]
        {
            new Vector3(-9.5f, 4.5f, 0),
            new Vector3(-6f, 4.5f, 0),
            new Vector3(-6f, 1.5f, 0),
            new Vector3(-3f, 1.5f, 0),
            new Vector3(-3f, -1.5f, 0),
            new Vector3(-0.5f, -1.5f, 0),
            new Vector3(-0.5f, -4f, 0),
            new Vector3(3f, -4f, 0),
            new Vector3(3f, -1f, 0),
            new Vector3(5.5f, -1f, 0),
            new Vector3(5.5f, 3f, 0),
            new Vector3(8f, 3f, 0),
            new Vector3(8f, 0f, 0),
            new Vector3(9.5f, 0f, 0),
        };

        var pathObj = new GameObject("EnemyPath");
        pathObj.transform.position = Vector3.zero;

        var pathScript = pathObj.AddComponent<EnemyPath>();

        Transform[] wps = new Transform[positions.Length];
        for (int i = 0; i < positions.Length; i++)
        {
            var wp = new GameObject("WP_" + i);
            wp.transform.parent = pathObj.transform;
            wp.transform.position = positions[i];
            wps[i] = wp.transform;
        }
        pathScript.waypoints = wps;

        CreateDoor(positions[0], "SpawnDoor", -90f);
        CreateDoor(positions[positions.Length - 1], "BaseDoor", 90f);
    }

    void CreateDoor(Vector3 pos, string doorName, float rotationZ)
    {
        var doorObj = new GameObject(doorName);
        doorObj.transform.position = pos;
        doorObj.transform.rotation = Quaternion.Euler(0, 0, rotationZ);
        doorObj.transform.localScale = new Vector3(5f, 5f, 1f);
        doorObj.AddComponent<DoorAnimator>();
    }

    void SetupSpawner()
    {
        var spawnerObj = new GameObject("EnemySpawner");
        var spawner = spawnerObj.AddComponent<EnemySpawner>();

        var path = Object.FindFirstObjectByType<EnemyPath>();
        if (path != null)
            spawner.path = path;
    }

    void SetupDatabase()
    {
        var dbObj = new GameObject("GameManager");
        dbObj.AddComponent<EnemyDatabase>();
        dbObj.AddComponent<TowerDatabase>();
        dbObj.AddComponent<PlayerHealth>();
    }

    void SetupUI()
    {
        var uiObj = new GameObject("UIManager");
        uiObj.AddComponent<MainMenu>();
        uiObj.AddComponent<GameOverUI>();
        uiObj.AddComponent<FPSCounter>();
        uiObj.AddComponent<PauseMenu>();
        uiObj.AddComponent<CurrencyManager>();
        uiObj.AddComponent<TowerShop>();
        uiObj.AddComponent<TowerPlacement>();
        uiObj.AddComponent<RoundManager>();
        uiObj.AddComponent<ServerManager>();

        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<InputSystemUIInputModule>();
    }

    void SetupMusic()
    {
        if (GameMusic.instance != null) return;
        var musicObj = new GameObject("GameMusic");
        musicObj.AddComponent<GameMusic>();
    }

    void KillCollider(GameObject obj)
    {
        var col = obj.GetComponent<Collider>();
        if (col != null) Destroy(col);
    }
}
