using UnityEngine;

public class FallDetector : MonoBehaviour
{
    [Header("Fall Settings")]
    [SerializeField] private float fallYThreshold = -10f;
    [SerializeField] private Transform respawnPoint;

    [Header("Debug")]
    [SerializeField] private bool logRespawns = true;

    private Rigidbody rb;
    private CharacterController cc;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cc = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (transform.position.y < fallYThreshold)
        {
            Respawn();
        }
    }

    private void Respawn()
    {
        Vector3 targetPos = respawnPoint != null
            ? respawnPoint.position
            : Vector3.zero;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (cc != null)
        {
            cc.enabled = false;
            transform.position = targetPos;
            cc.enabled = true;
        }
        else
        {
            transform.position = targetPos;
        }

        if (logRespawns)
            Debug.Log("Player fell - respawning at start.");
    }
}