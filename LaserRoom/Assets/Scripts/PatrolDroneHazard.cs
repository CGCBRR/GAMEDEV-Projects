using UnityEngine;

/// <summary>
/// Query-based patrol drone - the moving hazard of the stealth corridor.
///
/// The drone flies back and forth along its patrol axis (no colliders or triggers).
/// Detection is physics-only and staggered across hazards:
///   1. Physics.CheckSphere around the drone - the volume query that bounds sight,
///   2. a vision-cone angle test against the player's chest,
///   3. a raycast for line of sight, so walls block detection.
/// Staying in sight fills a detection meter. The player's flashlight roughly
/// doubles how fast the meter fills (beam on = you are a lighthouse). When the meter
/// fills, the drone raises the alarm: one burst of damage (a held shield absorbs it,
/// like a laser touch), then it chases along its route for a few seconds and keeps
/// dealing contact damage while it has eyes on the player.
///
/// The drone visual (body, glowing eye, eye light) is built at runtime, so the scene
/// holds nothing but a placeholder object with this script.
/// </summary>
public class PatrolDroneHazard : MonoBehaviour
{
    [Header("Patrol route")]
    [Tooltip("Metres travelled each way from the starting position.")]
    public float patrolRange = 4f;
    [Tooltip("World-space axis the route runs along (Y is ignored; kept horizontal).")]
    public Vector3 patrolAxis = Vector3.forward;
    [Tooltip("Cruise speed along the route (m/s).")]
    public float patrolSpeed = 1.6f;
    [Tooltip("Seconds spent hanging at each end of the route while turning around.")]
    public float endPause = 1.2f;
    [Tooltip("Yaw rate used to face the travel direction or the player (deg/s).")]
    public float turnSpeed = 140f;
    [Tooltip("Gentle vertical bobbing while flying (metres).")]
    public float bobAmplitude = 0.06f;
    public float bobSpeed = 1.7f;

    [Header("Vision")]
    [Tooltip("Maximum sight range - the radius of the CheckSphere volume query.")]
    public float visionRange = 10f;
    [Tooltip("Full cone angle of the vision, in degrees.")]
    public float visionAngle = 70f;
    [Tooltip("Seconds of continuous sight (flashlight off) needed to fill the meter.")]
    public float timeToAlarm = 1.8f;
    [Tooltip("Detection-rate multiplier while the player's flashlight is ON.")]
    public float flashlightRateMultiplier = 2f;
    [Tooltip("Detection meter drain per second while the player is out of sight.")]
    public float meterDecayPerSecond = 0.5f;
    [Tooltip("Seconds between detection checks. Checks are staggered across hazards.")]
    public float checkInterval = 0.1f;

    [Header("Alarm")]
    [Tooltip("Damage dealt the moment the alarm triggers. A held shield absorbs it.")]
    public float alarmDamage = 15f;
    [Tooltip("Seconds the drone stays alerted and chases.")]
    public float alertDuration = 5f;
    [Tooltip("Seconds after an alarm ends before this drone can raise another.")]
    public float alarmCooldown = 3f;
    [Tooltip("Chase speed while alerted - outrun it or break line of sight.")]
    public float chaseSpeed = 3.2f;
    [Tooltip("How far past either end of its route the drone may chase.")]
    public float chaseLeash = 6f;
    [Tooltip("Distance the drone keeps from the player while chasing - it will not fly through you.")]
    public float chaseStandoff = 1.5f;
    [Tooltip("Damage per hit while alerted and touching the player.")]
    public float contactDamage = 7f;
    [Tooltip("Minimum seconds between contact hits.")]
    public float contactInterval = 0.9f;
    [Tooltip("Radius of the contact volume (CheckSphere) around the drone.")]
    public float contactRadius = 1.7f;

    [Header("State (written at runtime)")]
    [Tooltip("Detection progress 0..1. The eye colour shows it.")]
    public float meter;
    public bool alarmed;
    [Tooltip("Result of the most recent detection check.")]
    public bool seesPlayer;
    [Tooltip("Meter gain per second applied by the most recent check (0 when unseen).")]
    public float currentDetectionRate;

