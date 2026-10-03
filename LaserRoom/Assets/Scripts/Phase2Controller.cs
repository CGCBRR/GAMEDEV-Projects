using UnityEngine;

/// <summary>
/// Phase 2 laser wall (attach to the "Phase 2 Lasers" parent).
/// Each laser GROUP (4 beams) moves as one unit toward the player:
/// Group 4 first, then Group 5, then Group 6, staggered like Phase 1.
/// Keep this object INACTIVE until Checkpoint 1 activates it.
/// </summary>
public class Phase2Controller : MonoBehaviour
{
    [Header("Groups (sweep order: 4 first)")]
    [Tooltip("Leave empty to auto-find 'Laser Group 4', 'Laser Group 5', 'Laser Group 6' in that order.")]
    public Transform[] groups;

    [Header("Sweep toward the player")]
    public Vector3 moveDirection = Vector3.forward;
    public float speed = 2f;
    [Tooltip("How far each group travels from its start position.")]
    public float travelDistance = 20f;
    [Tooltip("Seconds between group starts.")]
    public float delayBetween = 2.5f;
    public float startDelay = 1f;

    [Header("Loop")]
    public bool loop = true;
    public float loopDelay = 4f;

    [Header("Activation")]
    [Tooltip("If false, the groups sit still (but visible) until Activate() is called, e.g. by Checkpoint 1.")]
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

    static readonly string[] GroupOrder = { "Laser Group 4", "Laser Group 2", "Laser Group 3", "Laser Group 5", "Laser Group 6" };

    void Awake()
    {
        if (groups == null || groups.Length == 0)
            AutoFindGroups();

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

    /// <summary>Arms the sweep (called by Checkpoint 1). Groups stay still until then.</summary>
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
        // explicit 4 -> 2 -> 3 order first (tolerates the other numbering as fallback)
        System.Collections.Generic.List<Transform> ordered = new System.Collections.Generic.List<Transform>();
        foreach (string n in GroupOrder)
        {
            Transform t = transform.Find(n);
            if (t != null && !ordered.Contains(t)) ordered.Add(t);
            if (ordered.Count >= 3) break;
        }
        if (ordered.Count < 3)
        {
            // fallback: direct children that contain beams, in hierarchy order
            ordered.Clear();
            foreach (Transform child in transform)
            {
                if (child.GetComponentsInChildren<MeshFilter>().Length > 0)
                    ordered.Add(child);
                if (ordered.Count >= 3) break;
            }
        }
        groups = ordered.ToArray();
    }

#if UNITY_EDITOR
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
