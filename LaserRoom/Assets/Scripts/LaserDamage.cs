using UnityEngine;

/// <summary>
/// Put on each laser. Hurts the player (tag "Player") only on real
/// physical touch with the beam trigger: instant hit on enter plus
/// damage-over-time while the player stays inside the beam.
/// Works with the player's CharacterController (no Rigidbody needed).
/// </summary>
[RequireComponent(typeof(Collider))]
public class LaserDamage : MonoBehaviour
{
    [Tooltip("Instant damage on first touch.")]
    public float hitDamage = 10f;
    [Tooltip("Damage per second while the player stays inside the beam.")]
    public float damagePerSecond = 30f;

    // While true, this touch was already paid for with the shield:
    // the whole touch (enter + stay) deals zero damage, then the
    // next touch hurts normally.
    bool _shieldedTouch;

    void Reset()
    {
        Collider c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    void OnDisable()
    {
        _shieldedTouch = false;
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerHealth hp = ResolvePlayer(other);
        if (hp == null) return;
        _shieldedTouch = false;
        if (hp.ConsumeShield())
        {
            _shieldedTouch = true;
            return;
        }
        hp.TakeDamage(hitDamage);
    }

    void OnTriggerStay(Collider other)
    {
        if (_shieldedTouch) return;
        PlayerHealth hp = ResolvePlayer(other);
        if (hp != null) hp.TakeDamage(damagePerSecond * Time.deltaTime);
    }

    void OnTriggerExit(Collider other)
    {
        if (ResolvePlayer(other) == null) return;
        _shieldedTouch = false;
    }

    static PlayerHealth ResolvePlayer(Collider other)
    {
        if (other == null) return null;
        PlayerHealth hp = other.GetComponent<PlayerHealth>();
        if (hp == null) hp = other.GetComponentInParent<PlayerHealth>();
        if (hp == null) return null;
        if (!other.CompareTag("Player") && !hp.CompareTag("Player")) return null;
        return hp;
    }
}
