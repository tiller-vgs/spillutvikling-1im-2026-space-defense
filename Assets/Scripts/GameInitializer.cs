using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public class GameInitializer : MonoBehaviour
{
    private static bool isRegistered = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void RegisterSceneCallback()
    {
        if (isRegistered) return;
        isRegistered = true;

        SceneManager.sceneLoaded += (scene, mode) => {
            GameObject initObj = new GameObject("GameInitializer");
            initObj.AddComponent<GameInitializer>().DoInit();
        };
    }

    void Awake()
    {
        // Empty Awake. If the user placed GameInitializer in the scene manually, 
        // it won't do anything, preventing double-initialization.
    }

    public void DoInit()
    {
        // The user likely saved the scene during Play Mode, baking "roundActive=true" and old enemy spawners into the .unity file.
        // We must brutally purge all remnants from the default loaded scene before dynamically building ours, 
        // to prevent phantom enemies from spawning exactly where the last Editor test left off.
        string[] oldObjects = { "UIManager", "GameManager", "EnemySpawner", "BackgroundBase", "Stars", "Nebulas", "EnemyPath", "PathParticles", "EventSystem", "RoundCanvas" };
        foreach (string objName in oldObjects)
        {
            GameObject old = GameObject.Find(objName);
            if (old != null) DestroyImmediate(old); // Immediate eliminates them before this frame's Awake/Start can run!
        }
        
        // Also purge any stray enemies or towers saved in the scene
        foreach (var enemy in Object.FindObjectsByType<EnemyMovement>(FindObjectsSortMode.None)) DestroyImmediate(enemy.gameObject);
        foreach (var tower in Object.FindObjectsByType<Tower>(FindObjectsSortMode.None)) DestroyImmediate(tower.gameObject);

        SetupCamera();
        CreateBackground();
        CreateStars();
        CreateNebulas();
        CreatePath();
        CreatePathParticles();
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
    }

    void CreateBackground()
    {
        GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bg.name = "Background";
        bg.transform.position = new Vector3(0, 0, 5);
        bg.transform.localScale = new Vector3(22, 16, 1);
        var sr = bg.GetComponent<Renderer>();
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(0.03f, 0.01f, 0.08f);
        sr.material = mat;
        KillCollider(bg);
    }

    void CreateStars()
    {
        GameObject starsParent = new GameObject("Stars");

        for (int i = 0; i < 80; i++)
        {
            GameObject star = GameObject.CreatePrimitive(PrimitiveType.Quad);
            star.name = "Star";
            star.transform.parent = starsParent.transform;
            star.transform.position = new Vector3(Random.Range(-10f, 10f), Random.Range(-6f, 6f), 4f);

            float size = Random.Range(0.02f, 0.08f);
            star.transform.localScale = new Vector3(size, size, 1);

            var sr = star.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Sprites/Default"));
            float b = Random.Range(0.4f, 1f);
            float tint = Random.Range(0f, 1f);
            if (tint > 0.7f)
                mat.color = new Color(b, b * 0.9f, b * 0.6f);
            else if (tint > 0.4f)
                mat.color = new Color(b * 0.7f, b * 0.8f, b);
            else
                mat.color = new Color(b, b, b);
            sr.material = mat;

            KillCollider(star);
        }

        starsParent.AddComponent<StarTwinkle>();
    }

    void CreateNebulas()
    {
        GameObject nebParent = new GameObject("Nebulas");

        Color[] nebColors = new Color[]
        {
            new Color(0.2f, 0.05f, 0.3f, 0.08f),
            new Color(0.05f, 0.1f, 0.3f, 0.06f),
            new Color(0.3f, 0.05f, 0.1f, 0.05f),
        };

        Vector3[] nebPositions = new Vector3[]
        {
            new Vector3(-4f, 3f, 3f),
            new Vector3(5f, -2f, 3f),
            new Vector3(1f, -4f, 3f),
        };

        for (int i = 0; i < nebColors.Length; i++)
        {
            GameObject neb = GameObject.CreatePrimitive(PrimitiveType.Quad);
            neb.name = "Nebula_" + i;
            neb.transform.parent = nebParent.transform;
            neb.transform.position = nebPositions[i];
            neb.transform.localScale = new Vector3(Random.Range(4f, 7f), Random.Range(3f, 5f), 1);
            neb.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

            var sr = neb.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = nebColors[i];
            sr.material = mat;
            KillCollider(neb);
        }
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

        GameObject pathObj = new GameObject("EnemyPath");
        pathObj.transform.position = Vector3.zero;

        EnemyPath pathScript = pathObj.AddComponent<EnemyPath>();

        Transform[] wps = new Transform[positions.Length];
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject wp = new GameObject("WP_" + i);
            wp.transform.parent = pathObj.transform;
            wp.transform.position = positions[i];
            wps[i] = wp.transform;
        }
        pathScript.waypoints = wps;

        // main path line
        LineRenderer line = pathObj.AddComponent<LineRenderer>();
        line.positionCount = positions.Length;
        line.SetPositions(positions);
        line.startWidth = 0.5f;
        line.endWidth = 0.5f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = new Color(0.1f, 0.4f, 0.9f, 0.4f);
        line.endColor = new Color(0.6f, 0.1f, 0.9f, 0.4f);
        line.sortingOrder = 2;
        line.useWorldSpace = true;
        line.numCapVertices = 4;
        line.numCornerVertices = 4;

        // glow line
        GameObject glowObj = new GameObject("PathGlow");
        glowObj.transform.parent = pathObj.transform;
        glowObj.transform.localPosition = Vector3.zero;

        LineRenderer glowLine = glowObj.AddComponent<LineRenderer>();
        glowLine.positionCount = positions.Length;
        glowLine.SetPositions(positions);
        glowLine.startWidth = 1.0f;
        glowLine.endWidth = 1.0f;
        glowLine.material = new Material(Shader.Find("Sprites/Default"));
        glowLine.startColor = new Color(0.1f, 0.3f, 0.8f, 0.1f);
        glowLine.endColor = new Color(0.5f, 0.1f, 0.8f, 0.1f);
        glowLine.sortingOrder = 1;
        glowLine.useWorldSpace = true;
        glowLine.numCapVertices = 6;
        glowLine.numCornerVertices = 6;

        // border line
        GameObject borderObj = new GameObject("PathBorder");
        borderObj.transform.parent = pathObj.transform;
        borderObj.transform.localPosition = Vector3.zero;

        LineRenderer borderLine = borderObj.AddComponent<LineRenderer>();
        borderLine.positionCount = positions.Length;
        Vector3[] borderPositions = new Vector3[positions.Length];
        for (int i = 0; i < positions.Length; i++)
            borderPositions[i] = positions[i] + new Vector3(0, 0, 0.1f);
        borderLine.SetPositions(borderPositions);
        borderLine.startWidth = 0.7f;
        borderLine.endWidth = 0.7f;
        borderLine.material = new Material(Shader.Find("Sprites/Default"));
        borderLine.startColor = new Color(0.05f, 0.15f, 0.4f, 0.6f);
        borderLine.endColor = new Color(0.3f, 0.05f, 0.4f, 0.6f);
        borderLine.sortingOrder = 1;
        borderLine.useWorldSpace = true;
        borderLine.numCapVertices = 4;
        borderLine.numCornerVertices = 4;
    }

    void CreatePathParticles()
    {
        GameObject pathObj = GameObject.Find("EnemyPath");
        if (pathObj == null) return;

        GameObject particleObj = new GameObject("PathParticles");
        particleObj.transform.parent = pathObj.transform;
        particleObj.transform.localPosition = Vector3.zero;

        var ps = particleObj.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.startLifetime = 2f;
        main.startSpeed = 0.3f;
        main.startSize = 0.08f;
        main.startColor = new Color(0.3f, 0.6f, 1f, 0.5f);
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = true;
        main.duration = 5f;

        var emission = ps.emission;
        emission.rateOverTime = 15;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.SingleSidedEdge;
        shape.radius = 8f;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 1, 1, 0));

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(0.3f, 0.6f, 1f), 0), new GradientColorKey(new Color(0.6f, 0.2f, 1f), 1) },
            new GradientAlphaKey[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(0.5f, 0.3f), new GradientAlphaKey(0, 1) }
        );
        colorOverLifetime.color = grad;

        var renderer = particleObj.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.sortingOrder = 3;

        ps.Play();
    }

    void SetupSpawner()
    {
        GameObject spawnerObj = new GameObject("EnemySpawner");
        EnemySpawner spawner = spawnerObj.AddComponent<EnemySpawner>();

        EnemyPath path = Object.FindFirstObjectByType<EnemyPath>();
        if (path != null)
            spawner.path = path;
    }

    void SetupDatabase()
    {
        GameObject dbObj = new GameObject("GameManager");
        dbObj.AddComponent<EnemyDatabase>();
        dbObj.AddComponent<TowerDatabase>();
        dbObj.AddComponent<PlayerHealth>();
    }

    void SetupUI()
    {
        GameObject uiObj = new GameObject("UIManager");
        uiObj.AddComponent<MainMenu>();
        uiObj.AddComponent<GameOverUI>();
        uiObj.AddComponent<FPSCounter>();
        uiObj.AddComponent<PauseMenu>();
        uiObj.AddComponent<CurrencyManager>();
        uiObj.AddComponent<TowerShop>();
        uiObj.AddComponent<TowerPlacement>();
        uiObj.AddComponent<RoundManager>();

        // EventSystem for button clicks
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<InputSystemUIInputModule>();
    }

    void KillCollider(GameObject obj)
    {
        var col = obj.GetComponent<Collider>();
        if (col != null) Destroy(col);
    }
}