    // Route
    Vector3 _routeCenter;
    Vector3 _axis = Vector3.forward;
    int _endIndex = 1;          // endpoint currently being travelled to (0 = -range, 1 = +range)
    float _pauseUntil;

    // Bookkeeping
    float _nextCheck;
    float _lastCheckTime;
    float _alarmEndTime;
    float _nextAlarmAllowedAt;
    float _nextContactTime;

    // Visuals
    Material _eyeMaterial;
    Light _eyeLight;

    static readonly Color CalmColor = new Color(0.25f, 0.9f, 1f);
    static readonly Color WarnColor = new Color(1f, 0.75f, 0.15f);
    static readonly Color AlertColor = new Color(1f, 0.18f, 0.12f);

    void Awake()
    {
        _routeCenter = transform.position;
        _axis = patrolAxis;
        _axis.y = 0f;
        _axis = _axis.sqrMagnitude > 0.0001f ? _axis.normalized : Vector3.forward;
        BuildVisual();
    }

    void Start()
    {
        _lastCheckTime = Time.time;
        // Stagger detection checks across hazards (same slot scheme as the beams).
        _nextCheck = Time.time + checkInterval * 0.25f * (StealthPhysics.NextStaggerSlot() % 4);
        transform.rotation = Quaternion.LookRotation(_axis, Vector3.up);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;   // paused (round won / timed out): freeze everything

        MoveAndFace(dt);
        DetectionTick();
        UpdateEye();
    }

    // ------------------------------------------------------------------ movement

    void MoveAndFace(float dt)
    {
        float myProj = Vector3.Dot(transform.position - _routeCenter, _axis);
        Vector3 desiredForward = transform.forward;

        if (alarmed)
        {
            if (StealthPhysics.HasPlayer)
            {
                float playerProj = Vector3.Dot(StealthPhysics.PlayerAimPoint - _routeCenter, _axis);

                // Stand off at arm's length: right on top of the player the view cone
                // folds onto itself and the drone would go blind. It also stops the
                // drone from flying straight through the player.
                float gap = myProj - playerProj;
                float side = Mathf.Abs(gap) > 0.001f ? Mathf.Sign(gap) : 1f;
                float standoffTarget = playerProj + side * chaseStandoff;
                float chaseTarget = Mathf.Clamp(standoffTarget, -patrolRange - chaseLeash, patrolRange + chaseLeash);

                // Only advance while the player is actually in sight - a drone that lost
                // them hovers and scans instead of racing to the edge of its leash.
                if (seesPlayer)
                    myProj = Mathf.MoveTowards(myProj, chaseTarget, chaseSpeed * dt);

                desiredForward = FlatDir(StealthPhysics.PlayerAimPoint - transform.position);
                if (desiredForward.sqrMagnitude < 0.0001f) desiredForward = transform.forward;
            }
            // No player: hover where it is, keep watching forward.
        }
        else
        {
            float endProj = _endIndex == 1 ? patrolRange : -patrolRange;
            if (Time.time < _pauseUntil)
            {
                // Hanging at the end of the route, turning toward the way back.
            }
            else
            {
                float newProj = Mathf.MoveTowards(myProj, endProj, patrolSpeed * dt);
                if (Mathf.Approximately(newProj, endProj))
                {
                    _endIndex = 1 - _endIndex;            // next leg goes the other way
                    _pauseUntil = Time.time + endPause;
                }
                myProj = newProj;
            }
            desiredForward = _axis * (_endIndex == 1 ? 1f : -1f);
        }

        Vector3 pos = _routeCenter + _axis * myProj;
        pos.y = _routeCenter.y + Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        transform.position = pos;

        if (desiredForward.sqrMagnitude > 0.0001f)
        {
            Quaternion want = Quaternion.LookRotation(desiredForward, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, want, turnSpeed * dt);
        }
    }

