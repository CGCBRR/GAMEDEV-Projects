using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Phase 6 run stats. Lives next to the HUD (created automatically at runtime) and
/// samples the player's motion, so the count survives respawns and teleports.
/// The HUD reads steps/distance from here and the spotted/deaths counters from
/// <see cref="PlayerHealth"/>.
/// </summary>
public class StealthStats : MonoBehaviour
{
    [Tooltip("Metres per counted step.")]
    public float strideLength = 0.75f;
    [Tooltip("Movement above this distance between frames is treated as a teleport, not walking.")]
    public float teleportThreshold = 5f;

    [Header("State (written at runtime)")]
    public int steps;
    public float distance;

    static StealthStats _instance;

    public static StealthStats Instance
    {
        get
        {
            if (_instance == null) _instance = FindFirstObjectByType<StealthStats>();
            return _instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        EnsureInstance();
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _instance = null;
        EnsureInstance();
    }

    static void EnsureInstance()
    {
        if (FindFirstObjectByType<StealthStats>() != null) return;
        new GameObject("StealthStats").AddComponent<StealthStats>();
    }

    Transform _player;
    Vector3 _lastPos;

    void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    void Update()
    {
        if (Time.deltaTime <= 0f) return;   // frozen (round won / timed out)

        if (_player == null)
        {
            PlayerHealth hp = StealthPhysics.Player;
            if (hp == null) return;
            _player = hp.transform;
            _lastPos = _player.position;
            return;
        }

        Vector3 pos = _player.position;
        Vector3 delta = pos - _lastPos;
        _lastPos = pos;
        delta.y = 0f;

        float d = delta.magnitude;
        if (d < 0.001f || d > teleportThreshold) return;   // idling or teleported
        distance += d;
        steps = Mathf.FloorToInt(distance / Mathf.Max(0.1f, strideLength));
    }
}
