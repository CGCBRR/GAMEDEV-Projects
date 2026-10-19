using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    [Tooltip("Invulnerability window after (re)spawn, in seconds.")]
    public float spawnInvulnTime = 2f;
    [Tooltip("Shield buff: blocks exactly one laser hit, then breaks.")]
    public bool hasShield;

    [Header("Run stats")]
    [Tooltip("Times damage actually reached the player (lasers, drones, scanners).")]
    public int hitCount;
    [Tooltip("Times a hazard alarm was raised against the player.")]
    public int alarmCount;
    [Tooltip("Times the player died and respawned.")]
    public int deathCount;

    public bool IsDead { get; private set; }

    Vector3 _spawnPos;
    Quaternion _spawnRot;
    float _invulnTimer;
    float _lastHitStatTime;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Start()
    {
        _spawnPos = transform.position;
        _spawnRot = transform.rotation;
        _invulnTimer = spawnInvulnTime;
    }

    void Update()
    {
        if (_invulnTimer > 0f)
            _invulnTimer -= Time.deltaTime;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || amount <= 0f) return;
        if (_invulnTimer > 0f) return;

        currentHealth -= amount;
        if (Time.time - _lastHitStatTime > 0.25f)   // continuous damage (laser wash) counts once
        {
            _lastHitStatTime = Time.time;
            hitCount++;
        }
        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    public void GrantShield()
    {
        if (IsDead) return;
        hasShield = true;
    }

    public bool ConsumeShield()
    {
        if (!hasShield) return false;
        hasShield = false;
        return true;
    }

    /// <summary>Called by detection hazards when their alarm trips (run stats).</summary>
    public void ReportAlarm()
    {
        alarmCount++;
    }

    public void SetSpawn(Vector3 pos)
    {
        _spawnPos = pos;
    }

    void Die()
    {
        deathCount++;
        IsDead = true;
        Debug.Log(gameObject.name + " killed by lasers - respawning.");

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        transform.position = _spawnPos;
        transform.rotation = _spawnRot;
        if (cc != null) cc.enabled = true;

        currentHealth = maxHealth;
        hasShield = false;
        _invulnTimer = spawnInvulnTime;
        IsDead = false;
    }
}
