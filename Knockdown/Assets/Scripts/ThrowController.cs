using UnityEngine;

public class ThrowController : MonoBehaviour
{
    [Header("References")]
    public Transform holdPoint;         // child of Main Camera, positioned right-hand side
    public Transform cameraTransform;   // Main Camera

    [Header("Ball")]
    public float ballScale = 0.4f;
    public float ballMass = 1f;

    [Header("Throw Settings")]
    public float minThrowForce = 12f;   // was 6, now stronger
    public float maxThrowForce = 45f;   // was 25, now much stronger
    public float chargeTime = 1.0f;     // was 1.2, feels snappier
    public float upwardBoost = 2.5f;

    [Header("Respawn")]
    public float respawnDelay = 0.15f;  // was 1.5, now near-instant

    [Header("Despawn")]
    public float ballLifetime = 6f;     // seconds before thrown ball is destroyed

    private Rigidbody heldBall;
    private float chargeStartTime;
    private bool charging = false;
    private bool hasBall = false;

    void Start()
    {
        if (holdPoint == null)
        {
            Debug.LogError("ThrowController: holdPoint not assigned!");
            return;
        }

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        SpawnBall();
    }

    void Update()
    {
        if (!hasBall) return;

        // Pin ball to hold point while held
        if (!charging && heldBall != null)
        {
            heldBall.transform.position = holdPoint.position;
            heldBall.transform.rotation = holdPoint.rotation;
        }

        if (Input.GetMouseButtonDown(0))
        {
            charging = true;
            chargeStartTime = Time.time;
        }

        if (Input.GetMouseButtonUp(0) && charging)
        {
            charging = false;
            ThrowBall();
        }
    }

    void SpawnBall()
    {
        GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.name = "Ball";
        ball.transform.localScale = Vector3.one * ballScale;
        ball.transform.position = holdPoint.position;

        Rigidbody rb = ball.AddComponent<Rigidbody>();
        rb.mass = ballMass;
        rb.linearDamping = 0.05f;      // less drag → flies farther
        rb.angularDamping = 0.3f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.isKinematic = true;

        ball.AddComponent<BallHitDetector>();

        heldBall = rb;
        hasBall = true;
    }

    void ThrowBall()
    {
        if (heldBall == null) return;

        float charge = Mathf.Clamp01((Time.time - chargeStartTime) / chargeTime);
        float force = Mathf.Lerp(minThrowForce, maxThrowForce, charge);

        // Detach
        heldBall.transform.SetParent(null);
        heldBall.isKinematic = false;

        // Throw direction: camera forward, slight upward arc
        Vector3 dir = cameraTransform.forward + Vector3.up * (upwardBoost / force);
        heldBall.AddForce(dir.normalized * force, ForceMode.Impulse);

        // Add spin
        heldBall.AddTorque(Random.insideUnitSphere * 0.5f, ForceMode.Impulse);

        // Schedule despawn for this thrown ball
        Destroy(heldBall.gameObject, ballLifetime);

        heldBall = null;
        hasBall = false;

        // Fast respawn
        Invoke(nameof(SpawnBall), respawnDelay);
    }
}