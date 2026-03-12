using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float speed = 3f;
    public float maxHealth = 50f;
    public int reward = 10;

    private float currentHealth;
    private Transform[] waypoints;
    private int currentWP = 0;
    private HealthBar healthBar;
    private SpriteRenderer sr;
    private Animator animator;
    private float slowFactor = 1f;
    private float slowTimer = 0f;
    private Color originalColor = Color.white;

    public void Setup(EnemyData data, Transform[] path)
    {
        Setup(data, path, 1f, 1);
    }

    public void Setup(EnemyData data, Transform[] path, float healthMultiplier, int rewardMultiplier)
    {
        speed = data.speed;
        maxHealth = data.health * healthMultiplier;
        reward = data.reward * rewardMultiplier;
        currentHealth = maxHealth;
        waypoints = path;
        transform.position = waypoints[0].position;
        // Multiply by 6 because Otter sprite is only 32 pixels wide (0.32 world units) and was generating microscopic enemies covered by health bars
        transform.localScale = Vector3.one * data.size * 6f;

        sr = GetComponentInChildren<SpriteRenderer>();
        // Only apply color tint if it's the fallback square (which starts white)
        // If it has a sprite other than the default square, we leave it white to show its actual colors.
        if (sr != null && sr.sprite != null && sr.sprite.name == "Square")
            sr.color = data.color.ToColor();

        if (sr != null) originalColor = sr.color;

        // Get the Animator from children
        animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            // Assuming 3.0 is a "normal" base speed, we scale animation speed relatively.
            // A speed of 6.0 will play the animation 2x as fast.
            animator.speed = speed / 3f;
        }

        healthBar = GetComponentInChildren<HealthBar>();
        if (healthBar != null)
            healthBar.SetHealth(1f);
    }

    public void TakeDamage(float dmg)
    {
        currentHealth -= dmg;
        if (healthBar != null)
            healthBar.SetHealth(currentHealth / maxHealth);

        if (currentHealth <= 0)
        {
            // give money
            if (CurrencyManager.instance != null)
                CurrencyManager.instance.AddMoney(reward);

            SpawnDeathEffect();
            Destroy(gameObject);
        }
    }

    public void ApplySlow(float factor, float duration)
    {
        // factor is how much to REDUCE speed (e.g. 0.3 means 30% slower, so 0.7 speed)
        float newSlowFactor = 1f - factor;
        if (newSlowFactor < slowFactor)
        {
            slowFactor = newSlowFactor;
            if (sr != null) sr.color = Color.cyan; // Visual feedback for slow
        }
        slowTimer = Mathf.Max(slowTimer, duration);
    }

    void Update()
    {
        if (waypoints == null || currentWP >= waypoints.Length) return;

        Transform target = waypoints[currentWP];
        
        // Calculate raw movement vector without offset
        Vector3 dir = (target.position - transform.position).normalized;
        dir.z = 0; // keep it 2D
        
        // Flip sprite based on horizontal direction
        if (sr != null && Mathf.Abs(dir.x) > 0.01f)
        {
            // The default otter sprite faces left. So if we are moving right (positive x), flipX should be true
            sr.flipX = dir.x > 0; 
        }

        // Move towards target
        if (slowTimer > 0)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0)
            {
                slowFactor = 1f;
                if (sr != null) sr.color = originalColor;
            }
        }

        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * slowFactor * Time.deltaTime);

        // Add a visual-only vertical offset so the sprite sits "on" the path instead of "in" it
        // We do this by locally shifting the SpriteRenderer if it exists
        if (sr != null && sr.transform == transform)
        {
            // We can't shift the root easily because it messes up waypoints, 
            // but we can't shift a component. Since sr is on root, we must shift the root 
            // but compensate waypoint distance checks, or better: just shift the visual child?
            // Since the code structure is set up, a simpler way is to let the root follow the path exactly, 
            // but offset the SpriteRenderer component? You can't offset a SpriteRenderer component without a child object.
            // But wait, there is no separate child for the sprite in EnemySpawner. 
            // Let's modify the target position temporarily in Update. No, that breaks movement.
        }

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            currentWP++;
            if (currentWP >= waypoints.Length)
            {
                // reached the end - damage the player
                PlayerHealth ph = Object.FindFirstObjectByType<PlayerHealth>();
                if (ph != null)
                    ph.TakeDamage(10);

                Destroy(gameObject);
            }
        }
    }

    void SpawnDeathEffect()
    {
        GameObject fx = new GameObject("DeathFX");
        fx.transform.position = transform.position;
        var ps = fx.AddComponent<ParticleSystem>();
        
        // Stop the particle system before changing duration
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 3f;
        main.startSize = 0.15f;
        main.startColor = sr != null ? sr.color : Color.red;
        main.maxParticles = 20;
        main.duration = 0.3f;
        main.loop = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 15) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.2f;

        var renderer = fx.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));

        ps.Play();
        Destroy(fx, 1f);
    }
}
