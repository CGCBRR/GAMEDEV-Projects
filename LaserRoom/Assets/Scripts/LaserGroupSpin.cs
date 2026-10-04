using UnityEngine;

public class LaserGroupSpin : MonoBehaviour
{
    [Header("Spin (local X axis, around beam center)")]
    [Tooltip("Degrees per second around the local X axis through the beams' center. Negative spins the other way. 0 = off.")]
    public float spinSpeed = 45f;

    Transform[] _children;
    Vector3[] _startPos;
    Quaternion[] _startRot;
    Vector3 _center;
    float _angle;

    void Awake()
    {
        System.Collections.Generic.List<Transform> list = new System.Collections.Generic.List<Transform>();
        foreach (Transform child in transform)
            list.Add(child);
        _children = list.ToArray();
        _startPos = new Vector3[_children.Length];
        _startRot = new Quaternion[_children.Length];
        _center = Vector3.zero;
        for (int i = 0; i < _children.Length; i++)
        {
            _startPos[i] = _children[i].localPosition;
            _startRot[i] = _children[i].localRotation;
            _center += _startPos[i];
        }
        if (_children.Length > 0)
            _center /= _children.Length;
    }

    void Update()
    {
        if (_children == null || _children.Length == 0) return;
        if (spinSpeed == 0f) return;
        _angle += spinSpeed * Time.deltaTime;
        Quaternion q = Quaternion.AngleAxis(_angle, Vector3.right);
        for (int i = 0; i < _children.Length; i++)
        {
            if (_children[i] == null) continue;
            _children[i].localPosition = _center + q * (_startPos[i] - _center);
            _children[i].localRotation = q * _startRot[i];
        }
    }
}
