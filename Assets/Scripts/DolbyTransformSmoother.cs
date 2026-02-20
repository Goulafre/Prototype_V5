using UnityEngine;

/// <summary>
/// Smooths an animated Transform (position + rotation) to reduce jitter.
/// Put this on a separate GameObject and assign the animated object as Target.
/// </summary>
public class DolbyTransformSmoother : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Smoothing (seconds)")]
    [Tooltip("Bigger = smoother but more lag. Typical: 0.03 - 0.15")]
    public float positionSmoothTime = 0.06f;

    [Tooltip("Bigger = smoother but more lag. Typical: 0.03 - 0.15")]
    public float rotationSmoothTime = 0.06f;

    [Header("Jitter Gate")]
    [Tooltip("If the animation only moves by less than this (meters), ignore it (helps micro-jitter).")]
    public float deadZoneMeters = 0.0005f;

    [Tooltip("If the rotation change is less than this (degrees), ignore it (helps micro-jitter).")]
    public float deadZoneDegrees = 0.05f;

    [Header("Time Source")]
    [Tooltip("Use unscaled time (true) if your timeline/animation is independent of Time.timeScale.")]
    public bool useUnscaledTime = false;

    Vector3 _posVelocity;
    Quaternion _smoothedRot;
    Vector3 _smoothedPos;

    Vector3 _lastRawPos;
    Quaternion _lastRawRot;
    bool _initialized;

    void Reset()
    {
        // Helpful default if you drop it onto an object accidentally.
        target = transform;
    }

    // Run AFTER animation has applied transforms.
    void LateUpdate()
    {
        if (!target) return;

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        if (dt <= 0f) return;

        Vector3 rawPos = target.position;
        Quaternion rawRot = target.rotation;

        if (!_initialized)
        {
            _initialized = true;
            _smoothedPos = rawPos;
            _smoothedRot = rawRot;
            _lastRawPos = rawPos;
            _lastRawRot = rawRot;
            _posVelocity = Vector3.zero;
            return;
        }

        // --- Dead-zone gating (kills tiny flutter) ---
        Vector3 gatedPos = rawPos;
        if ((rawPos - _lastRawPos).sqrMagnitude < deadZoneMeters * deadZoneMeters)
            gatedPos = _lastRawPos;

        Quaternion gatedRot = rawRot;
        if (Quaternion.Angle(rawRot, _lastRawRot) < deadZoneDegrees)
            gatedRot = _lastRawRot;

        // --- Smooth position using SmoothDamp (nice for jitter) ---
        _smoothedPos = Vector3.SmoothDamp(
            _smoothedPos,
            gatedPos,
            ref _posVelocity,
            Mathf.Max(0.0001f, positionSmoothTime),
            Mathf.Infinity,
            dt
        );

        // --- Smooth rotation with exponential damping ---
        // Convert "seconds" feel into a stable lerp factor.
        float rotT = 1f - Mathf.Exp(-dt / Mathf.Max(0.0001f, rotationSmoothTime));
        _smoothedRot = Quaternion.Slerp(_smoothedRot, gatedRot, rotT);

        // Apply back to the animated object.
        target.SetPositionAndRotation(_smoothedPos, _smoothedRot);

        _lastRawPos = gatedPos;
        _lastRawRot = gatedRot;
    }
}