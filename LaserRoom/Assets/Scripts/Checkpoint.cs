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
    [Tooltip("Optional: object to activate when stepped on (e.g. the next laser phase, kept inactive until then).")]
    public GameObject activateOnTouch;

    void Awake()
    {
        EnsureTrigger();
    }

    void EnsureTrigger()
    {
        // Keep any existing solid collider (walkable pad) and add a
        // slightly larger trigger zone above it for step detection.
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
        if (!other.CompareTag("Player")) return;
        PlayerHealth hp = other.GetComponent<PlayerHealth>();
        if (hp == null) return;
        hp.SetSpawn(transform.position + Vector3.up * spawnHeight);
        hp.GrantShield();
        if (healOnPickup) hp.Heal(hp.maxHealth);
        if (activateOnTouch != null)
        {
            activateOnTouch.SetActive(true);
            activateOnTouch.SendMessage("Activate", SendMessageOptions.DontRequireReceiver);
        }
        Debug.Log("Checkpoint reached - new spawn set, shield granted.");
    }
}
