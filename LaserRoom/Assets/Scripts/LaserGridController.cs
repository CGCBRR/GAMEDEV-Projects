using UnityEngine;

/// <summary>
/// Resident-Evil style laser grid (attach to the "Phase 1" parent).
/// Laser 1 sweeps first, then 2, then 3, each moving toward the player
/// down the corridor. Found automatically by child name (Laser 1..3).
/// </summary>
public class LaserGridController : MonoBehaviour
{
    [Header("Lasers (3 only - array order = sweep order)")]
    [Tooltip("Drag Laser 1, Laser 2, Laser 3 here in order. Element 0 moves first, then 1, then 2. Leave empty to auto-find by name. Only 3 are supported.")]
    public Transform[] lasers = new Transform[3];

    const int MaxLasers = 3;

    [Header("Sweep toward the player")]
    [Tooltip("World-space direction the lasers travel. +Z runs down the corridor toward the player entrance.")]
    public Vector3 moveDirection = Vector3.forward;
    public float speed = 1.5f;
    [Tooltip("How far each laser travels from its start position.")]
    public float travelDistance = 8f;
    [Tooltip("Seconds between Laser 1 / 2 / 3 starts.")]
    public float delayBetween = 2f;
    public float startDelay = 1f;

    [Header("Loop")]
    public bool loop = true;
    public float loopDelay = 3f;

    [Header("Look")]
    [Tooltip("Red laser material. If empty, a runtime emissive-red material is created and shared.")]
    public Material laserMaterial;
    [Header("Touch damage (trigger-based)")]
    [Tooltip("Damage applied while a laser touches the player (per second).")]
    public float damagePerSecond = 30f;
    [Tooltip("Instant damage on first touch.")]
    public float hitDamage = 10f;
    [Tooltip("Trigger boxes match the beam mesh exactly, plus this margin (1.25 = 25% bigger). Proportional, so thin beams stay thin.")]
    public float triggerMargin = 1.25f;

    Vector3[] _startPos;
    float _waveStart;
    float _waveEndTime;

    void Awake()
    {
        NormalizeLasers();
        if (lasers == null || lasers.Length == 0)
            AutoFindLasers();
        NormalizeLasers();

        _startPos = new Vector3[lasers.Length];
        for (int i = 0; i < lasers.Length; i++)
        {
            if (lasers[i] == null) continue;
            _startPos[i] = lasers[i].position;
        }

        if (laserMaterial == null)
            laserMaterial = LaserSetup.GetRuntimeRed();
        LaserSetup.SetupBeams(transform, laserMaterial, triggerMargin, damagePerSecond, hitDamage);
    }

    void Start()
    {
        _waveStart = Time.time + startDelay;
        _waveEndTime = float.MaxValue;
    }

    void Update()
    {
        if (lasers == null || lasers.Length == 0) return;

        Vector3 dir = moveDirection.normalized;
        bool allDone = true;

        for (int i = 0; i < lasers.Length; i++)
        {
            if (lasers[i] == null) continue;
            float t = Time.time - (_waveStart + i * delayBetween);
            float d = Mathf.Clamp(t * speed, 0f, travelDistance);
            lasers[i].position = _startPos[i] + dir * d;
            if (d < travelDistance) allDone = false;
        }

        if (allDone)
        {
            if (_waveEndTime == float.MaxValue)
                _waveEndTime = Time.time;
            if (loop && Time.time - _waveEndTime >= loopDelay)
            {
                _waveStart = Time.time;
                _waveEndTime = float.MaxValue;
            }
        }
    }

    void AutoFindLasers()
    {
        System.Collections.Generic.List<Transform> ordered = new System.Collections.Generic.List<Transform>();
        for (int i = 1; i <= MaxLasers; i++)
        {
            Transform t = transform.Find("Laser " + i);
            if (t == null) t = transform.Find("Laser" + i);
            if (t == null) t = transform.Find("Laser (" + i + ")");
            if (t != null && !ordered.Contains(t)) ordered.Add(t);
        }
        if (ordered.Count == 0)
        {
            // fallback: first 3 children in hierarchy order
            foreach (Transform child in transform)
            {
                if (ordered.Count >= MaxLasers) break;
                if (!ordered.Contains(child)) ordered.Add(child);
            }
        }
        lasers = ordered.ToArray();
    }

    /// <summary>Enforces 3-only, drag-drop order. Nulls/duplicates removed, extras trimmed. Array index = sweep order.</summary>
    void NormalizeLasers()
    {
        if (lasers == null) return;
        if (lasers.Length > MaxLasers)
            Debug.LogWarning($"Phase 1 supports only {MaxLasers} lasers — extra entries ignored. Array order is sweep order (0 first).", this);
        System.Collections.Generic.List<Transform> valid = new System.Collections.Generic.List<Transform>();
        foreach (Transform t in lasers)
        {
            if (t == null || valid.Contains(t)) continue;
            valid.Add(t);
            if (valid.Count >= MaxLasers) break;
        }
        lasers = valid.ToArray();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (lasers != null && lasers.Length > MaxLasers)
            System.Array.Resize(ref lasers, MaxLasers);
    }

    void OnDrawGizmosSelected()
    {
        if (lasers == null) return;
        Gizmos.color = Color.red;
        Vector3 dir = moveDirection.normalized;
        foreach (Transform laser in lasers)
        {
            if (laser == null) continue;
            int idx = System.Array.IndexOf(lasers, laser);
            Vector3 from = Application.isPlaying && _startPos != null && idx >= 0 && idx < _startPos.Length
                ? _startPos[idx] : laser.position;
            Gizmos.DrawLine(from, from + dir * travelDistance);
        }
    }
#endif
}
