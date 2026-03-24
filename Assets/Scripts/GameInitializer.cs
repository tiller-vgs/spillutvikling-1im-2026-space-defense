using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

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

        string[] canvasTargets = { 
            "RoundCanvas", "HealthCanvas", "CurrencyCanvas", "ShopCanvas", 
            "PauseCanvas", "GameOverCanvas", "MenuCanvas", "UpgradePanel", 
            "BackgroundCanvas", "TowerPreview" 
        };
        var allCanvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var c in allCanvases)
        {
            foreach (string name in canvasTargets) {
                if (c.name == name) {
                    DestroyImmediate(c.gameObject);
                    break;
                }
            }
        }

        string[] managers = { "UIManager", "GameManager", "EnemySpawner", "EventSystem" };
        var allObjs = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var o in allObjs) {
            if (o == gameObject) continue;
            foreach (string mName in managers) {
                if (o.name == mName) {
                    DestroyImmediate(o);
                    break;
                }
            }
        }

        string[] sceneObjects = { "EnemyPath", "PathParticles", "SpawnDoor", "BaseDoor" };
        foreach (string objName in sceneObjects) {
            var old = GameObject.Find(objName);
            if (old != null) DestroyImmediate(old);
        }

        var initializers = Object.FindObjectsByType<GameInitializer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var init in initializers)
        {
            if (init != this) DestroyImmediate(init.gameObject);
        }

        foreach (var enemy in Object.FindObjectsByType<EnemyMovement>(FindObjectsInactive.Include, FindObjectsSortMode.None)) DestroyImmediate(enemy.gameObject);
        foreach (var tower in Object.FindObjectsByType<Tower>(FindObjectsInactive.Include, FindObjectsSortMode.None)) DestroyImmediate(tower.gameObject);

        SetupCamera();
        CreateBackground();
        CreatePath();
        SetupSpawner();
        SetupDatabase();
        SetupUI();

        Destroy(gameObject);
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
        var bgCanvasObj = new GameObject("BackgroundCanvas");
        var canvas = bgCanvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = Camera.main;
        canvas.planeDistance = 90;
        canvas.sortingOrder = -100;

        var bg = new GameObject("BackgroundVideo");
        bg.transform.SetParent(bgCanvasObj.transform, false);
        var bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        var rawImage = bg.AddComponent<RawImage>();
        rawImage.color = Color.black;

        var vp = bg.AddComponent<VideoPlayer>();
        vp.playOnAwake = false;
        vp.isLooping = true;
        vp.renderMode = VideoRenderMode.APIOnly;
        vp.audioOutputMode = VideoAudioOutputMode.None;
        vp.clip = Resources.Load<VideoClip>("GameBk");
        
        vp.prepareCompleted += (p) => {
            rawImage.texture = p.texture;
            rawImage.color = Color.white;
            p.Play();
        };
        vp.Prepare();
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
