using UnityEngine;

namespace SuperSentaiVFX
{
    /// <summary>Builds a glowing helix with a LineRenderer; pair with Spin for a rotating ribbon.</summary>
    [RequireComponent(typeof(LineRenderer))]
    public class SpiralRibbon : MonoBehaviour
    {
        public int segments = 96;
        public float radius = 2.2f;
        public float height = 6.5f;
        public float turns = 1.6f;
        public float phase = 0f;
        [Range(0f, 1f)] public float taper = 0.3f;

        void Start()
        {
            Build();
        }

        void Build()
        {
            var line = GetComponent<LineRenderer>();
            line.positionCount = segments;
            line.loop = false;

            for (int i = 0; i < segments; i++)
            {
                float t = segments <= 1 ? 0f : i / (float)(segments - 1);
                float angle = phase + t * Mathf.PI * 2f * turns;
                float r = radius * (1f - taper * t);
                float y = height * t;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle) * r, y, Mathf.Sin(angle) * r));
            }
        }
    }
}
