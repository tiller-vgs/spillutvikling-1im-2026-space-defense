using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public static FPSCounter instance;
    
    private float deltaTime;
    private bool showFPS = false;
    private GUIStyle style;

    void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Toggle(bool on)
    {
        showFPS = on;
    }

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        if (!showFPS) return;

        if (style == null)
        {
            style = new GUIStyle();
            style.fontSize = 18;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.UpperRight;
        }

        int fps = Mathf.RoundToInt(1.0f / deltaTime);
        string text = fps + " FPS";

        // shadow
        GUIStyle shadow = new GUIStyle(style);
        shadow.normal.textColor = Color.black;
        GUI.Label(new Rect(Screen.width - 149, 11, 140, 30), text, shadow);
        
        // colored text based on fps
        if (fps >= 55) style.normal.textColor = Color.green;
        else if (fps >= 30) style.normal.textColor = Color.yellow;
        else style.normal.textColor = Color.red;

        GUI.Label(new Rect(Screen.width - 150, 10, 140, 30), text, style);
    }
}
