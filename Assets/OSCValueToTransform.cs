using UnityEngine;

public class OSCValueToTransform : MonoBehaviour
{
    [Header("Intensity Controls")]
    public float volumeIntensity = 1.5f;
    public float kickIntensity = 0.5f;

    Vector3 originalScale;

    float volumeValue; // 0–1 from TD
    float kickValue;   // 0–1 envelope from TD

    readonly Vector3 rotationSpeed = new Vector3(5f, 8f, 4f);

    void Awake()
    {
        originalScale = transform.localScale;
    }

    // Called from Klak OSC
    public void SetVolume(float v)
    {
        volumeValue = Mathf.Clamp01(v);
    }

    public void SetKick(float k)
    {
        kickValue = Mathf.Clamp01(k);
    }

    void Update()
    {
        // rotation (same as before)
        Vector3 rot = rotationSpeed * Time.deltaTime * (1f + volumeValue);
        transform.Rotate(rot, Space.Self);

        // scale
        float multiplier =
            1f +
            (volumeValue * volumeIntensity) +
            (kickValue * kickIntensity);

        transform.localScale = originalScale * multiplier;
    }
}