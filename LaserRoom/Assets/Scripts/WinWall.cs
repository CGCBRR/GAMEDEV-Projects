using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class WinWall : MonoBehaviour
{
    [Header("Popup text")]
    public string winTitle = "GAME COMPLETE!";
    public string winSubtitle = "You escaped the Laser Room!";

    [Header("Popup layout")]
    [Tooltip("Popup box size in pixels.")]
    public Vector2 popupSize = new Vector2(520f, 340f);
    public int titleFontSize = 48;
    public int bodyFontSize = 24;
    public int buttonFontSize = 28;

    bool _won;
    float _winTime;
    bool _cursorWasLocked;

    void Awake()
    {
        Collider c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        TryWin(other);
    }

    void OnTriggerStay(Collider other)
    {
        TryWin(other);
    }

    void TryWin(Collider other)
    {
        if (_won) return;
        if (ResolvePlayer(other) == null) return;
        _won = true;
        if (GameTimer.Instance != null && GameTimer.Instance.HasStarted)
            _winTime = GameTimer.Instance.Elapsed;
        else
            _winTime = Time.timeSinceLevelLoad;
        if (GameTimer.Instance != null) GameTimer.Instance.OnWin();
        _cursorWasLocked = Cursor.lockState == CursorLockMode.Locked;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Win Wall touched - game complete!");
    }

    void OnGUI()
    {
        if (!_won) return;

        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float x = (Screen.width - popupSize.x) * 0.5f;
        float y = (Screen.height - popupSize.y) * 0.5f;
        GUILayout.BeginArea(new Rect(x, y, popupSize.x, popupSize.y));
        GUI.Box(new Rect(0f, 0f, popupSize.x, popupSize.y), "");

        GUIStyle title = new GUIStyle(GUI.skin.label);
        title.alignment = TextAnchor.MiddleCenter;
        title.fontSize = titleFontSize;
        title.normal.textColor = Color.green;
        GUILayout.Label(winTitle, title);

        GUIStyle body = new GUIStyle(GUI.skin.label);
        body.alignment = TextAnchor.MiddleCenter;
        body.fontSize = bodyFontSize;
        body.normal.textColor = Color.white;
        GUILayout.Label(winSubtitle, body);
        GUILayout.Label(FormatTime(_winTime), body);
        GUILayout.Space(16f);

        GUIStyle button = new GUIStyle(GUI.skin.button);
        button.fontSize = buttonFontSize;
        if (GUILayout.Button("Play Again", button, GUILayout.Height(64f)))
        {
            Time.timeScale = 1f;
            if (_cursorWasLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        GUILayout.EndArea();
    }

    static string FormatTime(float t)
    {
        int m = (int)(t / 60f);
        float s = t - m * 60f;
        return $"Time: {m:00}:{s:00.0}";
    }

    static PlayerHealth ResolvePlayer(Collider other)
    {
        if (other == null) return null;
        PlayerHealth hp = other.GetComponent<PlayerHealth>();
        if (hp == null) hp = other.GetComponentInParent<PlayerHealth>();
        if (hp == null) return null;
        if (!other.CompareTag("Player") && !hp.CompareTag("Player")) return null;
        return hp;
    }
}
