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

    static Shader FindShader()
    {
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        if (s == null) s = Shader.Find("Standard");
        return s;
    }
}
