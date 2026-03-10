using UnityEngine;

public class IgnoreParentScale : MonoBehaviour
{
    Transform _parent;

    Vector3 _initialLocalScale;
    Vector3 _initialLocalPos;

    void Awake()
    {
        _parent = transform.parent;
        _initialLocalScale = transform.localScale;
        _initialLocalPos = transform.localPosition;
    }

    void LateUpdate()
    {
        if (_parent == null) return;

        Vector3 ps = _parent.localScale;

        // Avoid divide by zero
        float ix = ps.x != 0f ? 1f / ps.x : 1f;
        float iy = ps.y != 0f ? 1f / ps.y : 1f;
        float iz = ps.z != 0f ? 1f / ps.z : 1f;

        // 1) Cancel inherited SCALE (size)
        transform.localScale = new Vector3(
            _initialLocalScale.x * ix,
            _initialLocalScale.y * iy,
            _initialLocalScale.z * iz
        );

        // 2) Cancel scaled POSITION OFFSET (movement distance explosion)
        // Keep whatever Timeline is animating around the initial local position.
        Vector3 animatedOffset = transform.localPosition - _initialLocalPos;

        transform.localPosition = _initialLocalPos + new Vector3(
            animatedOffset.x * ix,
            animatedOffset.y * iy,
            animatedOffset.z * iz
        );
    }
}