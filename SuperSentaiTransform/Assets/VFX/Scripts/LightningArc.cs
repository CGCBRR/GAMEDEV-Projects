using UnityEngine;

namespace SuperSentaiVFX
{
    /// <summary>Jittering, crackling lightning arc rendered with a LineRenderer in local space.</summary>
    [RequireComponent(typeof(LineRenderer))]
    public class LightningArc : MonoBehaviour
    {
        public int segments = 11;
        public float height = 8.5f;
        public float startRadius = 2.1f;
        public float endRadius = 0.8f;
        public float drift = 0.55f;
        public Vector2 rebuildInterval = new Vector2(0.04f, 0.12f);
        [Range(0f, 1f)] public float visibleChance = 0.85f;
        public float twist = 0.45f;

        LineRenderer _line;
        float _timer;

        void Awake()
        {
            _line = GetComponent<LineRenderer>();
            _line.useWorldSpace = false;
            _line.positionCount = segments;
        }

        void Start()
        {
            Rebuild();
            _timer = Random.Range(rebuildInterval.x, rebuildInterval.y);
        }

        void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                Rebuild();
                _timer = Random.Range(rebuildInterval.x, rebuildInterval.y);
            }
        }

        void Rebuild()
        {
            float baseAngle = Random.value * Mathf.PI * 2f;
            _line.enabled = Random.value < visibleChance;

            for (int i = 0; i < segments; i++)
            {
                float t = segments <= 1 ? 0f : i / (float)(segments - 1);
                float angle = baseAngle + t * Mathf.PI * twist + Random.Range(-0.22f, 0.22f);
                float taper = Mathf.Lerp(startRadius, endRadius, t);
                float radius = taper + Random.Range(-drift, drift) * (1f - Mathf.Abs(t - 0.5f) * 0.7f);
                float y = t * height + Random.Range(-0.22f, 0.22f) * (1f - t * 0.35f);

                Vector3 p = new Vector3(Mathf.Cos(angle) * radius, y, Mathf.Sin(angle) * radius);
                _line.SetPosition(i, p);
            }
        }
    }
}