    // ----------------------------------------------------------------- detection

    void DetectionTick()
    {
        float now = Time.time;
        if (now < _nextCheck) return;
        float dt = now - _lastCheckTime;
        _lastCheckTime = now;
        _nextCheck = now + checkInterval;

        bool seen = CanSeePlayer();
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

        if (alarmed)
        {
            if (now >= _alarmEndTime)
            {
                alarmed = false;
                meter = 0f;
                _nextAlarmAllowedAt = now + alarmCooldown;
                Debug.Log($"{name}: lost the player, back to patrol.", this);
            }
            else if (seen && now >= _nextContactTime
                     && StealthPhysics.SphereHitsPlayer(transform.position, contactRadius)
                     && StealthPhysics.HasLineOfSightToPlayer(transform.position))
            {
                _nextContactTime = now + contactInterval;
                if (!hp.ConsumeShield())
                    hp.TakeDamage(contactDamage);
            }
        }
    }

    /// <summary>Volume query (CheckSphere) + cone angle + line-of-sight raycast.</summary>
    bool CanSeePlayer()
    {
        if (!StealthPhysics.HasPlayer) return false;

        Vector3 eye = transform.position;

        // 1. Everything outside the sphere volume is invisible.
        if (!StealthPhysics.SphereHitsPlayer(eye, visionRange)) return false;

        // 2. Vision cone (flat, so the drone's altitude does not eat the angle).
        Vector3 target = StealthPhysics.PlayerAimPoint;
        Vector3 to = target - eye;
        if (to.sqrMagnitude > 0.0001f)
        {
            Vector3 flatFacing = FlatDir(transform.forward);
            Vector3 flatTo = FlatDir(to);
            float halfAngleCos = Mathf.Cos(visionAngle * 0.5f * Mathf.Deg2Rad);
            if (flatFacing.sqrMagnitude < 0.0001f || Vector3.Dot(flatFacing, flatTo) < halfAngleCos)
                return false;
        }

        // 3. A wall between drone and player blocks the sight.
        return StealthPhysics.HasLineOfSight(eye, target);
    }

    void RaiseAlarm(PlayerHealth hp)
    {
        alarmed = true;
        meter = 1f;
        _alarmEndTime = Time.time + alertDuration;
        _nextContactTime = Time.time + contactInterval * 0.5f;

        if (!hp.ConsumeShield())
            hp.TakeDamage(alarmDamage);
        hp.ReportAlarm();

        StealthHUD.ShowToast("Spotted by a drone!");
        Debug.Log($"{name}: ALARM - player detected (flashlight {(PlayerFlashlight.IsOn ? "ON" : "off")}).", this);
    }

    // ------------------------------------------------------------------- visuals

    void BuildVisual()
    {
        Transform visual = new GameObject("Visual").transform;
        visual.SetParent(transform, false);

        Material bodyMaterial = StealthVisuals.MakeLit(new Color(0.16f, 0.17f, 0.2f), 0.65f, 0.55f, "DroneBody (Runtime)");

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        body.name = "Body";
        body.transform.SetParent(visual, false);
        body.transform.localScale = Vector3.one * 0.5f;
        body.GetComponent<Renderer>().sharedMaterial = bodyMaterial;
        StripCollider(body);

        _eyeMaterial = StealthVisuals.MakeEmissive(CalmColor, 2.6f, "DroneEye (Runtime)");
        GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        eye.name = "Eye";
        eye.transform.SetParent(visual, false);
        eye.transform.localPosition = new Vector3(0f, 0.02f, 0.21f);
        eye.transform.localScale = Vector3.one * 0.16f;
        eye.GetComponent<Renderer>().sharedMaterial = _eyeMaterial;
        StripCollider(eye);

        GameObject lightGo = new GameObject("EyeLight");
        lightGo.transform.SetParent(visual, false);
        lightGo.transform.localPosition = new Vector3(0f, 0.02f, 0.3f);
        _eyeLight = lightGo.AddComponent<Light>();
        _eyeLight.type = LightType.Point;
        _eyeLight.range = 3.5f;
        _eyeLight.intensity = 0.4f;
        _eyeLight.color = CalmColor;
        _eyeLight.shadows = LightShadows.None;
    }

