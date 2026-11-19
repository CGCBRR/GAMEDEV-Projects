using UnityEngine;

namespace SuperSentaiVFX
{
    /// <summary>Constant local rotation, used for magic circles, energy rings and glyphs.</summary>
    public class Spin : MonoBehaviour
    {
        public Vector3 degreesPerSecond = new Vector3(0f, 0f, 20f);

        void Update()
        {
            transform.Rotate(degreesPerSecond * Time.deltaTime, Space.Self);
        }
    }
}
