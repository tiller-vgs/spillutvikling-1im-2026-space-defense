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
        transform.localScale = Vector3.one * data.size;

        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = data.color.ToColor();

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

    void Update()
    {
        if (waypoints == null || currentWP >= waypoints.Length) return;

        Transform target = waypoints[currentWP];
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

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
