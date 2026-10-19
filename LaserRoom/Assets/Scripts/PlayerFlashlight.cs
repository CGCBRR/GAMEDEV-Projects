using StarterAssets;
using UnityEngine;

/// <summary>
/// Player flashlight, toggled with F (or gamepad Y).
///
/// It drives two things:
///  - a spot light on the camera (pure visuals), and
///  - the hidden-object reveal system: every <see cref="checkInterval"/> seconds each
///    registered <see cref="HiddenObject"/> is tested against the beam with physics
///    only — range, cone angle, then a linecast so walls block the reveal.
/// </summary>
[RequireComponent(typeof(StarterAssetsInputs))]
public class PlayerFlashlight : MonoBehaviour
{
    [Header("Beam")]
    [Tooltip("How far the flashlight can reveal hidden objects.")]
    public float range = 14f;
    [Tooltip("Cone angle in degrees (matches the spot light).")]
    public float spotAngle = 50f;
    [Tooltip("Objects this close to the camera are revealed even at steep angles - the light spills around your feet.")]
    public float nearRevealRadius = 2.2f;
    [Tooltip("Seconds between cone checks.")]
    public float checkInterval = 0.06f;

    [Header("Light")]
    public Color lightColor = new Color(1f, 0.95f, 0.82f);
    public float lightIntensity = 3.2f;
    public LightShadows shadows = LightShadows.Soft;

    static PlayerFlashlight _instance;

    public static PlayerFlashlight Instance => _instance;
    public static bool IsOn => _instance != null && _instance.on;

    [Tooltip("Current state. Toggled by the input binding; also drivable from scripts.")]
    public bool on;

    StarterAssetsInputs _input;
    Light _light;
    Camera _camera;
    float _nextCheck;

    void Awake()
    {
        _instance = this;
        _input = GetComponent<StarterAssetsInputs>();
    }

    void Start()
    {
        _camera = Camera.main;
        if (_camera == null) _camera = GetComponentInChildren<Camera>(true);
        BuildLight();
        SetOn(on);
        _nextCheck = Time.time + 0.2f;
        Debug.Log("Press F (or gamepad Y) to toggle the flashlight - hidden objects only show up in its cone.");
    }

    void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    void Update()
    {
        // Keep visuals/reveals in sync if the state was set from elsewhere.
        if (_light != null && _light.enabled != on)
            SetOn(on);

        // The Button action only delivers the press (the release never reaches
        // value.isPressed), so like Jump in the Starter Assets controller the press is
        // consumed here. That is what lets every F tap toggle the light.
        if (_input != null && _input.flashlight)
        {
            _input.flashlight = false;
            SetOn(!on);
        }

        if (!on) return;
        if (Time.time < _nextCheck) return;
        _nextCheck = Time.time + checkInterval;
        Sweep();
    }

    /// <summary>Turns the flashlight on or off and applies the state to the light and reveals.</summary>
    public void SetOn(bool value)
    {
        on = value;
        if (_light != null) _light.enabled = on;
        if (!on) RevealNone();
    }

    void BuildLight()
    {
        if (_camera == null) return;
        Transform parent = _camera.transform;
        Transform existing = parent.Find("Flashlight");
        GameObject go = existing != null ? existing.gameObject : new GameObject("Flashlight");
        go.transform.SetParent(parent, false);
        _light = go.GetComponent<Light>();
        if (_light == null) _light = go.AddComponent<Light>();
        _light.type = LightType.Spot;
        _light.spotAngle = spotAngle;
        _light.range = range;
        _light.color = lightColor;
        _light.intensity = lightIntensity;
        _light.shadows = shadows;
        _light.enabled = false;
    }

    void Sweep()
    {
        if (_camera == null) return;
        Vector3 origin = _camera.transform.position;
        Vector3 forward = _camera.transform.forward;
        float halfAngleCos = Mathf.Cos((spotAngle * 0.5f + 3f) * Mathf.Deg2Rad); // small forgiving margin

        for (int i = HiddenObject.All.Count - 1; i >= 0; i--)
        {
            HiddenObject hidden = HiddenObject.All[i];
            if (hidden == null)
            {
                HiddenObject.All.RemoveAt(i);
                continue;
            }
            hidden.SetLit(CanSee(origin, forward, halfAngleCos, hidden.transform.position));
        }
    }

    bool CanSee(Vector3 origin, Vector3 forward, float halfAngleCos, Vector3 target)
    {
        Vector3 to = target - origin;
        float distance = to.magnitude;
        if (distance > range) return false;

        // Inside the near radius the cone test is skipped, otherwise floor-level
        // objects would blink out exactly when the player walks up to them.
        if (distance > nearRevealRadius && Vector3.Dot(forward, to / distance) < halfAngleCos)
            return false;

        // Occlusion: a wall in front blocks the reveal. Triggers are ignored, and the
        // hit lands at or behind the object when the view is genuinely clear.
        return !Physics.Linecast(origin, target, out RaycastHit hit, ~0, QueryTriggerInteraction.Ignore)
            || hit.distance >= distance - 0.2f;
    }

    void RevealNone()
    {
        for (int i = HiddenObject.All.Count - 1; i >= 0; i--)
        {
            HiddenObject hidden = HiddenObject.All[i];
            if (hidden == null)
            {
                HiddenObject.All.RemoveAt(i);
                continue;
            }
            hidden.SetLit(false);
        }
    }
}
