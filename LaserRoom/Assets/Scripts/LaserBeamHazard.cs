using UnityEngine;

/// <summary>
/// Query-based laser beam hazard.
///
/// The beam has no colliders or triggers: every <see cref="checkInterval"/> seconds
/// the beam volume is rebuilt as a capsule along the beam's long axis and tested
/// with Physics.CheckCapsule against the player's layer. A raycast then confirms
/// line of sight before any damage is applied, so beams cannot hit through walls.
/// Checks are staggered across beams so they never all run on the same frame.
/// </summary>
public class LaserBeamHazard : MonoBehaviour
{
    [Header("Damage")]
    [Tooltip("Instant damage the moment the beam starts touching the player.")]
    public float hitDamage = 30f;
    [Tooltip("Damage per second while the player stays inside the beam.")]
    public float damagePerSecond = 30f;

    [Header("Queries")]
    [Tooltip("Seconds between volume checks. Checks are staggered across beams.")]
    public float checkInterval = 0.05f;
    [Tooltip("The beam cross-section is inflated by this factor to build the capsule radius.")]
    public float beamMargin = 1.25f;

    // Beam volume in this transform's local space (mesh bounds along its longest axis).
    Vector3 _localA;
    Vector3 _localB;
    float _worldRadius = 0.15f;

    bool _touching;
    bool _shieldedTouch;
    float _nextCheck;
    float _lastCheckTime;

    void Awake()
    {
        CacheBeamShape();
    }

    void OnEnable()
    {
        _touching = false;
        _shieldedTouch = false;
        _lastCheckTime = Time.time;
        // Stagger: beams share the 0.05 s window instead of checking together.
        _nextCheck = Time.time + checkInterval * 0.25f * (StealthPhysics.NextStaggerSlot() % 4);
    }

    void Update()
    {
        float now = Time.time;
        if (now < _nextCheck) return;
        float dt = Mathf.Max(0f, now - _lastCheckTime);
        _lastCheckTime = now;
        _nextCheck = now + checkInterval;

        PlayerHealth hp = StealthPhysics.Player;
        if (hp == null)
            return;

        if (!PlayerInBeam())
        {
            _touching = false;
            _shieldedTouch = false;
            return;
        }

        if (!_touching)
        {
            // First contact: the shield (if any) absorbs the whole touch, like the
            // old trigger version, otherwise the touch opens with instant damage.
            _touching = true;
            if (hp.ConsumeShield())
            {
                _shieldedTouch = true;
                return;
            }
            hp.TakeDamage(hitDamage);
            return;
        }

        if (!_shieldedTouch)
            hp.TakeDamage(damagePerSecond * dt);
    }

    /// <summary>Capsule volume test along the beam plus a line-of-sight raycast.</summary>
    bool PlayerInBeam()
    {
        Vector3 a = transform.TransformPoint(_localA);
        Vector3 b = transform.TransformPoint(_localB);
        if (!StealthPhysics.CapsuleHitsPlayer(a, b, _worldRadius))
            return false;

        Vector3 eye = StealthPhysics.ClosestPointOnSegment(a, b, StealthPhysics.PlayerAimPoint);
        return StealthPhysics.HasLineOfSightToPlayer(eye);
    }

    /// <summary>
    /// Builds the capsule from the beam mesh: the longest axis of the mesh bounds is
    /// the segment, the largest of the two other half-extents (scaled to world) is
    /// the radius.
    /// </summary>
    void CacheBeamShape()
    {
        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter == null) filter = GetComponentInChildren<MeshFilter>();
        if (filter == null || filter.sharedMesh == null)
        {
            _localA = Vector3.back * 1.5f;
            _localB = Vector3.forward * 1.5f;
            _worldRadius = 0.15f;
            return;
        }

        Bounds bounds = filter.sharedMesh.bounds;
        Vector3 half = bounds.size * 0.5f;
        Vector3 scale = filter.transform.lossyScale;

        Vector3 axis;
        float halfLength;
        float crossA, crossB, scaleA, scaleB;

        if (half.y >= half.x && half.y >= half.z)
        {
            axis = Vector3.up; halfLength = half.y;
            crossA = half.x; crossB = half.z;
            scaleA = Mathf.Abs(scale.x); scaleB = Mathf.Abs(scale.z);
        }
        else if (half.x >= half.z)
        {
            axis = Vector3.right; halfLength = half.x;
            crossA = half.y; crossB = half.z;
            scaleA = Mathf.Abs(scale.y); scaleB = Mathf.Abs(scale.z);
        }
        else
        {
            axis = Vector3.forward; halfLength = half.z;
            crossA = half.x; crossB = half.y;
            scaleA = Mathf.Abs(scale.x); scaleB = Mathf.Abs(scale.y);
        }

        Vector3 localA = bounds.center - axis * halfLength;
        Vector3 localB = bounds.center + axis * halfLength;

        // Mesh bounds live in the filter's local space; bring the ends into ours.
        if (filter.transform != transform)
        {
            _localA = transform.InverseTransformPoint(filter.transform.TransformPoint(localA));
            _localB = transform.InverseTransformPoint(filter.transform.TransformPoint(localB));
        }
        else
        {
            _localA = localA;
            _localB = localB;
        }

        _worldRadius = Mathf.Max(0.03f, Mathf.Max(crossA * scaleA, crossB * scaleB) * beamMargin);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        checkInterval = Mathf.Max(0.01f, checkInterval);
        beamMargin = Mathf.Max(0.01f, beamMargin);
    }

    void OnDrawGizmosSelected()
    {
        CacheBeamShape();
        Vector3 a = transform.TransformPoint(_localA);
        Vector3 b = transform.TransformPoint(_localB);
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.9f);
        Gizmos.DrawLine(a, b);
        Gizmos.DrawWireSphere(a, _worldRadius);
        Gizmos.DrawWireSphere(b, _worldRadius);
    }
#endif
}
