using StarterAssets;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Double-tap sprint: two quick forward taps (W) latch sprint on while the player
/// keeps holding forward. Releasing forward (or stopping) drops back to walking.
/// Holding the sprint key (Left Shift / gamepad trigger) still works on its own.
/// </summary>
[RequireComponent(typeof(StarterAssetsInputs))]
public class DoubleTapSprint : MonoBehaviour
{
    [Header("Gesture")]
    [Tooltip("Two forward taps closer together than this (seconds) count as a double-tap.")]
    public float doubleTapWindow = 0.35f;
    [Tooltip("Forward input needed for a tap to count (W = 1 on the move axis; diagonals are ~0.7).")]
    public float forwardThreshold = 0.5f;

    [Header("State (written at runtime)")]
    [Tooltip("True while the double-tap latch is holding sprint on.")]
    public bool latched;

    StarterAssetsInputs _input;
#if ENABLE_INPUT_SYSTEM
    InputAction _sprintAction;
#endif
    bool _forwardLastFrame;
    float _lastTapTime = -999f;

    void Awake()
    {
        _input = GetComponent<StarterAssetsInputs>();
    }

    void Update()
    {
        if (Time.deltaTime <= 0f) return;   // frozen (round won / timed out)

        bool forward = _input != null && _input.move.y > forwardThreshold;
        if (forward && !_forwardLastFrame)
        {
            if (Time.time - _lastTapTime <= doubleTapWindow) latched = true;
            _lastTapTime = Time.time;
        }
        _forwardLastFrame = forward;

        // Let go of forward and the sprint drops: you must commit while it is latched.
        if (!forward) latched = false;

        bool want = SprintKeyHeld() || latched;
        if (_input != null && _input.sprint != want)
            _input.sprint = want;
    }

    bool SprintKeyHeld()
    {
#if ENABLE_INPUT_SYSTEM
        if (_sprintAction == null)
        {
            PlayerInput playerInput = GetComponent<PlayerInput>();
            if (playerInput != null && playerInput.actions != null)
                _sprintAction = playerInput.actions.FindAction("Sprint", false);
        }
        return _sprintAction != null && _sprintAction.ReadValue<float>() > 0.5f;
#else
        return false;
#endif
    }

    /// <summary>Debug hook: pretend W was double-tapped (used by the editor test menu).</summary>
    public void DebugBeginSprintRun()
    {
        if (_input == null) return;
        _input.MoveInput(Vector2.up);
        _forwardLastFrame = true;
        latched = true;
    }

    /// <summary>Debug hook: end the scripted sprint run.</summary>
    public void DebugEndSprintRun()
    {
        if (_input == null) return;
        _input.MoveInput(Vector2.zero);
        _forwardLastFrame = false;
        latched = false;
    }
}
