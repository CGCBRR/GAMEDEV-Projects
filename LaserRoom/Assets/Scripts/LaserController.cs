using UnityEngine;

/// <summary>
/// Unified laser sweeper for ALL phases (attach to each "Phase N Lasers" parent).
/// Drag up to 3 movers (single beams like Phase 1, or groups like Phase 2/3)
/// in sweep order: Element 0 moves first, then 1, then 2.
/// Phases gated by checkpoints use startActive = false + Activate()
/// (Checkpoint calls it via SendMessage). Phase 1 uses startActive = true.
/// </summary>
public class LaserController : MonoBehaviour
{
    [Header("Movers (3 only — array order = sweep order)")]
    [Tooltip("Drag the 3 lasers/groups here in order. Element 0 moves first, then 1, then 2. Leave empty to auto-find direct children. Only 3 are supported.")]
    public Transform[] movers = new Transform[3];

    const int MaxMovers = 3;

    [Header("Sweep toward the player")]
    [Tooltip("World-space direction the movers travel. +Z runs down the corridor toward the player entrance.")]
    public Vector3 moveDirection = Vector3.forward;
    public float speed = 4f;
    [Tooltip("How far each mover travels from its start position.")]
    public float travelDistance = 20f;
    [Tooltip("Seconds between mover starts.")]
    public float delayBetween = 2.5f;
    public float startDelay = 1f;

    [Header("Loop")]
    public bool loop = true;
    public float loopDelay = 4f;

    [Header("Activation")]
    [Tooltip("If true, sweeps from the start (Phase 1). If false, sits still until Activate() is called, e.g. by a Checkpoint (Phase 2/3).")]
    public bool startActive = true;

    [Header("Beams")]
    [Tooltip("Red laser material. If empty, a runtime emissive-red material is created and shared.")]
    public Material laserMaterial;
    [Tooltip("Trigger boxes match the beam mesh plus this margin (1.25 = 25% bigger).")]
    public float triggerMargin = 1.25f;
    [Tooltip("Damage applied while a laser touches the player (per second).")]
    public float damagePerSecond = 30f;
    [Tooltip("Instant damage on first touch.")]
    public float hitDamage = 10f;

    Vector3[] _startPos;
    float _waveStart;
    float _waveEndTime;
    bool _activated;

    void Awake()
    {
        NormalizeMovers();
        if (movers == null || movers.Length == 0)
            AutoFindMovers();
        NormalizeMovers();

        _startPos = new Vector3[movers.Length];
        for (int i = 0; i < movers.Length; i++)
        {
            if (movers[i] == null) continue;
            _startPos[i] = movers[i].position;
        }

        if (laserMaterial == null)
            laserMaterial = LaserSetup.GetRuntimeRed();
        LaserSetup.SetupBeams(transform, laserMaterial, triggerMargin, damagePerSecond, hitDamage);
    }

    void Start()
    {
        _activated = startActive;
        _waveStart = Time.time + startDelay;
        _waveEndTime = float.MaxValue;
    }

    /// <summary>Arms the sweep (called by Checkpoints via SendMessage). Movers stay still until then.</summary>
    public void Activate()
    {
        if (_activated) return;
        _activated = true;
        _waveStart = Time.time + startDelay;
        _waveEndTime = float.MaxValue;
    }

    void Update()
    {
        if (!_activated) return;
        if (movers == null || movers.Length == 0) return;

        Vector3 dir = moveDirection.normalized;
        bool allDone = true;

        for (int i = 0; i < movers.Length; i++)
        {
            if (movers[i] == null) continue;
            float t = Time.time - (_waveStart + i * delayBetween);
            float d = Mathf.Clamp(t * speed, 0f, travelDistance);
            movers[i].position = _startPos[i] + dir * d;
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

    void AutoFindMovers()
    {
        // Generic: direct children containing beam meshes, in hierarchy order.
        // Works for single beams (Phase 1) and groups (Phase 2/3).
        System.Collections.Generic.List<Transform> ordered = new System.Collections.Generic.List<Transform>();
        foreach (Transform child in transform)
        {
            if (ordered.Count >= MaxMovers) break;
            if (child.GetComponentsInChildren<MeshFilter>().Length > 0)
                ordered.Add(child);
        }
        movers = ordered.ToArray();
    }

    /// <summary>Enforces 3-only, drag-drop order. Nulls/duplicates removed, extras trimmed. Array index = sweep order.</summary>
    void NormalizeMovers()
    {
        if (movers == null) return;
        if (movers.Length > MaxMovers)
            Debug.LogWarning($"LaserController supports only {MaxMovers} movers — extra entries ignored. Array order is sweep order (0 first).", this);
        System.Collections.Generic.List<Transform> valid = new System.Collections.Generic.List<Transform>();
        foreach (Transform t in movers)
        {
            if (t == null || valid.Contains(t)) continue;
            valid.Add(t);
            if (valid.Count >= MaxMovers) break;
        }
        movers = valid.ToArray();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (movers != null && movers.Length > MaxMovers)
            System.Array.Resize(ref movers, MaxMovers);
    }

    void OnDrawGizmosSelected()
    {
        if (movers == null) return;
        Gizmos.color = Color.red;
        Vector3 dir = moveDirection.normalized;
        for (int i = 0; i < movers.Length; i++)
        {
            if (movers[i] == null) continue;
            Vector3 from = Application.isPlaying && _startPos != null && i < _startPos.Length
                ? _startPos[i] : movers[i].position;
            Gizmos.DrawLine(from, from + dir * travelDistance);
        }
    }
#endif
}
