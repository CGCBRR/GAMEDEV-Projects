using UnityEngine;

public class MovingStack : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2f;
    public float range = 2f;         // how far it slides each way
    public bool moveOnZ = false;     // false = X axis, true = Z axis

    private Rigidbody[] bodies;
    private Vector3[] initialPositions;
    private float t = 0f;

    void Start()
    {
        bodies = GetComponentsInChildren<Rigidbody>();
        initialPositions = new Vector3[bodies.Length];

        for (int i = 0; i < bodies.Length; i++)
        {
            initialPositions[i] = bodies[i].position;
            bodies[i].isKinematic = true;   // move kinematically until hit
        }
    }

    void FixedUpdate()
    {
        t += Time.fixedDeltaTime * speed;
        float offset = Mathf.Sin(t) * range;
        Vector3 dir = moveOnZ ? Vector3.forward : Vector3.right;

        for (int i = 0; i < bodies.Length; i++)
        {
            if (!bodies[i].isKinematic) continue;   // stop controlling after hit
            Vector3 target = initialPositions[i] + dir * offset;
            bodies[i].MovePosition(target);
            bodies[i].MoveRotation(Quaternion.identity);
        }
    }

    // Called when the ball hits this stack
    public void Unfreeze()
    {
        foreach (var rb in bodies)
            rb.isKinematic = false;
    }
}