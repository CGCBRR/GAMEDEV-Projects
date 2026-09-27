using UnityEngine;

public class RotationTrigger : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private RotatablePlatform targetPlatform;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (targetPlatform != null)
        {
            targetPlatform.RotateStep();
            Debug.Log($"Rotating {targetPlatform.name} by 45 degrees");
        }
        else
        {
            Debug.LogWarning($"{name}: No target platform assigned!");
        }
    }
}