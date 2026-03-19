using UnityEngine;

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
        
        frames = new Sprite[6];
        frames[0] = Resources.Load<Sprite>("door1");
        frames[1] = Resources.Load<Sprite>("door2");
        frames[2] = Resources.Load<Sprite>("door3");
        frames[3] = Resources.Load<Sprite>("door4");
        frames[4] = Resources.Load<Sprite>("door 5");
        frames[5] = Resources.Load<Sprite>("door 6");
        
        if (frames[0] != null) sr.sprite = frames[0];
    }

    void Update()
    {
        int targetFrame = 0;
        if (RoundManager.instance != null && RoundManager.instance.roundActive)
        {
            targetFrame = frames.Length - 1;
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
