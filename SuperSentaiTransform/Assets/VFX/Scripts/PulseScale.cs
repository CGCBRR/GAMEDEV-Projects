using UnityEngine;

namespace SuperSentaiVFX
{
    /// <summary>Breathing scale pulse, used for light pillars, cores and glow cards.</summary>
    public class PulseScale : MonoBehaviour
    {
        public Vector3 minScale = Vector3.one;
        public Vector3 maxScale = Vector3.one * 1.08f;
        public float cyclesPerSecond = 0.35f;
        public float phase = 0f;

        void Update()
        {
            float t = Mathf.Sin((Time.time + phase) * cyclesPerSecond * Mathf.PI * 2f) * 0.5f + 0.5f;
            transform.localScale = Vector3.Lerp(minScale, maxScale, t);
        }
    }
}
