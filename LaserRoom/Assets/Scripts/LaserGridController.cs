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
    static Material _runtimeRed;

    void Awake()
    {
        if (lasers == null || lasers.Length == 0)
            AutoFindLasers();

        _startPos = new Vector3[lasers.Length];
        for (int i = 0; i < lasers.Length; i++)
        {
            if (lasers[i] == null) continue;
            _startPos[i] = lasers[i].position;
            EnsureTriggerCollider(lasers[i]);
            EnsureDamage(lasers[i]);
        }

        if (laserMaterial == null)
            laserMaterial = GetRuntimeRed();
        ApplyMaterial();
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

    void EnsureTriggerCollider(Transform laser)
    {
        // Triggers on concave MeshColliders are NOT supported by Unity,
        // and a solid beam would physically block the player, so every
        // laser gets a Box trigger sized to its visible beam instead.
        // NOTE: size comes straight from the mesh's LOCAL bounds - dividing
        // world bounds by lossy scale per-axis blows up on rotated beams.
        foreach (Collider c in laser.GetComponents<Collider>())
            Object.Destroy(c);
        MeshFilter mf = laser.GetComponent<MeshFilter>();
        BoxCollider box = laser.gameObject.AddComponent<BoxCollider>();
        box.isTrigger = true;
        if (mf != null && mf.sharedMesh != null)
        {
            // thin grid bar + margin, in the beam's own space (goes through
            // the exact same transform as the mesh, so rotation/scale match)
            box.size = mf.sharedMesh.bounds.size * triggerMargin;
            box.center = mf.sharedMesh.bounds.center;
        }
        else
        {
            // corridor-width grid plane fallback (corridor is ~3m wide, 3m tall)
            box.size = new Vector3(3f, 3f, 0.2f);
        }
    }

    void EnsureDamage(Transform laser)
    {
        LaserDamage dmg = laser.GetComponent<LaserDamage>();
        if (dmg == null)
        {
            dmg = laser.gameObject.AddComponent<LaserDamage>();
            dmg.damagePerSecond = damagePerSecond;
            dmg.hitDamage = hitDamage;
        }
    }

    void ApplyMaterial()
    {
        if (laserMaterial == null) return;
        foreach (Transform laser in lasers)
        {
            if (laser == null) continue;
            foreach (Renderer r in laser.GetComponentsInChildren<Renderer>())
                r.sharedMaterial = laserMaterial;
        }
    }

    static Material GetRuntimeRed()
    {
        if (_runtimeRed != null) return _runtimeRed;
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        if (s == null) s = Shader.Find("Standard");
        _runtimeRed = new Material(s);
        _runtimeRed.name = "LaserRed (Runtime)";
        _runtimeRed.color = Color.red;
        if (_runtimeRed.HasProperty("_BaseColor"))
            _runtimeRed.SetColor("_BaseColor", Color.red);
        if (_runtimeRed.HasProperty("_EmissionColor"))
        {
            _runtimeRed.SetColor("_EmissionColor", Color.red * 2f);
            _runtimeRed.EnableKeyword("_EMISSION");
        }
        return _runtimeRed;
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
