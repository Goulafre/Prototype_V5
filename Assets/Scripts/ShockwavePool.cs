using UnityEngine;

public class ShockwavePool : MonoBehaviour
{
    [Header("Rings (assign 4–6 torus transforms)")]
    public Transform[] rings;

    [Header("Kick Trigger")]
    public float kickThreshold = 0.25f;
    public float rearmRatio = 0.5f;

    [Header("Shape")]
    public float startRadiusXZ = 0.2f;
    public float endRadiusXZ = 8f;
    public float yScale = 0.15f;

    [Header("Timing")]
    public float expandDuration = 0.35f;
    public float fadeDuration = 0.25f;
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Material Fade (optional)")]
    public Renderer[] ringRenderers;      // optional: assign renderers in same order as rings
    public string alphaParam = "_Alpha";  // your shader float
    public bool disableRendererWhenDone = true;

    struct RingState
    {
        public float age;
        public bool active;
    }

    RingState[] state;
    int index;
    bool armed = true;

    MaterialPropertyBlock mpb;

    void Awake()
    {
        mpb = new MaterialPropertyBlock();

        if (rings == null) rings = new Transform[0];
        state = new RingState[rings.Length];

        // If renderers not provided, try get from rings
        if (ringRenderers == null || ringRenderers.Length != rings.Length)
        {
            ringRenderers = new Renderer[rings.Length];
            for (int i = 0; i < rings.Length; i++)
                ringRenderers[i] = rings[i] ? rings[i].GetComponentInChildren<Renderer>() : null;
        }

        // init hidden
        for (int i = 0; i < rings.Length; i++)
        {
            if (!rings[i]) continue;
            rings[i].localScale = new Vector3(startRadiusXZ, yScale, startRadiusXZ);
            if (ringRenderers[i] != null) ringRenderers[i].enabled = false;
            state[i].active = false;
            state[i].age = 999f;
        }
    }

    // Klak OSC calls this with 0..1
    public void SetKick(float k)
    {
        k = Mathf.Clamp01(k);

        if (armed && k >= kickThreshold)
        {
            TriggerNext(k);
            armed = false;
        }
        else if (!armed && k < kickThreshold * rearmRatio)
        {
            armed = true;
        }
    }

    void TriggerNext(float strength01)
    {
        if (rings.Length == 0) return;

        int i = index;
        index = (index + 1) % rings.Length;

        if (!rings[i]) return;

        state[i].active = true;
        state[i].age = 0f;

        rings[i].localScale = new Vector3(startRadiusXZ, yScale, startRadiusXZ);

        if (ringRenderers[i] != null)
        {
            ringRenderers[i].enabled = true;
            SetAlpha(i, 1f);
        }
    }

    void Update()
    {
        float total = Mathf.Max(0.01f, expandDuration + fadeDuration);

        for (int i = 0; i < rings.Length; i++)
        {
            if (!state[i].active || !rings[i]) continue;

            state[i].age += Time.deltaTime;

            float age = state[i].age;

            // expand
            float expandT = Mathf.Clamp01(age / Mathf.Max(0.01f, expandDuration));
            float e = ease.Evaluate(expandT);
            float r = Mathf.Lerp(startRadiusXZ, endRadiusXZ, e);
            rings[i].localScale = new Vector3(r, yScale, r);

            // fade after expand
            float fadeT = 0f;
            if (age > expandDuration)
                fadeT = Mathf.Clamp01((age - expandDuration) / Mathf.Max(0.01f, fadeDuration));

            float alpha = 1f - fadeT;
            SetAlpha(i, alpha);

            if (age >= total)
            {
                state[i].active = false;
                if (ringRenderers[i] != null && disableRendererWhenDone)
                    ringRenderers[i].enabled = false;
            }
        }
    }

    void SetAlpha(int i, float a)
    {
        var rend = ringRenderers[i];
        if (rend == null) return;

        rend.GetPropertyBlock(mpb);
        mpb.SetFloat(alphaParam, a);
        rend.SetPropertyBlock(mpb);
    }
}