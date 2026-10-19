using StarterAssets;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Double-tap sprint: two quick taps of a movement key (W/A/S/D) latch sprint on
/// while the player keeps moving. Releasing the movement keys (or stopping) drops
/// back to walking. Holding the sprint key (Left Shift / gamepad trigger) still
/// works on its own.
/// </summary>
[RequireComponent(typeof(StarterAssetsInputs))]
public class DoubleTapSprint : MonoBehaviour
{
    [Header("Gesture")]
    [Tooltip("Two taps of the same movement key closer together than this (seconds) count as a double-tap.")]
    public float doubleTapWindow = 0.35f;
    [Tooltip("Direction input needed for a tap to count (full press = 1; diagonals are ~0.7).")]
    public float tapThreshold = 0.5f;

    [Header("State (written at runtime)")]
    [Tooltip("True while the double-tap latch is holding sprint on.")]
    public bool latched;

    StarterAssetsInputs _input;
#if ENABLE_INPUT_SYSTEM
    InputAction _sprintAction;
#endif
    bool _fwdLast, _backLast, _leftLast, _rightLast;
    float _fwdTap = -999f, _backTap = -999f, _leftTap = -999f, _rightTap = -999f;

    void Awake()
    {
        _input = GetComponent<StarterAssetsInputs>();
    }

    void Update()
    {
        if (Time.deltaTime <= 0f) return;   // frozen (round won / timed out)

        Vector2 move = _input != null ? _input.move : Vector2.zero;
        bool fwd = move.y > tapThreshold;
        bool back = move.y < -tapThreshold;
        bool left = move.x < -tapThreshold;
        bool right = move.x > tapThreshold;

        if (Tap(fwd, ref _fwdLast, ref _fwdTap)) latched = true;
        if (Tap(back, ref _backLast, ref _backTap)) latched = true;
        if (Tap(left, ref _leftLast, ref _leftTap)) latched = true;
        if (Tap(right, ref _rightLast, ref _rightTap)) latched = true;

        // Let go of the movement keys and the sprint drops: you must commit while it is latched.
        if (!(fwd || back || left || right)) latched = false;

        bool want = SprintKeyHeld() || latched;
        if (_input != null && _input.sprint != want)
            _input.sprint = want;
    }

    /// <summary>True on the frame a direction key goes down; a second tap inside the window returns true.</summary>
    bool Tap(bool down, ref bool lastFrame, ref float lastTapTime)
    {
        bool tap = down && !lastFrame;
        lastFrame = down;
        if (!tap) return false;

        bool isSecondTap = Time.time - lastTapTime <= doubleTapWindow;
        lastTapTime = Time.time;
        return isSecondTap;
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

    /// <summary>Debug hook: script a real double-tap gesture in a direction and keep holding it (editor test menu).</summary>
    public void DebugDoubleTapRun(Vector2 dir)
    {
        if (_input == null) return;
        StopAllCoroutines();
        StartCoroutine(FakeDoubleTap(dir));
    }

    System.Collections.IEnumerator FakeDoubleTap(Vector2 dir)
    {
        _input.MoveInput(dir);            // first tap
        yield return null;
        _input.MoveInput(Vector2.zero);   // release between taps
        yield return null;
        _input.MoveInput(dir);            // second tap: Update latches here, direction stays held
    }

    /// <summary>Debug hook: end the scripted sprint run.</summary>
    public void DebugEndSprintRun()
    {
        if (_input == null) return;
        StopAllCoroutines();
        _input.MoveInput(Vector2.zero);
        _fwdLast = false;
        _backLast = false;
        _leftLast = false;
        _rightLast = false;
        latched = false;
    }
}
