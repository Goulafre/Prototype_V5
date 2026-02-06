using UnityEngine;

[ExecuteAlways]
public class AtmosObject : MonoBehaviour
{
    public string objectId;

    [Header("Script-Accessible Variables")]
    public float x;
    public float y;
    public float z;
    public float level;
    public float loudness;
    [Range(0, 1)] public float onsetPulse;

    [Header("Spectral Data")]
    public float band0; public float band1; public float band2; public float band3;
    public float band4; public float band5; public float band6; public float band7;

    public float[] currentBands => new float[] { band0, band1, band2, band3, band4, band5, band6, band7 };

    // Update() removed. Timeline now drives Transform directly.
}