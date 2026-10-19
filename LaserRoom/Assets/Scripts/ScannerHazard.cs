using UnityEngine;

/// <summary>
/// Sweeping scanner - the Phase 5 detection hazard.
///
/// A wall-mounted emitter slides along its rail and drags a detection curtain with
/// it. All detection is query-only (no colliders or triggers): an oriented CheckBox
/// bounds the curtain, then a line-of-sight ray confirms the player. The curtain
/// rides above crouch height, so there are two counters: duck under it (C) or sprint
/// straight through before the meter fills. Standing in the wash fills the meter
/// twice as fast while the flashlight is on.
///
/// Tripping the meter raises a scanner alarm: one burst of damage (a held shield
/// absorbs it, like a laser touch), a toast, then a cooldown before it can trip
/// again.
///
/// The visual (rail, emitter head, glowing curtain) is built at runtime, so the
/// scene holds nothing but a placeholder object with this script.
/// </summary>
public class ScannerHazard : MonoBehaviour
{
    [Header("Sweep")]
    [Tooltip("Half-length of the rail measured along the world Z axis.")]
    public float sweepRange = 1.3f;
    [Tooltip("Curtain travel speed (m/s).")]
    public float sweepSpeed = 2.6f;
    [Tooltip("Seconds the curtain rests at each end of the rail.")]
    public float endPause = 0.6f;
    [Tooltip("Seconds before the first sweep starts (stagger the scanners).")]
    public float startDelay = 1f;

    [Header("Detection curtain")]
    [Tooltip("Half-width of the curtain volume. Queries only test the player's layer.")]
    public float slabHalfWidth = 4.9f;
    [Tooltip("Bottom edge of the curtain relative to this object (place the scanner at floor level).")]
    public float slabBottom = 1.35f;
    [Tooltip("Top edge of the curtain relative to this object.")]
    public float slabTop = 2.6f;
    [Tooltip("Half-depth of the curtain along the sweep axis.")]
    public float slabHalfDepth = 0.8f;
    [Tooltip("Half-width of the glowing curtain visual (roughly half the corridor width).")]
    public float visualHalfWidth = 4.75f;

    [Header("Detection")]
    [Tooltip("Seconds of continuous exposure (flashlight off) needed to trip the alarm.")]
    public float timeToAlarm = 0.8f;
    [Tooltip("Detection-rate multiplier while the player's flashlight is ON.")]
    public float flashlightRateMultiplier = 2f;
    [Tooltip("Detection meter drain per second while the player is clear of the curtain.")]
    public float meterDecayPerSecond = 0.6f;
    [Tooltip("Seconds between detection checks. Checks are staggered across hazards.")]
    public float checkInterval = 0.1f;

    [Header("Alarm")]
    [Tooltip("Damage dealt the moment the alarm trips. A held shield absorbs it.")]
    public float alarmDamage = 15f;
    [Tooltip("Seconds the alarm state (flashing curtain) lasts.")]
    public float alarmDuration = 1.2f;
    [Tooltip("Seconds after an alarm before this scanner can trip another.")]
    public float alarmCooldown = 3f;

    [Header("State (written at runtime)")]
    [Tooltip("Detection progress 0..1. The curtain colour shows it.")]
    public float meter;
    public bool alarmed;
    [Tooltip("Result of the most recent detection check.")]
    public bool seesPlayer;
    [Tooltip("Meter gain per second applied by the most recent check (0 when unseen).")]
    public float currentDetectionRate;
    [Tooltip("Current offset of the curtain from the anchor (-sweepRange..sweepRange).")]
    public float sweepOffset;

    // Sweep state
    float _dir = 1f;
    float _pauseUntil;

    // Detection bookkeeping
    float _nextCheck;
    float _lastCheckTime;
    float _alarmEndTime;
    float _nextAlarmAllowedAt;

    // Visuals
    Transform _curtain;
    Transform _emitter;
    Material _curtainMaterial;
    Material _lensMaterial;
    Light _lensLight;

    static readonly Color CalmColor = new Color(1f, 0.62f, 0.2f);
    static readonly Color AlertColor = new Color(1f, 0.12f, 0.08f);
    const float CurtainAlpha = 0.3f;

    void Awake()
    {
        BuildVisual();
    }

    void Start()
    {
        sweepOffset = -sweepRange;
        _pauseUntil = Time.time + startDelay;
        _lastCheckTime = Time.time;
        _nextCheck = Time.time + checkInterval * 0.25f * (StealthPhysics.NextStaggerSlot() % 4);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;   // paused (round won / timed out): freeze everything

        Sweep(dt);
        DetectionTick();
        UpdateGlow();
    }

    // -------------------------------------------------------------------- sweep

