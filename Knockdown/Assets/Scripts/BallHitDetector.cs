using UnityEngine;

public class BallHitDetector : MonoBehaviour
{
    private bool hasHitStack = false;

    void OnCollisionEnter(Collision col)
    {
        // Unfreeze the moving stack if we hit one of its cylinders
        MovingStack stack = col.gameObject.GetComponentInParent<MovingStack>();
        if (stack != null && !hasHitStack)
        {
            hasHitStack = true;
            stack.Unfreeze();
        }

        // Optional: play sound, spawn particles, etc.
    }
}