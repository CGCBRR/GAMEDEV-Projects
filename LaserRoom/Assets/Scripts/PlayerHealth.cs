using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    [Tooltip("Invulnerability window after (re)spawn, in seconds.")]
    public float spawnInvulnTime = 2f;
    [Tooltip("Shield buff: blocks exactly one laser hit, then breaks.")]
    public bool hasShield;

    public bool IsDead { get; private set; }

    Vector3 _spawnPos;
    Quaternion _spawnRot;
    float _invulnTimer;

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

    public void SetSpawn(Vector3 pos)
    {
        _spawnPos = pos;
    }

    void Die()
    {
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
