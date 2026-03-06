using UnityEngine;
using UnityEditor;

public class MapBuilder : EditorWindow
{
    [MenuItem("Space Defense/Build Map")]
    static void BuildMap()
    {
        SetupCamera();
        CreateBackground();
        CreateStars();
        CreateNebulas();
        CreatePath();
        CreatePathParticles();
        SetupSpawner();
        SetupDatabase();
        SetupUI();
        AddSceneToBuild();

        Debug.Log("Space Defense map built!");
    }

    static void AddSceneToBuild()
    {
        string scenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool found = false;
        foreach (var s in scenes)
            if (s.path == scenePath) { found = true; break; }
        if (!found)
        {
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }

    static void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        cam.backgroundColor = new Color(0.02f, 0.01f, 0.06f);
        cam.orthographicSize = 6;
    }

    static void CreateBackground()
    {
        GameObject bg = FindOrCreate("Background", PrimitiveType.Quad);
        bg.transform.position = new Vector3(0, 0, 5);
        bg.transform.localScale = new Vector3(22, 16, 1);
        var sr = bg.GetComponent<Renderer>();
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(0.03f, 0.01f, 0.08f);
        sr.sharedMaterial = mat;
        KillCollider(bg);
    }

    static void CreateStars()
    {
        GameObject starsParent = FindOrCreateEmpty("Stars");

        // clean old ones
        while (starsParent.transform.childCount > 0)
            DestroyImmediate(starsParent.transform.GetChild(0).gameObject);

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
            // some stars have a slight blue/yellow tint
            float tint = Random.Range(0f, 1f);
            if (tint > 0.7f)
                mat.color = new Color(b, b * 0.9f, b * 0.6f);
            else if (tint > 0.4f)
                mat.color = new Color(b * 0.7f, b * 0.8f, b);
            else
                mat.color = new Color(b, b, b);
            sr.sharedMaterial = mat;

            KillCollider(star);
        }

        // add the twinkle script
        var twinkle = starsParent.GetComponent<StarTwinkle>();
        if (twinkle == null)
            starsParent.AddComponent<StarTwinkle>();
    }

    static void CreateNebulas()
    {
        // big soft colored clouds in the background
        GameObject nebParent = FindOrCreateEmpty("Nebulas");
        while (nebParent.transform.childCount > 0)
            DestroyImmediate(nebParent.transform.GetChild(0).gameObject);

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
            sr.sharedMaterial = mat;
            KillCollider(neb);
        }
    }

    static void CreatePath()
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

        GameObject pathObj = FindOrCreateEmpty("EnemyPath");
        pathObj.transform.position = Vector3.zero;

        EnemyPath pathScript = pathObj.GetComponent<EnemyPath>();
        if (pathScript == null)
            pathScript = pathObj.AddComponent<EnemyPath>();

        while (pathObj.transform.childCount > 0)
            DestroyImmediate(pathObj.transform.GetChild(0).gameObject);

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
        LineRenderer line = pathObj.GetComponent<LineRenderer>();
        if (line == null)
            line = pathObj.AddComponent<LineRenderer>();

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

        // glow line (wider, more transparent)
        GameObject glowObj = FindOrCreateEmpty("PathGlow");
        glowObj.transform.parent = pathObj.transform;
        glowObj.transform.localPosition = Vector3.zero;

        LineRenderer glowLine = glowObj.GetComponent<LineRenderer>();
        if (glowLine == null)
            glowLine = glowObj.AddComponent<LineRenderer>();

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

        // path border line (slightly wider, darker)
        GameObject borderObj = FindOrCreateEmpty("PathBorder");
        borderObj.transform.parent = pathObj.transform;
        borderObj.transform.localPosition = Vector3.zero;

        LineRenderer borderLine = borderObj.GetComponent<LineRenderer>();
        if (borderLine == null)
            borderLine = borderObj.AddComponent<LineRenderer>();

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

    static void CreatePathParticles()
    {
        GameObject pathObj = GameObject.Find("EnemyPath");
        if (pathObj == null) return;

        // floating particles along the path
        GameObject particleObj = FindOrCreateEmpty("PathParticles");
        particleObj.transform.parent = pathObj.transform;
        particleObj.transform.localPosition = Vector3.zero;

        var ps = particleObj.GetComponent<ParticleSystem>();
        if (ps == null)
            ps = particleObj.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startLifetime = 2f;
        main.startSpeed = 0.3f;
        main.startSize = 0.08f;
        main.startColor = new Color(0.3f, 0.6f, 1f, 0.5f);
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = true;

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
    }

    static void SetupSpawner()
    {
        GameObject spawnerObj = FindOrCreateEmpty("EnemySpawner");

        EnemySpawner spawner = spawnerObj.GetComponent<EnemySpawner>();
        if (spawner == null)
            spawner = spawnerObj.AddComponent<EnemySpawner>();

        EnemyPath path = GameObject.Find("EnemyPath")?.GetComponent<EnemyPath>();
        if (path != null)
            spawner.path = path;
    }

    static void SetupDatabase()
    {
        GameObject dbObj = FindOrCreateEmpty("GameManager");
        if (dbObj.GetComponent<EnemyDatabase>() == null)
            dbObj.AddComponent<EnemyDatabase>();
        if (dbObj.GetComponent<TowerDatabase>() == null)
            dbObj.AddComponent<TowerDatabase>();
        if (dbObj.GetComponent<PlayerHealth>() == null)
            dbObj.AddComponent<PlayerHealth>();
    }

    static void SetupUI()
    {
        GameObject uiObj = FindOrCreateEmpty("UIManager");
        if (uiObj.GetComponent<MainMenu>() == null)
            uiObj.AddComponent<MainMenu>();
        if (uiObj.GetComponent<GameOverUI>() == null)
            uiObj.AddComponent<GameOverUI>();
        if (uiObj.GetComponent<FPSCounter>() == null)
            uiObj.AddComponent<FPSCounter>();
        if (uiObj.GetComponent<PauseMenu>() == null)
            uiObj.AddComponent<PauseMenu>();
        if (uiObj.GetComponent<CurrencyManager>() == null)
            uiObj.AddComponent<CurrencyManager>();
        if (uiObj.GetComponent<TowerShop>() == null)
            uiObj.AddComponent<TowerShop>();
        if (uiObj.GetComponent<TowerPlacement>() == null)
            uiObj.AddComponent<TowerPlacement>();
        if (uiObj.GetComponent<RoundManager>() == null)
            uiObj.AddComponent<RoundManager>();

        // need an EventSystem for buttons to work
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
    }

    // helpers
    static GameObject FindOrCreate(string name, PrimitiveType type)
    {
        GameObject obj = GameObject.Find(name);
        if (obj == null)
        {
            obj = GameObject.CreatePrimitive(type);
            obj.name = name;
        }
        return obj;
    }

    static GameObject FindOrCreateEmpty(string name)
    {
        GameObject obj = GameObject.Find(name);
        if (obj == null)
        {
            obj = new GameObject(name);
        }
        return obj;
    }

    static void KillCollider(GameObject obj)
    {
        var col = obj.GetComponent<Collider>();
        if (col != null) DestroyImmediate(col);
    }
}
