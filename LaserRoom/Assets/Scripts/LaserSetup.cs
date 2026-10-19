using UnityEngine;

public static class LaserSetup
{
    static Material _runtimeRed;

    public static void SetupBeams(Transform root, Material laserMaterial, float beamMargin, float dps, float hit)
    {
        if (root == null) return;
        if (laserMaterial == null)
            laserMaterial = GetRuntimeRed();
        foreach (Transform beam in root.GetComponentsInChildren<Transform>(true))
        {
            if (beam == root) continue;
            if (beam.childCount > 0) continue;
            StripBeamColliders(beam);
            EnsureBeamHazard(beam, beamMargin, dps, hit);
            foreach (Renderer r in beam.GetComponentsInChildren<Renderer>())
                r.sharedMaterial = laserMaterial;
        }
    }

    /// <summary>
    /// Beams are visuals plus query hazards only — colliders would block the player and
    /// would make the beam detectable by raycasts, so any authored ones are removed.
    /// </summary>
    public static void StripBeamColliders(Transform beam)
    {
        foreach (Collider c in beam.GetComponents<Collider>())
            Object.Destroy(c);
    }

    public static void EnsureBeamHazard(Transform beam, float beamMargin, float dps, float hit)
    {
        LaserBeamHazard hazard = beam.GetComponent<LaserBeamHazard>();
        if (hazard == null)
            hazard = beam.gameObject.AddComponent<LaserBeamHazard>();
        hazard.beamMargin = beamMargin;
        hazard.damagePerSecond = dps;
        hazard.hitDamage = hit;
    }

    public static Material GetRuntimeRed()
    {
        if (_runtimeRed != null) return _runtimeRed;
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        if (s == null) s = Shader.Find("Standard");
        _runtimeRed = new Material(s);
        _runtimeRed.name = "LaserRed (Runtime)";
        _runtimeRed.color = Color.red;
        if (_runtimeRed.HasProperty("_BaseColor"))
            _runtimeRed.SetColor("_BaseColor", Color.red);
        if (_runtimeRed.HasProperty("_EmissionColor"))
        {
            _runtimeRed.SetColor("_EmissionColor", Color.red * 2f);
            _runtimeRed.EnableKeyword("_EMISSION");
        }
        return _runtimeRed;
    }
}
