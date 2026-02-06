using UnityEngine;

public class OSCValueToTransform : MonoBehaviour
{
    [Header("Incoming Audio (0–1)")]
    [Range(0f, 1f)]
    public float audioValue;

    [Header("Rotation")]
    public Vector3 rotationSpeed = new Vector3(5f, 8f, 4f);

    [Header("Scale (More Reactive)")]
    [Tooltip("Overall amount of scaling from audio.")]
    public float scaleIntensity = 3.2f;

    [Tooltip("Hard cap on how big the object can get (1 = original size).")]
    public float maxScaleMultiplier = 2.3f;

    [Tooltip("Makes peaks hit harder ( > 1 boosts peaks, < 1 boosts quiet parts).")]
    [Range(0.2f, 4f)]
    public float responsePower = 1.7f;

    [Header("Snappy Response (Attack/Release)")]
    [Tooltip("How fast it grows on peaks.")]
    public float attackSpeed = 28f;

    [Tooltip("How fast it shrinks back down.")]
    public float releaseSpeed = 10f;

    [Header("Punch")]
    [Tooltip("Extra pop on fast changes (transients).")]
    public float transientBoost = 1.2f;

    [Tooltip("How much previous audio we remember for transient detection.")]
    public float transientMemory = 18f;

    private Vector3 originalScale;
    private Vector3 currentScale;

    private float lastAudio;
    private float transient;

    void Start()
    {
        originalScale = transform.localScale;
        currentScale = originalScale;

        lastAudio = audioValue;
        transient = 0f;
    }

    // Called by OSC (Dynamic Float)
    public void SetValue(float value)
    {
        audioValue = Mathf.Clamp01(value);
    }

    void Update()
    {
        HandleRotation();
        HandleScale();
    }

    void HandleRotation()
    {
        Vector3 rot = rotationSpeed * Time.deltaTime * (1f + audioValue);
        transform.Rotate(rot, Space.Self);
    }

    void HandleScale()
    {
        // 1) Shape the audio so peaks pop more
        float shaped = Mathf.Pow(audioValue, responsePower);

        // 2) Transient detection: if audio jumps up fast, add extra punch briefly
        float delta = Mathf.Max(0f, shaped - lastAudio);
        transient = Mathf.Lerp(transient, delta, Time.deltaTime * transientMemory);
        float punched = shaped + transient * transientBoost;

        // 3) Build target scale multiplier + clamp
        float rawMultiplier = 1f + (punched * scaleIntensity);
        float clampedMultiplier = Mathf.Min(rawMultiplier, maxScaleMultiplier);

        Vector3 targetScale = originalScale * clampedMultiplier;

        // 4) Snappy attack/release (fast up, slower down)
        float speed = (targetScale.magnitude > currentScale.magnitude) ? attackSpeed : releaseSpeed;

        // Frame-rate independent smoothing that feels very responsive
        float t = 1f - Mathf.Exp(-speed * Time.deltaTime);
        currentScale = Vector3.Lerp(currentScale, targetScale, t);

        transform.localScale = currentScale;

        lastAudio = shaped;
    }
}