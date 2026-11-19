using UnityEngine;

namespace SuperSentaiVFX
{
    /// <summary>Organic light flicker for point lights.</summary>
    [RequireComponent(typeof(Light))]
    public class LightPulse : MonoBehaviour
    {
        public float baseIntensity = 6f;
        public float amplitude = 0.45f;
        public float speed = 0.9f;
        public float phase = 0f;

        Light _light;

        void Awake()
        {
            _light = GetComponent<Light>();
        }

        void Update()
        {
            float n = Mathf.PerlinNoise(Time.time * speed + phase, 0.37f);
            _light.intensity = baseIntensity * (1f - amplitude * 0.5f + n * amplitude);
        }
    }
}