    void Sweep(float dt)
    {
        if (Time.time >= _pauseUntil)
        {
            sweepOffset += _dir * sweepSpeed * dt;
            if (sweepOffset >= sweepRange)
            {
                sweepOffset = sweepRange;
                _dir = -1f;
                _pauseUntil = Time.time + endPause;
            }
            else if (sweepOffset <= -sweepRange)
            {
                sweepOffset = -sweepRange;
                _dir = 1f;
                _pauseUntil = Time.time + endPause;
            }
        }

        float midY = (slabBottom + slabTop) * 0.5f;
        if (_curtain != null)
            _curtain.localPosition = new Vector3(0f, midY, sweepOffset);
        if (_emitter != null)
            _emitter.localPosition = new Vector3(-(visualHalfWidth + 0.02f), midY, sweepOffset);
    }

    // ---------------------------------------------------------------- detection

    void DetectionTick()
    {
        float now = Time.time;
        if (now < _nextCheck) return;
        float dt = now - _lastCheckTime;
        _lastCheckTime = now;
        _nextCheck = now + checkInterval;

        bool seen = CurtainHitsPlayer();
        seesPlayer = seen;

        if (seen)
        {
            float rate = 1f / Mathf.Max(0.05f, timeToAlarm);
            if (PlayerFlashlight.IsOn) rate *= flashlightRateMultiplier;
            currentDetectionRate = rate;
            if (meter < 1f) meter = Mathf.Min(1f, meter + rate * dt);
        }
        else
        {
            currentDetectionRate = 0f;
            if (!alarmed && meter > 0f) meter = Mathf.Max(0f, meter - meterDecayPerSecond * dt);
        }

        PlayerHealth hp = StealthPhysics.Player;
        if (hp == null) return;

        if (!alarmed && meter >= 1f && now >= _nextAlarmAllowedAt)
            RaiseAlarm(hp);

        if (alarmed && now >= _alarmEndTime)
        {
            alarmed = false;
            meter = 0f;
            _nextAlarmAllowedAt = now + alarmCooldown;
            Debug.Log($"{name}: scan complete, curtain back to standby.", this);
        }
    }

    /// <summary>CheckBox volume query along the curtain, then a line-of-sight ray.</summary>
    bool CurtainHitsPlayer()
    {
        if (!StealthPhysics.HasPlayer) return false;

        Vector3 center = transform.TransformPoint(new Vector3(0f, (slabBottom + slabTop) * 0.5f, sweepOffset));
        Vector3 halfExtents = new Vector3(slabHalfWidth, (slabTop - slabBottom) * 0.5f, slabHalfDepth);

        if (!StealthPhysics.BoxHitsPlayer(center, halfExtents, transform.rotation)) return false;
        return StealthPhysics.HasLineOfSight(center, StealthPhysics.PlayerAimPoint);
    }

    void RaiseAlarm(PlayerHealth hp)
    {
        alarmed = true;
        meter = 1f;
        _alarmEndTime = Time.time + alarmDuration;

        if (!hp.ConsumeShield())
            hp.TakeDamage(alarmDamage);
        hp.ReportAlarm();

        StealthHUD.ShowToast("Spotted by a scanner!");
        Debug.Log($"{name}: ALARM - curtain swept the player (flashlight {(PlayerFlashlight.IsOn ? "ON" : "off")}).", this);
    }

    // ----------------------------------------------------------------- visuals

