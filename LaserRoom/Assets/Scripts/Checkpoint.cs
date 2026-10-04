using UnityEngine;

/// <summary>
/// Put on Checkpoint 1. When the player steps on it, it becomes the new
/// respawn point and grants the shield buff (blocks exactly one laser hit).
/// Stepping on it again refreshes the shield.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    [Tooltip("Height above the pad the player respawns at.")]
    public float spawnHeight = 1f;
    [Tooltip("If true, stepping on the checkpoint also refills health.")]
    public bool healOnPickup = false;
    [Tooltip("If true, stepping on the checkpoint grants the shield buff (blocks exactly one laser hit). Turn off for the Start Checkpoint.")]
    public bool grantShield = true;
    [Tooltip("Optional: object to activate when stepped on (e.g. the next laser phase, kept inactive until then).")]
    public GameObject activateOnTouch;

    void Awake()
    {
        EnsureTrigger();
    }

    void EnsureTrigger()
    {
        // Manual-collider workflow: preserve the user's BoxCollider trigger.
        // Keep any existing solid collider (walkable pad) and only add a
        // trigger zone if none exists.
        BoxCollider manualBox = GetComponent<BoxCollider>();
        if (manualBox != null)
        {
            manualBox.isTrigger = true;
            EnsureMinTriggerHeight(manualBox);
            return;
        }
        foreach (Collider c in GetComponents<Collider>())
            if (c.isTrigger) return;

        Renderer r = GetComponentInChildren<Renderer>();
        BoxCollider box = gameObject.AddComponent<BoxCollider>();
        box.isTrigger = true;
        if (r != null)
        {
            Vector3 worldSize = r.bounds.size + new Vector3(0.6f, 1.4f, 0.6f);
            Vector3 lossy = transform.lossyScale;
            box.size = new Vector3(
                lossy.x != 0 ? worldSize.x / Mathf.Abs(lossy.x) : worldSize.x,
                lossy.y != 0 ? worldSize.y / Mathf.Abs(lossy.y) : worldSize.y,
                lossy.z != 0 ? worldSize.z / Mathf.Abs(lossy.z) : worldSize.z);
            box.center = transform.InverseTransformPoint(r.bounds.center + Vector3.up * 0.6f);
        }
        else
        {
            box.size = new Vector3(2f, 2f, 2f);
            box.center = Vector3.up * 0.5f;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        TryGrant(other, true);
    }

    void OnTriggerStay(Collider other)
    {
        // Fallback for missed Enter (thin trigger graze, fast move,
        // or player starting inside the zone). Grant is idempotent.
        TryGrant(other, false);
    }

    bool TryGrant(Collider other, bool log)
    {
        if (other == null) return false;
        PlayerHealth hp = other.GetComponent<PlayerHealth>();
        if (hp == null) hp = other.GetComponentInParent<PlayerHealth>();
        if (hp == null) return false;
        if (!other.CompareTag("Player") && !hp.CompareTag("Player")) return false;
        hp.SetSpawn(transform.position + Vector3.up * spawnHeight);
        if (grantShield) hp.GrantShield();
        if (healOnPickup) hp.Heal(hp.maxHealth);
        if (activateOnTouch != null)
        {
            activateOnTouch.SetActive(true);
            activateOnTouch.SendMessage("Activate", SendMessageOptions.DontRequireReceiver);
        }
        if (log)
            Debug.Log("Checkpoint reached - new spawn set" + (grantShield ? ", shield granted." : "."));
        return true;
    }

    /// <summary>Manual triggers copied from the thin pad are too flat for the capsule to overlap. Grow upward to a 2m zone.</summary>
    void EnsureMinTriggerHeight(BoxCollider box)
    {
        float scaleY = Mathf.Abs(transform.lossyScale.y);
        if (scaleY < 0.0001f) scaleY = 1f;
        float worldHeight = box.size.y * scaleY;
        const float minHeight = 1.5f;
        if (worldHeight >= minHeight) return;
        float oldSizeY = box.size.y;
        float newSizeY = 2f / scaleY;
        Vector3 size = box.size;
        size.y = newSizeY;
        box.size = size;
        Vector3 center = box.center;
        center.y += (newSizeY - oldSizeY) * 0.5f; // grow upward, keep bottom
        box.center = center;
        Debug.LogWarning($"{gameObject.name} trigger was too flat ({worldHeight:F2}m) for the player capsule - expanded to 2m tall. Your XZ size was kept.", this);
    }
}
