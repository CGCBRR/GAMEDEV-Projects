using UnityEngine;

namespace SuperSentaiVFX
{
    /// <summary>Expanding, fading floor ring driven through a MaterialPropertyBlock (no material leaks).</summary>
    [RequireComponent(typeof(MeshRenderer))]
    public class PulseRing : MonoBehaviour
    {
        public float startScale = 0.5f;
        public float endScale = 8f;
        public float duration = 2f;
        public float interval = 2.5f;
        public float phase = 0f;
        public Color color = Color.white;
        public AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int ColorId = Shader.PropertyToID("_Color");

        MeshRenderer _renderer;
        MaterialPropertyBlock _block;
        float _time;

        void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
            _block = new MaterialPropertyBlock();
            _time = phase;
        }

        void Update()
        {
            _time += Time.deltaTime;
            float local = Mathf.Repeat(_time, Mathf.Max(0.01f, interval));
            float k = Mathf.Clamp01(local / Mathf.Max(0.01f, duration));
            float scale = Mathf.Lerp(startScale, endScale, Mathf.Pow(k, 0.55f));

            // The quad lies in its local XY plane, so expand X/Y (world XZ once rotated flat).
            transform.localScale = new Vector3(scale, scale, 1f);

            Color c = color;
            c.a *= alphaCurve.Evaluate(k);

            _renderer.GetPropertyBlock(_block);
            _block.SetColor(BaseColorId, c);
            _block.SetColor(ColorId, c);
            _renderer.SetPropertyBlock(_block);
        }
    }
}
