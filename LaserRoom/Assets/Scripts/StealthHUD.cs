using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Minimal stealth HUD, created automatically at runtime:
///  - top-right status line: intel count, keycard, flashlight state
///  - short center toast for pickups (see <see cref="ShowToast"/>)
/// </summary>
public class StealthHUD : MonoBehaviour
{
    static string _toast;
    static float _toastUntil;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        EnsureInstance();
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureInstance();
    }

    static void EnsureInstance()
    {
        if (FindFirstObjectByType<StealthHUD>() != null) return;
        new GameObject("StealthHUD").AddComponent<StealthHUD>();
    }

    /// <summary>Shows a message in the middle of the screen for a couple of seconds.</summary>
    public static void ShowToast(string message, float duration = 2.2f)
    {
        _toast = message;
        _toastUntil = Time.unscaledTime + duration;
    }

    void OnGUI()
    {
        DrawStatus();
        DrawToast();
    }

    void DrawStatus()
    {
        const float width = 235f;
        const float height = 58f;
        Rect area = new Rect(Screen.width - width - 12f, 12f, width, height);
        GUI.Box(area, "");

        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            richText = true,
            fontSize = 14,
            alignment = TextAnchor.MiddleCenter
        };
        style.normal.textColor = Color.white;

        GUILayout.BeginArea(new Rect(area.x, area.y + 6f, area.width, area.height - 8f));
        string keycard = HiddenObject.HasKeycard ? "<color=#ffd24a>Keycard OK</color>" : "Keycard --";
        GUILayout.Label($"Intel {HiddenObject.IntelCollected}/{HiddenObject.IntelTotal}   {keycard}", style);
        string flashlight = PlayerFlashlight.IsOn ? "<b>ON</b>" : "off";
        GUILayout.Label($"Flashlight [{flashlight}] - F", style);
        GUILayout.EndArea();
    }

    void DrawToast()
    {
        if (string.IsNullOrEmpty(_toast)) return;
        if (Time.unscaledTime > _toastUntil) return;

        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 26
        };
        style.normal.textColor = new Color(1f, 0.95f, 0.65f);
        GUI.Label(new Rect(0f, Screen.height * 0.28f, Screen.width, 40f), _toast, style);
    }
}
