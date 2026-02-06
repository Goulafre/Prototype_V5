using UnityEngine;

public class HapRecorderFix : MonoBehaviour
{
    public RenderTexture hapSource;
    public RenderTexture captureRT;

    void OnPreRender()
    {
        if (hapSource && captureRT)
            Graphics.Blit(hapSource, captureRT);
    }
}