using UnityEngine;

/// <summary>
/// Small runtime-material helpers shared by the stealth hazards.
///
/// Hazards build their own visuals at runtime (so the scene only ever holds a
/// placeholder object plus the behaviour script). These helpers keep the material
/// setup - URP shader lookup, fallback, HDR emission - in one place.
/// </summary>
public static class StealthVisuals
{
    /// <summary>Plain lit material (URP/Lit with a Standard fallback).</summary>
    public static Material MakeLit(Color color, float metallic, float smoothness, string name)
    {
        Material m = new Material(FindShader());
        m.name = name;
        m.color = color;
        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smoothness);
        return m;
    }

    /// <summary>Emissive material: glows on its own (post-processing bloom picks it up).</summary>
    public static Material MakeEmissive(Color color, float intensity, string name)
    {
        Material m = new Material(FindShader());
        m.name = name;
        m.color = color;
        if (m.HasProperty("_EmissionColor"))
        {
            m.SetColor("_EmissionColor", color * Mathf.Max(0f, intensity));
            m.EnableKeyword("_EMISSION");
        }
        return m;
    }

    /// <summary>Re-applies a colour (and HDR emission level) to an emissive material.</summary>
    public static void SetEmissive(Material material, Color color, float intensity)
    {
        if (material == null) return;
        material.color = color;
        if (material.HasProperty("_EmissionColor"))
            material.SetColor("_EmissionColor", color * Mathf.Max(0f, intensity));
    }

    /// <summary>Transparent emissive material for scanner curtains (URP Lit, alpha-blended).</summary>
    public static Material MakeCurtain(Color color, float intensity, string name)
    {
        Material m = new Material(FindShader());
        m.name = name;
        m.SetOverrideTag("RenderType", "Transparent");
        if (m.HasProperty("_Surface")) m.SetFloat("_Surface", 1f);      // 1 = transparent
        if (m.HasProperty("_Blend")) m.SetFloat("_Blend", 0f);          // alpha blend
        if (m.HasProperty("_SrcBlend")) m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        if (m.HasProperty("_DstBlend")) m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        if (m.HasProperty("_ZWrite")) m.SetInt("_ZWrite", 0);
        if (m.HasProperty("_AlphaClip")) m.SetFloat("_AlphaClip", 0f);
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.DisableKeyword("_ALPHATEST_ON");
        m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        Color c = color;
        c.a = 0.3f;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        m.color = c;
        if (m.HasProperty("_EmissionColor"))
        {
            m.SetColor("_EmissionColor", color * Mathf.Max(0f, intensity));
            m.EnableKeyword("_EMISSION");
        }
        return m;
    }

    static Shader FindShader()
    {
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        if (s == null) s = Shader.Find("Standard");
        return s;
    }
}
