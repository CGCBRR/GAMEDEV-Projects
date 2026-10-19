using UnityEngine;

/// <summary>
/// Shared physics-query helpers for the stealth hazards.
///
/// Hazards detect the player with volume queries (Physics.CheckSphere / CheckBox /
/// CheckCapsule) and confirm what they hit with a raycast, so hazards need no
/// trigger colliders at all. Every query here ignores triggers, and the volume
/// queries only test the player's layer.
/// </summary>
public static class StealthPhysics
{
    /// <summary>Layer the player is expected to use when it cannot be resolved at runtime.</summary>
    public const int FallbackPlayerLayer = 8;

    /// <summary>Height above the player's origin used as the "chest" aim point for line-of-sight tests.</summary>
    public const float PlayerAimHeight = 1f;

    static PlayerHealth _player;
    static int _playerMask;
    static int _staggerSlot;

    /// <summary>Cached player health (the scene's Player-tagged rig), resolved on first use.</summary>
    public static PlayerHealth Player
    {
        get
        {
            if (_player == null)
            {
                _player = Object.FindFirstObjectByType<PlayerHealth>();
                if (_player == null)
                {
                    GameObject go = GameObject.FindGameObjectWithTag("Player");
                    if (go != null) _player = go.GetComponentInChildren<PlayerHealth>();
                }
            }
            return _player;
        }
    }

    public static bool HasPlayer => Player != null;

    /// <summary>Layer mask containing only the player's layer.</summary>
    public static int PlayerMask
    {
        get
        {
            if (_playerMask == 0)
            {
                PlayerHealth p = Player;
                _playerMask = 1 << (p != null ? p.gameObject.layer : FallbackPlayerLayer);
            }
            return _playerMask;
        }
    }

    /// <summary>World-space "chest" point of the player used for line-of-sight rays.</summary>
    public static Vector3 PlayerAimPoint
    {
        get
        {
            PlayerHealth p = Player;
            return p != null ? p.transform.position + Vector3.up * PlayerAimHeight : Vector3.zero;
        }
    }

    /// <summary>Sphere volume test against the player (Physics.CheckSphere).</summary>
    public static bool SphereHitsPlayer(Vector3 center, float radius)
        => Physics.CheckSphere(center, Mathf.Max(0.01f, radius), PlayerMask, QueryTriggerInteraction.Ignore);

    /// <summary>Oriented box volume test against the player (Physics.CheckBox).</summary>
    public static bool BoxHitsPlayer(Vector3 center, Vector3 halfExtents, Quaternion orientation)
    {
        halfExtents = new Vector3(
            Mathf.Max(0.01f, halfExtents.x),
            Mathf.Max(0.01f, halfExtents.y),
            Mathf.Max(0.01f, halfExtents.z));
        return Physics.CheckBox(center, halfExtents, orientation, PlayerMask, QueryTriggerInteraction.Ignore);
    }

    /// <summary>Capsule volume test against the player (Physics.CheckCapsule).</summary>
    public static bool CapsuleHitsPlayer(Vector3 start, Vector3 end, float radius)
        => Physics.CheckCapsule(start, end, Mathf.Max(0.01f, radius), PlayerMask, QueryTriggerInteraction.Ignore);

    /// <summary>Raycast line of sight from a hazard towards the player's chest.</summary>
    public static bool HasLineOfSightToPlayer(Vector3 from)
        => HasLineOfSight(from, PlayerAimPoint);

    /// <summary>
    /// Casts a ray from <paramref name="from"/> to <paramref name="target"/> and reports whether
    /// the view is clear. True when the first solid hit is the player, or when nothing is hit at
    /// all (the ray started inside the player). False when geometry blocks the view.
    /// </summary>
    public static bool HasLineOfSight(Vector3 from, Vector3 target)
    {
        Vector3 delta = target - from;
        float distance = delta.magnitude;
        if (distance < 0.001f) return true;

        RaycastHit hit;
        if (Physics.Raycast(from, delta / distance, out hit, distance, ~0, QueryTriggerInteraction.Ignore))
            return IsPlayer(hit.collider);

        // Nothing hit: the origin was already inside the player's capsule, which
        // means the volume test that led here is looking straight at the player.
        return true;
    }

    /// <summary>True when the collider belongs to the Player-tagged rig.</summary>
    public static bool IsPlayer(Collider collider)
    {
        if (collider == null) return false;
        PlayerHealth hp = collider.GetComponent<PlayerHealth>();
        if (hp == null) hp = collider.GetComponentInParent<PlayerHealth>();
        if (hp == null) return false;
        return collider.CompareTag("Player") || hp.CompareTag("Player");
    }

    /// <summary>Point on segment a-b closest to <paramref name="point"/> (used to aim line-of-sight rays).</summary>
    public static Vector3 ClosestPointOnSegment(Vector3 a, Vector3 b, Vector3 point)
    {
        Vector3 ab = b - a;
        float lengthSq = ab.sqrMagnitude;
        if (lengthSq < 0.000001f) return a;
        float t = Mathf.Clamp01(Vector3.Dot(point - a, ab) / lengthSq);
        return a + ab * t;
    }

    /// <summary>
    /// Round-robin slot so repeated hazard checks can be spread over several frames
    /// instead of all running on the same one.
    /// </summary>
    public static int NextStaggerSlot() => _staggerSlot++;
}
