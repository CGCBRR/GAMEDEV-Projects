using UnityEngine;

/// <summary>
/// Resident-Evil style laser grid (attach to the "Phase 1" parent).
/// Laser 1 sweeps first, then 2, then 3, each moving toward the player
/// down the corridor. Found automatically by child name (Laser 1..3).
/// </summary>
public class LaserGridController : MonoBehaviour
{
    [Header("Lasers (sweep order: 1 first)")]
    [Tooltip("Leave empty to auto-find children named 'Laser 1', 'Laser 2', 'Laser 3'.")]
    public Transform[] lasers;

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
        if (lasers == null || lasers.Length == 0)
            AutoFindLasers();

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
        Transform[] found = new Transform[3];
        int count = 0;
        for (int i = 1; i <= 3; i++)
        {
            Transform t = transform.Find("Laser " + i);
            if (t == null) t = transform.Find("Laser" + i);
            if (t == null) t = transform.Find("Laser (" + i + ")");
            if (t != null) found[count++] = t;
        }
        if (count == 0)
        {
            // fallback: first 3 children in hierarchy order
            count = 0;
            foreach (Transform child in transform)
            {
                if (count >= 3) break;
                found[count++] = child;
            }
        }
        lasers = found;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 dir = moveDirection.normalized;
        foreach (Transform laser in lasers)
        {
            if (laser == null) continue;
            Vector3 from = Application.isPlaying && _startPos != null ? _startPos[System.Array.IndexOf(lasers, laser)] : laser.position;
            Gizmos.DrawLine(from, from + dir * travelDistance);
        }
    }
#endif
}