    void BuildVisual()
    {
        Transform visual = new GameObject("Visual").transform;
        visual.SetParent(transform, false);

        float midY = (slabBottom + slabTop) * 0.5f;
        float height = slabTop - slabBottom;

        // Glowing curtain = the detection volume.
        _curtainMaterial = StealthVisuals.MakeCurtain(CalmColor, 1.3f, "ScannerCurtain (Runtime)");
        GameObject curtain = GameObject.CreatePrimitive(PrimitiveType.Cube);
        curtain.name = "Curtain";
        curtain.transform.SetParent(visual, false);
        curtain.transform.localPosition = new Vector3(0f, midY, 0f);
        curtain.transform.localScale = new Vector3(visualHalfWidth * 2f, height, slabHalfDepth * 2f);
        curtain.GetComponent<Renderer>().sharedMaterial = _curtainMaterial;
        StripCollider(curtain);
        _curtain = curtain.transform;

        // Rail the emitter slides along (left wall).
        Material railMaterial = StealthVisuals.MakeLit(new Color(0.14f, 0.15f, 0.18f), 0.7f, 0.5f, "ScannerRail (Runtime)");
        GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rail.name = "Rail";
        rail.transform.SetParent(visual, false);
        rail.transform.localPosition = new Vector3(-(visualHalfWidth + 0.06f), midY, 0f);
        rail.transform.localScale = new Vector3(0.07f, 0.14f, sweepRange * 2f + 0.5f);
        rail.GetComponent<Renderer>().sharedMaterial = railMaterial;
        StripCollider(rail);

        // Emitter head that rides with the curtain.
        GameObject emitter = GameObject.CreatePrimitive(PrimitiveType.Cube);
        emitter.name = "Emitter";
        emitter.transform.SetParent(visual, false);
        emitter.transform.localScale = new Vector3(0.16f, 0.3f, 0.34f);
        emitter.GetComponent<Renderer>().sharedMaterial = railMaterial;
        StripCollider(emitter);
        _emitter = emitter.transform;

        // Glowing lens on the corridor-facing side of the emitter.
        _lensMaterial = StealthVisuals.MakeEmissive(CalmColor, 2.2f, "ScannerLens (Runtime)");
        GameObject lens = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lens.name = "Lens";
        lens.transform.SetParent(emitter.transform, false);
        lens.transform.localPosition = new Vector3(0.52f, 0f, 0f);              // half emitter width, flush on its face
        lens.transform.localScale = new Vector3(0.3f, 0.72f, 0.7f);             // compensates the emitter's scale
        lens.GetComponent<Renderer>().sharedMaterial = _lensMaterial;
        StripCollider(lens);

        GameObject lightGo = new GameObject("ScanLight");
        lightGo.transform.SetParent(emitter.transform, false);
        lightGo.transform.localPosition = new Vector3(0.6f, 0f, 0f);
        _lensLight = lightGo.AddComponent<Light>();
        _lensLight.type = LightType.Point;
        _lensLight.range = 3.2f;
        _lensLight.intensity = 0.35f;
        _lensLight.color = CalmColor;
        _lensLight.shadows = LightShadows.None;
    }

    void UpdateGlow()
    {
        float t = alarmed ? 1f : meter;
        Color color = t < 0.5f
            ? Color.Lerp(CalmColor, new Color(1f, 0.35f, 0.1f), t * 2f)
            : Color.Lerp(new Color(1f, 0.35f, 0.1f), AlertColor, (t - 0.5f) * 2f);

        float glow = 1.3f + t * 1.6f;
        if (alarmed) glow += 1.4f + Mathf.Sin(Time.time * 12f) * 0.7f;

        if (_curtainMaterial != null)
        {
            Color baseColor = color;
            baseColor.a = CurtainAlpha;
            if (_curtainMaterial.HasProperty("_BaseColor")) _curtainMaterial.SetColor("_BaseColor", baseColor);
            if (_curtainMaterial.HasProperty("_Color")) _curtainMaterial.SetColor("_Color", baseColor);
            if (_curtainMaterial.HasProperty("_EmissionColor")) _curtainMaterial.SetColor("_EmissionColor", color * glow);
        }
        if (_lensMaterial != null)
            StealthVisuals.SetEmissive(_lensMaterial, color, 2.2f + t * 1.4f);
        if (_lensLight != null)
        {
            _lensLight.color = color;
            _lensLight.intensity = 0.35f + t * 1.5f;
        }
    }

    static void StripCollider(GameObject go)
    {
        Collider c = go.GetComponent<Collider>();
        if (c == null) return;
        c.enabled = false;
        Destroy(c);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        sweepRange = Mathf.Max(0.1f, sweepRange);
        sweepSpeed = Mathf.Max(0.05f, sweepSpeed);
        endPause = Mathf.Max(0f, endPause);
        slabBottom = Mathf.Max(0.1f, slabBottom);
        slabTop = Mathf.Max(slabBottom + 0.2f, slabTop);
        slabHalfDepth = Mathf.Max(0.1f, slabHalfDepth);
        visualHalfWidth = Mathf.Max(0.2f, visualHalfWidth);
        timeToAlarm = Mathf.Max(0.1f, timeToAlarm);
        flashlightRateMultiplier = Mathf.Max(1f, flashlightRateMultiplier);
        checkInterval = Mathf.Max(0.02f, checkInterval);
        alarmDuration = Mathf.Max(0.1f, alarmDuration);
        alarmCooldown = Mathf.Max(0f, alarmCooldown);
    }

    void OnDrawGizmosSelected()
    {
        float midY = (slabBottom + slabTop) * 0.5f;
        Vector3 size = new Vector3(slabHalfWidth * 2f, slabTop - slabBottom, slabHalfDepth * 2f);

        Gizmos.color = new Color(1f, 0.5f, 0.1f, 0.6f);
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(new Vector3(0f, midY, Application.isPlaying ? sweepOffset : 0f), size);
        Gizmos.DrawLine(new Vector3(-visualHalfWidth, midY, -sweepRange), new Vector3(-visualHalfWidth, midY, sweepRange));
    }
#endif
}
