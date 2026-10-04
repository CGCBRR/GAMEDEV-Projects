using UnityEngine;

/// <summary>
/// Phase 3 laser wall (attach to the "Phase 3 Lasers" parent).
/// Each laser GROUP moves as one unit toward the player, staggered
/// like Phase 1/2: Element 0 first, then 1, then 2 (array order).
/// Keep startActive false so Checkpoint 2 activates it via Activate().
/// </summary>
public class Phase3Controller : MonoBehaviour
{
    [Header("Groups (3 only — array order = sweep order)")]
    [Tooltip("Drag Laser Group 7, Laser Group 8, Laser Group 9 here in order. Element 0 moves first, then 1, then 2. Leave empty to auto-find by name. Only 3 are supported.")]
    public Transform[] groups = new Transform[3];

    const int MaxGroups = 3;

    [Header("Sweep toward the player")]
    public Vector3 moveDirection = Vector3.forward;
    public float speed = 4f;
    [Tooltip("How far each group travels from its start position.")]
    public float travelDistance = 20f;
    [Tooltip("Seconds between group starts.")]
    public float delayBetween = 2.5f;
    public float startDelay = 1f;

    [Header("Loop")]
    public bool loop = true;
    public float loopDelay = 4f;

    [Header("Activation")]
    [Tooltip("If false, the groups sit still (but visible) until Activate() is called, e.g. by Checkpoint 2.")]
    public bool startActive = false;

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

    static readonly string[] GroupOrder = { "Laser Group 7", "Laser Group 8", "Laser Group 9" };

    void Awake()
    {
        NormalizeGroups();
        if (groups == null || groups.Length == 0)
            AutoFindGroups();
        NormalizeGroups();

        _startPos = new Vector3[groups.Length];
        for (int i = 0; i < groups.Length; i++)
        {
            if (groups[i] == null) continue;
            _startPos[i] = groups[i].position;
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

    /// <summary>Arms the sweep (called by Checkpoint 2). Groups stay still until then.</summary>
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
        if (groups == null || groups.Length == 0) return;

        Vector3 dir = moveDirection.normalized;
        bool allDone = true;

        for (int i = 0; i < groups.Length; i++)
        {
            if (groups[i] == null) continue;
            float t = Time.time - (_waveStart + i * delayBetween);
            float d = Mathf.Clamp(t * speed, 0f, travelDistance);
            groups[i].position = _startPos[i] + dir * d;
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

    void AutoFindGroups()
    {
        // explicit 7 -> 8 -> 9 order first
        System.Collections.Generic.List<Transform> ordered = new System.Collections.Generic.List<Transform>();
        foreach (string n in GroupOrder)
        {
            Transform t = transform.Find(n);
            if (t != null && !ordered.Contains(t)) ordered.Add(t);
            if (ordered.Count >= MaxGroups) break;
        }
        if (ordered.Count < MaxGroups)
        {
            // fill remainder: direct children that contain beams, in hierarchy order
            foreach (Transform child in transform)
            {
                if (ordered.Count >= MaxGroups) break;
                if (ordered.Contains(child)) continue;
                if (child.GetComponentsInChildren<MeshFilter>().Length > 0)
                    ordered.Add(child);
            }
        }
        groups = ordered.ToArray();
    }

    /// <summary>Enforces 3-only, drag-drop order. Nulls/duplicates removed, extras trimmed. Array index = sweep order.</summary>
    void NormalizeGroups()
    {
        if (groups == null) return;
        if (groups.Length > MaxGroups)
            Debug.LogWarning($"Phase 3 supports only {MaxGroups} groups — extra entries ignored. Array order is sweep order (0 first).", this);
        System.Collections.Generic.List<Transform> valid = new System.Collections.Generic.List<Transform>();
        foreach (Transform t in groups)
        {
            if (t == null || valid.Contains(t)) continue;
            valid.Add(t);
            if (valid.Count >= MaxGroups) break;
        }
        groups = valid.ToArray();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (groups != null && groups.Length > MaxGroups)
            System.Array.Resize(ref groups, MaxGroups);
    }

    void OnDrawGizmosSelected()
    {
        if (groups == null) return;
        Gizmos.color = Color.red;
        Vector3 dir = moveDirection.normalized;
        for (int i = 0; i < groups.Length; i++)
        {
            if (groups[i] == null) continue;
            Vector3 from = Application.isPlaying && _startPos != null && i < _startPos.Length
                ? _startPos[i] : groups[i].position;
            Gizmos.DrawLine(from, from + dir * travelDistance);
        }
    }
#endif
}
