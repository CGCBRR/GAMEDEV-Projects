using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum HiddenKind
{
    Intel,
    Keycard
}

/// <summary>
/// A collectible that stays invisible (and cannot be picked up) until the player's
/// flashlight cone lights it up. The flashlight drives <see cref="SetLit"/>; walking
/// into a lit object collects it.
///
/// Statics keep the run's score: <see cref="IntelCollected"/> / <see cref="IntelTotal"/>
/// and <see cref="HasKeycard"/>. They reset when a new scene is loaded (Play Again).
/// </summary>
public class HiddenObject : MonoBehaviour
{
    /// <summary>Everything currently hidden, so the flashlight never needs Find calls.</summary>
    public static readonly List<HiddenObject> All = new List<HiddenObject>();

    public static int IntelCollected { get; private set; }
    public static int IntelTotal { get; private set; }
    public static bool HasKeycard { get; private set; }

    static HiddenObject()
    {
        SceneManager.sceneUnloaded += _ => ResetStatics();
    }

    [Header("Type")]
    public HiddenKind kind = HiddenKind.Intel;

    [Header("State (written at runtime)")]
    public bool lit;
    public bool collected;

    [Header("Pickup")]
    public float pickupRadius = 1.6f;

    [Header("Visuals")]
    public float spinSpeed = 70f;

    static Material _intelMaterial;
    static Material _keycardMaterial;

    Renderer[] _renderers;

    void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>(true);
        Material material = kind == HiddenKind.Keycard ? GetKeycardMaterial() : GetIntelMaterial();
        foreach (Renderer r in _renderers)
            r.sharedMaterial = material;
        EnsureTriggerCollider();
        SetVisible(false);
    }

    void OnEnable()
    {
        if (!All.Contains(this)) All.Add(this);
        if (kind == HiddenKind.Intel) IntelTotal++;
    }

    void Update()
    {
        if (collected) return;
        if (spinSpeed != 0f)
            transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
        if (lit && StealthPhysics.SphereHitsPlayer(transform.position, pickupRadius))
            Collect();
    }

    /// <summary>Flashlight hook: visible while the beam is on this object, hidden again when it leaves.</summary>
    public void SetLit(bool value)
    {
        if (collected || lit == value) return;
        lit = value;
        SetVisible(value);
    }

    void Collect()
    {
        collected = true;
        lit = false;
        SetVisible(false);
        All.Remove(this);

        if (kind == HiddenKind.Keycard)
        {
            HasKeycard = true;
            StealthHUD.ShowToast("KEYCARD FOUND!");
            Debug.Log("Keycard collected - the Win Wall will now open.");
        }
        else
        {
            IntelCollected++;
            StealthHUD.ShowToast($"Intel collected - {IntelCollected}/{IntelTotal}");
            Debug.Log($"Intel collected ({IntelCollected}/{IntelTotal}).");
        }

        gameObject.SetActive(false);
    }

    void SetVisible(bool visible)
    {
        if (_renderers == null) return;
        foreach (Renderer r in _renderers)
            if (r != null) r.enabled = visible;
    }

    /// <summary>The object is never solid - it only leaves a trigger-sized presence.</summary>
    void EnsureTriggerCollider()
    {
        Collider c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    static Material GetIntelMaterial()
    {
        if (_intelMaterial == null)
            _intelMaterial = MakeEmissive(new Color(0.15f, 0.95f, 1f), "IntelCyan (Runtime)");
        return _intelMaterial;
    }

    static Material GetKeycardMaterial()
    {
        if (_keycardMaterial == null)
            _keycardMaterial = MakeEmissive(new Color(1f, 0.8f, 0.1f), "KeycardGold (Runtime)");
        return _keycardMaterial;
    }

    static Material MakeEmissive(Color color, string name)
    {
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        if (s == null) s = Shader.Find("Standard");
        Material m = new Material(s);
        m.name = name;
        m.color = color;
        if (m.HasProperty("_BaseColor"))
            m.SetColor("_BaseColor", color);
        if (m.HasProperty("_EmissionColor"))
        {
            m.SetColor("_EmissionColor", color * 3f);
            m.EnableKeyword("_EMISSION");
        }
        return m;
    }

    static void ResetStatics()
    {
        All.Clear();
        IntelCollected = 0;
        IntelTotal = 0;
        HasKeycard = false;
    }
}
