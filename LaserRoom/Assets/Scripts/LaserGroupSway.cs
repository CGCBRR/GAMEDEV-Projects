using UnityEngine;

/// <summary>
/// Side-to-side sway for the beams inside a laser group.
/// Attach to Laser Group 4 / 5 / 6 (NOT to individual beams).
/// Drag the group's beams into Lasers in the order they should stagger:
/// Element 0 sways first, then 1, 2, 3. Leave empty to auto-use all
/// direct children. Beams stay parented to the group, so the sway rides
/// on top of the group's sweep from LaserController.
/// </summary>
public class LaserGroupSway : MonoBehaviour
{
    [Header("Lasers (array order = stagger order)")]
    [Tooltip("Drag this group's beams here in order. Element 0 sways first. Leave empty to auto-find direct children. Max 4.")]
    public Transform[] lasers = new Transform[4];

    const int MaxLasers = 4;

    [Header("Sway (group-local space)")]
    [Tooltip("Direction lasers oscillate in. X = side to side for vertical beams.")]
    public Vector3 swayAxis = Vector3.right;
    [Tooltip("How far each beam moves from its center, in meters.")]
    public float amplitude = 0.6f;
    [Tooltip("Full side-to-side cycles per second.")]
    public float frequency = 0.5f;
    [Tooltip("Phase offset between consecutive lasers, in radians. ~2.1 staggers them so they don't sync.")]
    public float phaseStep = 2.1f;

    Transform[] _targets;
    Vector3[] _startLocal;
    Vector3 _axis = Vector3.right;

    void Awake()
    {
        RefreshAxis();
        NormalizeLasers();
        if (lasers == null || lasers.Length == 0)
            AutoFindLasers();
        NormalizeLasers();

        _targets = lasers;
        _startLocal = new Vector3[_targets.Length];
        for (int i = 0; i < _targets.Length; i++)
            _startLocal[i] = _targets[i].localPosition;
    }

    void Update()
    {
        if (_targets == null || _targets.Length == 0) return;
        float angle = Time.time * frequency * Mathf.PI * 2f;
        for (int i = 0; i < _targets.Length; i++)
        {
            if (_targets[i] == null) continue;
            float s = Mathf.Sin(angle + i * phaseStep);
            _targets[i].localPosition = _startLocal[i] + _axis * (s * amplitude);
        }
    }

    void AutoFindLasers()
    {
        System.Collections.Generic.List<Transform> ordered = new System.Collections.Generic.List<Transform>();
        foreach (Transform child in transform)
        {
            if (ordered.Count >= MaxLasers) break;
            ordered.Add(child);
        }
        lasers = ordered.ToArray();
    }

    /// <summary>Enforces max count, drag-drop order. Nulls/duplicates removed, extras trimmed. Array index = stagger order.</summary>
    void NormalizeLasers()
    {
        if (lasers == null) return;
        if (lasers.Length > MaxLasers)
            Debug.LogWarning($"LaserGroupSway supports only {MaxLasers} lasers — extra entries ignored. Array order is stagger order (0 first).", this);
        System.Collections.Generic.List<Transform> valid = new System.Collections.Generic.List<Transform>();
        foreach (Transform t in lasers)
        {
            if (t == null || valid.Contains(t)) continue;
            valid.Add(t);
            if (valid.Count >= MaxLasers) break;
        }
        lasers = valid.ToArray();
    }

    void RefreshAxis()
    {
        _axis = swayAxis.sqrMagnitude > 0.000001f ? swayAxis.normalized : Vector3.right;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        RefreshAxis();
        if (lasers != null && lasers.Length > MaxLasers)
            System.Array.Resize(ref lasers, MaxLasers);
        if (amplitude < 0f) amplitude = 0f;
        if (frequency < 0f) frequency = 0f;
    }
#endif
}
