using UnityEngine;

public class CubePianoReactive : MonoBehaviour
{
    [Header("Scale Limits")]
    public float baseScale = 2.6f;      // your cube's idle scale
    public float maxScale = 11f;        // desired peak scale

    [Header("Input Settings")]
    public float inputMax = 0.8f;       // real max coming from TD
    public float responseExponent = 2.5f;  // 1 = linear, 2+ = punchier

    [Header("Rotation (optional)")]
    public Vector3 rotationSpeed = new Vector3(5f, 8f, 4f);

    float volumeValue; // 0–1 from TD (but peaks at 0.8)

    void Awake()
    {
        transform.localScale = Vector3.one * baseScale;
    }

    // Called from Klak OSC
    public void SetCombine(float v)
    {
        volumeValue = Mathf.Clamp01(v);
    }

    void Update()
    {
        // Normalize to real max
        float normalized = Mathf.Clamp01(volumeValue / inputMax);

        // Shape response so peaks explode
        float shaped = Mathf.Pow(normalized, responseExponent);

        // Map to scale range
        float currentScale = Mathf.Lerp(baseScale, maxScale, shaped);

        transform.localScale = Vector3.one * currentScale;

        // Optional constant rotation
        transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
    }
}