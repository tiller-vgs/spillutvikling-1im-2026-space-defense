using UnityEngine;

public class StarTwinkle : MonoBehaviour
{
    void Update()
    {
        foreach (Transform star in transform)
        {
            var sr = star.GetComponent<Renderer>();
            if (sr == null) continue;

            float flicker = Mathf.PerlinNoise(star.position.x * 10f, Time.time * 0.5f);
            var c = sr.material.color;
            c.a = Mathf.Lerp(0.3f, 1f, flicker);
            sr.material.color = c;
        }
    }
}
