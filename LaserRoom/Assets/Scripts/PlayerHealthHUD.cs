using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthHUD : MonoBehaviour
{
    [Header("Bar layout")]
    public Vector2 barSize = new Vector2(320f, 32f);
    public Vector2 screenPadding = new Vector2(20f, 20f);
    [Tooltip("The bar is always this color (solid red by default).")]
    public Color barColor = Color.red;

    [Header("Number beside the bar")]
    public bool showNumber = true;
    public int fontSize = 28;
    public Color numberColor = Color.white;
    [Tooltip("Gap between the bar and the number.")]
    public float numberGap = 12f;

    [Header("Shield icon (buff)")]
    [Tooltip("Shown next to the number while the shield buff is active. Breaks on one laser hit.")]
    public Color shieldColor = new Color(0.3f, 0.85f, 1f, 1f);
    public Vector2 shieldSize = new Vector2(26f, 26f);

    PlayerHealth _health;
    Image _fill;
    Text _number;
    Image _shieldIcon;

    void Awake()
    {
        _health = FindFirstObjectByType<PlayerHealth>();
        if (_health == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) _health = player.GetComponent<PlayerHealth>();
        }
        BuildUI();
    }

    void Update()
    {
        if (_health == null || _fill == null) return;
        float frac = _health.maxHealth > 0f
            ? Mathf.Clamp01(_health.currentHealth / _health.maxHealth)
            : 0f;
        _fill.fillAmount = frac;
        _fill.color = barColor;
        if (_number != null)
        {
            _number.text = Mathf.CeilToInt(Mathf.Max(0f, _health.currentHealth)).ToString();
            _number.color = numberColor;
        }
        if (_shieldIcon != null)
            _shieldIcon.enabled = _health.hasShield;
    }

    void BuildUI()
    {
        GameObject canvasGo = new GameObject("HealthCanvas");
        canvasGo.transform.SetParent(transform, false);
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasGo.AddComponent<GraphicRaycaster>();

        GameObject bgGo = new GameObject("HealthBarBG");
        bgGo.transform.SetParent(canvasGo.transform, false);
        Image bg = bgGo.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.65f);
        RectTransform bgRect = bg.rectTransform;
        bgRect.anchorMin = new Vector2(0f, 1f);
        bgRect.anchorMax = new Vector2(0f, 1f);
        bgRect.pivot = new Vector2(0f, 1f);
        bgRect.sizeDelta = barSize;
        bgRect.anchoredPosition = new Vector2(screenPadding.x, -screenPadding.y);

        GameObject fillGo = new GameObject("HealthFill");
        fillGo.transform.SetParent(bgGo.transform, false);
        _fill = fillGo.AddComponent<Image>();
        _fill.type = Image.Type.Filled;
        _fill.fillMethod = Image.FillMethod.Horizontal;
        _fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        _fill.color = barColor;
        RectTransform fillRect = _fill.rectTransform;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.offsetMin = new Vector2(4f, 4f);
        fillRect.offsetMax = new Vector2(-4f, -4f);
        _fill.fillAmount = 1f;
        _fill.raycastTarget = false;
        bg.raycastTarget = false;

        if (!showNumber) return;

        GameObject numGo = new GameObject("HealthNumber");
        numGo.transform.SetParent(canvasGo.transform, false);
        _number = numGo.AddComponent<Text>();
        _number.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _number.fontSize = fontSize;
        _number.alignment = TextAnchor.MiddleLeft;
        _number.color = numberColor;
        _number.raycastTarget = false;
        _number.text = "100";
        RectTransform numRect = _number.rectTransform;
        numRect.anchorMin = new Vector2(0f, 1f);
        numRect.anchorMax = new Vector2(0f, 1f);
        numRect.pivot = new Vector2(0f, 0.5f);
        numRect.sizeDelta = new Vector2(140f, barSize.y);
        numRect.anchoredPosition = new Vector2(
            screenPadding.x + barSize.x + numberGap,
            -screenPadding.y - barSize.y * 0.5f);

        GameObject shieldGo = new GameObject("ShieldIcon");
        shieldGo.transform.SetParent(canvasGo.transform, false);
        _shieldIcon = shieldGo.AddComponent<Image>();
        _shieldIcon.color = shieldColor;
        _shieldIcon.raycastTarget = false;
        _shieldIcon.enabled = false;
        RectTransform shieldRect = _shieldIcon.rectTransform;
        shieldRect.anchorMin = new Vector2(0f, 1f);
        shieldRect.anchorMax = new Vector2(0f, 1f);
        shieldRect.pivot = new Vector2(0f, 0.5f);
        shieldRect.sizeDelta = shieldSize;
        shieldRect.anchoredPosition = new Vector2(
            screenPadding.x + barSize.x + numberGap + 140f + numberGap,
            -screenPadding.y - barSize.y * 0.5f);
        shieldRect.localRotation = Quaternion.Euler(0f, 0f, 45f);
    }
}
