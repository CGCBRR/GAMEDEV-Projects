using UnityEngine;
using UnityEngine.SceneManagement;

public class GameTimer : MonoBehaviour
{
    public static GameTimer Instance { get; private set; }

    [Header("Timer")]
    [Tooltip("Seconds on the clock when the Start Checkpoint is touched.")]
    public float duration = 40f;

    [Header("HUD label")]
    public int timerFontSize = 40;
    [Tooltip("Label turns red under this many seconds.")]
    public float dangerBelow = 10f;

    [Header("Timeout popup")]
    public string timeoutTitle = "TIME'S UP!";
    public string timeoutSubtitle = "Timer Run Out";
    public Vector2 popupSize = new Vector2(520f, 340f);
    public int titleFontSize = 48;
    public int bodyFontSize = 24;
    public int buttonFontSize = 28;

    float _timeLeft;
    float _elapsed;
    bool _running;
    bool _won;
    bool _timedOut;
    bool _cursorWasLocked;

    public float Elapsed => _elapsed;
    public float TimeLeft => _timeLeft;
    public bool HasStarted { get; private set; }

    void Awake()
    {
        Instance = this;
        _timeLeft = duration;
        _elapsed = 0f;
    }

    public void StartTimer()
    {
        if (_won) return;
        _timeLeft = duration;
        _elapsed = 0f;
        _running = true;
        _timedOut = false;
        HasStarted = true;
    }

    public void OnWin()
    {
        _won = true;
        _running = false;
    }

    void Update()
    {
        if (!_running || _won) return;
        _timeLeft -= Time.deltaTime;
        _elapsed += Time.deltaTime;
        if (_timeLeft <= 0f)
        {
            _timeLeft = 0f;
            _running = false;
            Timeout();
        }
    }

    void Timeout()
    {
        if (_won || _timedOut) return;
        _timedOut = true;
        _cursorWasLocked = Cursor.lockState == CursorLockMode.Locked;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Timer ran out - game over!");
    }

    void OnGUI()
    {
        if (_running || _won || _timedOut)
            DrawTimerLabel();
        if (_timedOut)
            DrawTimeoutPopup();
    }

    void DrawTimerLabel()
    {
        GUIStyle label = new GUIStyle(GUI.skin.label);
        label.alignment = TextAnchor.UpperCenter;
        label.fontSize = timerFontSize;
        label.normal.textColor = (_running && _timeLeft <= dangerBelow) ? Color.red : Color.white;
        GUI.Label(new Rect(0f, 12f, Screen.width, 70f), FormatClock(_timeLeft), label);
    }

    void DrawTimeoutPopup()
    {
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
        title.normal.textColor = Color.red;
        GUILayout.Label(timeoutTitle, title);

        GUIStyle body = new GUIStyle(GUI.skin.label);
        body.alignment = TextAnchor.MiddleCenter;
        body.fontSize = bodyFontSize;
        body.normal.textColor = Color.white;
        GUILayout.Label(timeoutSubtitle, body);
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

    static string FormatClock(float t)
    {
        int m = (int)(t / 60f);
        int s = Mathf.CeilToInt(t - m * 60f);
        if (s >= 60) { m++; s -= 60; }
        return $"{m}:{s:00}";
    }
}
