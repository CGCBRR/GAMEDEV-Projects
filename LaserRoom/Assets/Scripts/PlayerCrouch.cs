using StarterAssets;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Toggle crouch on C. Lowers the CharacterController and the camera rig, and slows
/// movement while crouched. Scanner curtains sweep above crouch height, so ducking
/// under them is the safe route through a scan.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerCrouch : MonoBehaviour
{
    [Header("Stance")]
    [Tooltip("CharacterController height while crouched (standing height is read from the controller).")]
    public float crouchHeight = 1.2f;
    [Tooltip("How fast the body and camera slide between stances.")]
    public float transitionSpeed = 9f;
    [Tooltip("Movement speed while crouched (sprint boost is disabled in a crouch).")]
    public float crouchMoveSpeed = 2.2f;
    [Tooltip("How far the camera drops when fully crouched.")]
    public float cameraDrop = 0.55f;

    [Header("Stand-up check")]
    [Tooltip("Layers that block standing back up (world geometry).")]
    public LayerMask blockingLayers = ~0;
    [Tooltip("Shrink the stand-up probe radius by this to avoid wall-hug false positives.")]
    public float probeShrink = 0.12f;

    [Header("State (written at runtime)")]
    public bool crouched;

    static PlayerCrouch _instance;

    /// <summary>True while the player is (targeting) crouched - used by the HUD.</summary>
    public static bool IsCrouched => _instance != null && _instance.crouched;

    CharacterController _cc;
    FirstPersonController _fpc;
    Transform _camTarget;
    float _standHeight, _standCenterY, _standCamY, _standMove, _standSprint;
    float _blend;

    void Awake()
    {
        _instance = this;
        _cc = GetComponent<CharacterController>();
        _fpc = GetComponent<FirstPersonController>();
        if (_fpc != null && _fpc.CinemachineCameraTarget != null)
            _camTarget = _fpc.CinemachineCameraTarget.transform;

        _standHeight = _cc.height;
        _standCenterY = _cc.center.y;
        _standMove = _fpc != null ? _fpc.MoveSpeed : 4f;
        _standSprint = _fpc != null ? _fpc.SprintSpeed : 6f;
        _standCamY = _camTarget != null ? _camTarget.localPosition.y : 1.375f;
    }

    void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    void Update()
    {
        if (Time.deltaTime <= 0f) return;   // frozen rounds ignore stance input

#if ENABLE_INPUT_SYSTEM
        bool toggle = Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame;
#else
        bool toggle = Input.GetKeyDown(KeyCode.C);
#endif
        if (toggle) Toggle();

        _blend = Mathf.MoveTowards(_blend, crouched ? 1f : 0f, transitionSpeed * Time.deltaTime);
        Apply(_blend);
    }

    /// <summary>Switches stance; standing back up needs headroom above the crouched head.</summary>
    public void Toggle()
    {
        if (!crouched) crouched = true;
        else if (StandClear()) crouched = false;
    }

    /// <summary>Debug hook for the editor test menu.</summary>
    public void DebugToggle() => Toggle();

    void Apply(float t)
    {
        _cc.height = Mathf.Lerp(_standHeight, crouchHeight, t);
        Vector3 center = _cc.center;
        center.y = Mathf.Lerp(_standCenterY, _standCenterY - (_standHeight - crouchHeight) * 0.5f, t);
        _cc.center = center;

        if (_camTarget != null)
        {
            Vector3 p = _camTarget.localPosition;
            p.y = Mathf.Lerp(_standCamY, _standCamY - cameraDrop, t);
            _camTarget.localPosition = p;
        }

        if (_fpc != null)
        {
            _fpc.MoveSpeed = Mathf.Lerp(_standMove, crouchMoveSpeed, t);
            _fpc.SprintSpeed = Mathf.Lerp(_standSprint, crouchMoveSpeed, t);
        }
    }

    bool StandClear()
    {
        float radius = Mathf.Max(0.05f, _cc.radius - probeShrink);
        float crouchTop = _cc.center.y + crouchHeight * 0.5f;
        float standTop = _standCenterY + _standHeight * 0.5f;
        float probeY = Mathf.Lerp(crouchTop, standTop, 0.5f);
        Vector3 probe = transform.TransformPoint(new Vector3(0f, probeY, 0f));

        // The player's own rig shares its layer; never let it block the stand-up probe.
        int blocking = blockingLayers & ~(1 << gameObject.layer);
        Collider[] hits = Physics.OverlapSphere(probe, radius, blocking, QueryTriggerInteraction.Ignore);
        if (hits.Length == 0) return true;

        Debug.LogWarning($"PlayerCrouch: cannot stand up - blocked by '{hits[0].gameObject.name}'.", this);
        return false;
    }
}
