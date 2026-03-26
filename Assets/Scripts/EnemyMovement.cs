using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float speed = 3f;
    public float maxHealth = 50f;
    public int reward = 10;
    public string serverEnemyId = "";

    float currentHealth;
    Transform[] waypoints;
    int currentWP = 0;
    HealthBar healthBar;
    SpriteRenderer sr;
    Animator animator;
    float slowFactor = 1f;
    float slowTimer = 0f;
    Color originalColor = Color.white;

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
        transform.localScale = Vector3.one * data.size * 6f;

        sr = GetComponentInChildren<SpriteRenderer>();

        if (sr != null && sr.sprite != null && sr.sprite.name == "Square")
            sr.color = data.color.ToColor();

        if (sr != null) originalColor = sr.color;

        animator = GetComponentInChildren<Animator>();
        if (animator != null)
            animator.speed = speed / 3f;

        healthBar = GetComponentInChildren<HealthBar>();
        if (healthBar != null)
            healthBar.SetHealth(1f);
    }

    public void SetupFromServer(string serverId, float serverMaxHealth, float serverSpeed, int serverReward)
    {
        serverEnemyId = serverId;
        maxHealth = serverMaxHealth;
        currentHealth = serverMaxHealth;
        speed = serverSpeed;
        reward = serverReward;
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetProgress()
    {
        if (waypoints == null || waypoints.Length == 0) return 0f;
        float progress = currentWP;
        if (currentWP < waypoints.Length)
        {
            float segDist = Vector3.Distance(waypoints[Mathf.Max(0, currentWP - 1)].position, waypoints[currentWP].position);
            if (segDist > 0.01f)
            {
                float traveled = segDist - Vector3.Distance(transform.position, waypoints[currentWP].position);
                progress += Mathf.Clamp01(traveled / segDist);
            }
        }
        return progress;
    }

    public void TakeDamage(float dmg)
    {
        currentHealth -= dmg;
        if (healthBar != null)
            healthBar.SetHealth(currentHealth / maxHealth);

        if (currentHealth <= 0)
        {
            if (ServerManager.instance != null && ServerManager.instance.connected && serverEnemyId != "")
            {
                ServerManager.instance.ReportEnemyKilled(serverEnemyId, (resp) =>
                {
                    if (resp != null && resp.ok)
                    {
                    }
                });
            }
            else
            {
                if (CurrencyManager.instance != null)
                    CurrencyManager.instance.AddMoney(reward);
            }

            SpawnDeathEffect();
            Destroy(gameObject);
        }
    }

    public void ApplySlow(float factor, float duration)
    {
        float newSlowFactor = 1f - factor;
        if (newSlowFactor < slowFactor)
        {
            slowFactor = newSlowFactor;
            if (sr != null) sr.color = Color.cyan;
        }
        slowTimer = Mathf.Max(slowTimer, duration);
    }

    void Update()
    {
        if (waypoints == null || currentWP >= waypoints.Length) return;

        Transform target = waypoints[currentWP];
        Vector3 dir = (target.position - transform.position).normalized;
        dir.z = 0;

        if (sr != null && Mathf.Abs(dir.x) > 0.01f)
            sr.flipX = dir.x > 0;

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

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            currentWP++;
            
            if (currentWP >= waypoints.Length)
            {
                if (ServerManager.instance != null && ServerManager.instance.connected && serverEnemyId != "")
                {
                    ServerManager.instance.ReportEnemyLeaked(serverEnemyId, (resp) =>
                    {
                    });
                }
                else
                {
                    PlayerHealth ph = Object.FindFirstObjectByType<PlayerHealth>();
                    if (ph != null)
                    {
                        int dmg;
                        if (maxHealth >= 400) dmg = 30;
                        else if (maxHealth >= 130) dmg = 20;
                        else if (maxHealth >= 100) dmg = 15;
                        else if (maxHealth >= 60) dmg = 10;
                        else if (maxHealth >= 45) dmg = 7;
                        else dmg = 5;
                        ph.TakeDamage(dmg);
                    }
                }

                Destroy(gameObject);
            }
        }
    }

    void SpawnDeathEffect()
    {
        var fx = new GameObject("DeathFX");
        fx.transform.position = transform.position;
        var ps = fx.AddComponent<ParticleSystem>();

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
