using UnityEngine;

public class TorusKickShockwave : MonoBehaviour
{
    [Header("Kick Trigger")]
    public float kickThreshold = 0.25f;   // trigger when kick crosses this
    public float rearmRatio = 0.5f;       // rearm when kick falls below threshold*rearmRatio

    [Header("Scale (local)")]
    public float startRadiusXZ = 0.2f;    // starting X/Z scale
    public float endRadiusXZ = 8f;        // ending X/Z scale
    public float yScale = 0.15f;          // constant Y scale (thickness/height)

    [Header("Timing")]
    public float expandDuration = 0.35f;  // seconds
    public float fadeDuration = 0.25f;    // seconds
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Render Fade (optional)")]
    public Renderer rend;                 // drag renderer (or leave blank)
    public string alphaParam = "_Alpha";  // requires shader to support it
    public bool disableRendererWhenDone = true;

    bool armed = true;
    float age = 999f;
    float totalDuration;
    float lastKick;

    MaterialPropertyBlock mpb;

    void Awake()
    {
        totalDuration = Mathf.Max(0.01f, expandDuration + fadeDuration);
        mpb = new MaterialPropertyBlock();
        if (rend == null) rend = GetComponentInChildren<Renderer>();

        // start hidden
        if (rend != null) rend.enabled = false;
        transform.localScale = new Vector3(startRadiusXZ, yScale, startRadiusXZ);
    }

    // Klak OSC calls this with 0..1
    public void SetKick(float k)
    {
        k = Mathf.Clamp01(k);
        lastKick = k;

        if (armed && k >= kickThreshold)
        {
            Trigger(k);
            armed = false;
        }
        else if (!armed && k < kickThreshold * rearmRatio)
        {
            armed = true;
        }
    }

    void Trigger(float strength01)
    {
        age = 0f;

        if (rend != null) rend.enabled = true;

        // reset ring small instantly
        transform.localScale = new Vector3(startRadiusXZ, yScale, startRadiusXZ);

        // start fully visible
        SetAlpha(1f);
    }

    void Update()
    {
        if (age > totalDuration) return;

        age += Time.deltaTime;

        // expand 0..1
        float expandT = Mathf.Clamp01(age / Mathf.Max(0.01f, expandDuration));
        float e = ease.Evaluate(expandT);

        float r = Mathf.Lerp(startRadiusXZ, endRadiusXZ, e);
        transform.localScale = new Vector3(r, yScale, r);

        // fade after expansion
        float fadeT = 0f;
        if (age > expandDuration)
            fadeT = Mathf.Clamp01((age - expandDuration) / Mathf.Max(0.01f, fadeDuration));

        float alpha = 1f - fadeT;
        SetAlpha(alpha);

        if (age >= totalDuration)
        {
            if (rend != null && disableRendererWhenDone)
                rend.enabled = false;
        }
    }

    void SetAlpha(float a)
    {
        if (rend == null) return;

        rend.GetPropertyBlock(mpb);
        mpb.SetFloat(alphaParam, a);
        rend.SetPropertyBlock(mpb);
    }
}