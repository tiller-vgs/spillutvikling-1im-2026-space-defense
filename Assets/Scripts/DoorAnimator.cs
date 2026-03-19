using UnityEngine;

public class DoorAnimator : MonoBehaviour
{
    Sprite[] frames;
    SpriteRenderer sr;
    float timer;
    public float frameRate = 0.15f;
    int currentFrame = 0;
    bool opening = true;

    void Start()
    {
        sr = gameObject.GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 1;
        
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
        timer += Time.deltaTime;
        if (timer >= frameRate)
        {
            timer = 0;
            if (opening)
            {
                currentFrame++;
                if (currentFrame >= frames.Length - 1)
                {
                    currentFrame = frames.Length - 1;
                    opening = false;
                }
            }
            else
            {
                currentFrame--;
                if (currentFrame <= 0)
                {
                    currentFrame = 0;
                    opening = true;
                }
            }
            
            if (frames[currentFrame] != null)
                sr.sprite = frames[currentFrame];
        }
    }
}