    static void StripCollider(GameObject go)
    {
        Collider c = go.GetComponent<Collider>();
        if (c == null) return;
        c.enabled = false;
        Destroy(c);
    }

    void UpdateEye()
    {
        float t = alarmed ? 1f : meter;
        Color color = t < 0.5f
            ? Color.Lerp(CalmColor, WarnColor, t * 2f)
            : Color.Lerp(WarnColor, AlertColor, (t - 0.5f) * 2f);

        float glow = 2.6f + t * 1.6f;
        if (alarmed) glow += 1.2f + Mathf.Sin(Time.time * 10f) * 0.6f;

        if (_eyeMaterial != null)
            StealthVisuals.SetEmissive(_eyeMaterial, color, glow);

        if (_eyeLight != null)
        {
            _eyeLight.color = color;
            _eyeLight.intensity = 0.35f + t * 1.4f;
        }
    }

    // -------------------------------------------------------------------- helpers

    static Vector3 FlatDir(Vector3 v)
    {
        v.y = 0f;
        return v.sqrMagnitude > 0.0001f ? v.normalized : Vector3.zero;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        patrolRange = Mathf.Max(0.1f, patrolRange);
        patrolSpeed = Mathf.Max(0.05f, patrolSpeed);
        endPause = Mathf.Max(0f, endPause);
        turnSpeed = Mathf.Max(10f, turnSpeed);
        visionRange = Mathf.Max(0.5f, visionRange);
        visionAngle = Mathf.Clamp(visionAngle, 5f, 170f);
        timeToAlarm = Mathf.Max(0.1f, timeToAlarm);
        flashlightRateMultiplier = Mathf.Max(1f, flashlightRateMultiplier);
        checkInterval = Mathf.Max(0.02f, checkInterval);
        alertDuration = Mathf.Max(0.1f, alertDuration);
        alarmCooldown = Mathf.Max(0f, alarmCooldown);
        chaseStandoff = Mathf.Clamp(chaseStandoff, 0.5f, 6f);
        contactRadius = Mathf.Max(0.2f, contactRadius);
        if (patrolAxis.sqrMagnitude < 0.0001f) patrolAxis = Vector3.forward;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? _routeCenter : transform.position;
        Vector3 axis = patrolAxis;
        axis.y = 0f;
        axis = axis.sqrMagnitude > 0.0001f ? axis.normalized : Vector3.forward;

        Vector3 a = center - axis * patrolRange;
        Vector3 b = center + axis * patrolRange;
        Gizmos.color = new Color(1f, 0.7f, 0.1f, 0.9f);
        Gizmos.DrawLine(a, b);
        Gizmos.DrawWireSphere(a, 0.15f);
        Gizmos.DrawWireSphere(b, 0.15f);

        Vector3 fwd = FlatDir(transform.forward);
        if (fwd.sqrMagnitude > 0.0001f)
        {
            Gizmos.color = new Color(1f, 0.35f, 0.15f, 0.5f);
            const int steps = 14;
            Vector3 prev = center + Quaternion.AngleAxis(-visionAngle * 0.5f, Vector3.up) * fwd * visionRange;
            for (int i = 1; i <= steps; i++)
            {
                Vector3 next = center + Quaternion.AngleAxis(-visionAngle * 0.5f + visionAngle * i / steps, Vector3.up) * fwd * visionRange;
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
            Gizmos.DrawLine(center, center + Quaternion.AngleAxis(-visionAngle * 0.5f, Vector3.up) * fwd * visionRange);
            Gizmos.DrawLine(center, center + Quaternion.AngleAxis(visionAngle * 0.5f, Vector3.up) * fwd * visionRange);
        }
    }
#endif
}
