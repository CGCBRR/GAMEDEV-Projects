using UnityEngine;

public class RotatablePlatform : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationStep = 45f;
    [SerializeField] private float rotationSpeed = 180f;

    private Quaternion targetRotation;

    private void Awake()
    {
        targetRotation = transform.rotation;
    }

    private void Update()
    {
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    public void RotateStep()
    {
        targetRotation *= Quaternion.Euler(0f, rotationStep, 0f);
    }
}