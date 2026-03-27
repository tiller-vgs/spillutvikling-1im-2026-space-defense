using UnityEngine;

// Gjør den visuelle bilde for bilde-animasjon av døran som åpnes og lukkes basert på ka som skjer i runden.
public class DoorAnimator : MonoBehaviour
{
    Sprite[] frames;
    SpriteRenderer sr;
    float timer;
    public float frameRate = 0.15f;
    int currentFrame = 0;
    void Start()
    {
        sr = gameObject.GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 10;
        
        // Last inn alle sprites fra Door spritesheete
        Sprite[] allSprites = Resources.LoadAll<Sprite>("Assets/Image/Door");
        
        // Vi træng 4 bilder Door_0 (lukket), Door_1, Door_2, Door_3 (åpen)
        frames = new Sprite[4];
        foreach (Sprite s in allSprites)
        {
            if (s.name == "Door_0") frames[0] = s;
            else if (s.name == "Door_1") frames[1] = s;
            else if (s.name == "Door_2") frames[2] = s;
            else if (s.name == "Door_3") frames[3] = s;
        }
        
        if (frames[0] != null) sr.sprite = frames[0];
    }

    void Update()
    {
        // Sikt mot bilde 0 når runden ikke er aktiv
        // Sikt mot bilde 3 når runden er aktiv
        int targetFrame = 0;
        if (RoundManager.instance != null && RoundManager.instance.roundActive)
        {
            targetFrame = frames.Length - 1; // Door_3
        }

        if (currentFrame != targetFrame)
        {
            timer += Time.deltaTime;
            if (timer >= frameRate)
            {
                timer = 0;
                if (currentFrame < targetFrame)
                {
                    currentFrame++;
                }
                else if (currentFrame > targetFrame)
                {
                    currentFrame--;
                }
                
                if (frames[currentFrame] != null)
                    sr.sprite = frames[currentFrame];
            }
        }
    }
}
