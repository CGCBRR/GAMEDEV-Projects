using UnityEngine;

/// <summary>
/// Shared setup for laser-trap phases (used by LaserGridController and Phase2Controller):
/// red material and per-beam LaserDamage. BoxCollider triggers are manual
/// (set in the editor) and preserved — this script no longer rebuilds them.
/// </summary>
public static class LaserSetup
{
    static Material _runtimeRed;

    /// <summary>Every beam under root (recursive) gets damage + red material. Manual BoxCollider triggers are left untouched.</summary>
    public static void SetupBeams(Transform root, Material laserMaterial, float triggerMargin, float dps, float hit)
    {
        if (root == null) return;
        if (laserMaterial == null)
            laserMaterial = GetRuntimeRed();
        foreach (Transform beam in root.GetComponentsInChildren<Transform>(true))
        {
            if (beam == root) continue;
            if (beam.childCount > 0) continue; // groups move as units; only leaf beams get triggers
            EnsureBeamTrigger(beam, triggerMargin);
            EnsureBeamDamage(beam, dps, hit);
            foreach (Renderer r in beam.GetComponentsInChildren<Renderer>())
                r.sharedMaterial = laserMaterial;
        }
    }

    public static void EnsureBeamTrigger(Transform laser, float margin)
    {
        // Manual-collider workflow: preserve the user's BoxCollider setup.
        // Only remove blocking MeshColliders (concave triggers unsupported,
        // solid beams would physically block the player) and force
        // remaining colliders to be triggers. Creates a BoxCollider only
        // if the beam has none (e.g. a newly added laser was missed).
        BoxCollider existingBox = laser.GetComponent<BoxCollider>();
        foreach (MeshCollider mc in laser.GetComponents<MeshCollider>())
            Object.Destroy(mc);
        if (existingBox != null)
        {
            existingBox.isTrigger = true;
            return;
        }
        foreach (Collider c in laser.GetComponents<Collider>())
        {
            if (c is MeshCollider) continue; // pending destroy above
            c.isTrigger = true;
            return;
        }
        MeshFilter mf = laser.GetComponent<MeshFilter>();
        BoxCollider box = laser.gameObject.AddComponent<BoxCollider>();
        box.isTrigger = true;
        if (mf != null && mf.sharedMesh != null)
        {
            // mesh-local bounds go through the exact same transform as the
            // visible mesh, so rotation and (parent) scale always match
            box.size = mf.sharedMesh.bounds.size * margin;
            box.center = mf.sharedMesh.bounds.center;
        }
        else
        {
            box.size = new Vector3(3f, 3f, 0.2f);
        }
    }

    public static void EnsureBeamDamage(Transform laser, float dps, float hit)
    {
        LaserDamage dmg = laser.GetComponent<LaserDamage>();
        if (dmg == null)
        {
            dmg = laser.gameObject.AddComponent<LaserDamage>();
            dmg.damagePerSecond = dps;
            dmg.hitDamage = hit;
        }
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
