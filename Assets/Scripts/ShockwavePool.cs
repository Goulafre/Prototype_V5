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
    public float endRadiusXZ = 15f;
    public float yScale = 0.15f;

    [Header("Timing")]
    public float expandDuration = 0.35f;
    public float fadeDuration = 0.25f;
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Renderer Behaviour")]
    public bool disableRendererWhenDone = true;

    struct RingState
    {
        public float age;
        public bool active;
    }

    RingState[] state;
    Renderer[] renderers;
    int index;
    bool armed = true;

    MaterialPropertyBlock mpb;

    static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        mpb = new MaterialPropertyBlock();

        if (rings == null) rings = new Transform[0];

        state = new RingState[rings.Length];
        renderers = new Renderer[rings.Length];

        for (int i = 0; i < rings.Length; i++)
        {
            if (!rings[i]) continue;

            renderers[i] = rings[i].GetComponentInChildren<Renderer>();
            rings[i].localScale = new Vector3(startRadiusXZ, yScale, startRadiusXZ);

            if (renderers[i] != null)
            {
                // start hidden
                renderers[i].enabled = false;
                SetFade(renderers[i], 0f);
            }

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
            TriggerNext();
            armed = false;
        }
        else if (!armed && k < kickThreshold * rearmRatio)
        {
            armed = true;
        }
    }

    void TriggerNext()
    {
        if (rings.Length == 0) return;

        int i = index;
        index = (index + 1) % rings.Length;

        if (!rings[i]) return;

        state[i].active = true;
        state[i].age = 0f;

        rings[i].localScale = new Vector3(startRadiusXZ, yScale, startRadiusXZ);

        if (renderers[i] != null)
        {
            renderers[i].enabled = true;
            SetFade(renderers[i], 1f);
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

            // Expand
            float expandT = Mathf.Clamp01(age / Mathf.Max(0.01f, expandDuration));
            float e = ease.Evaluate(expandT);
            float r = Mathf.Lerp(startRadiusXZ, endRadiusXZ, e);
            rings[i].localScale = new Vector3(r, yScale, r);

            // Fade after expand
            float fadeT = 0f;
            if (age > expandDuration)
                fadeT = Mathf.Clamp01((age - expandDuration) / Mathf.Max(0.01f, fadeDuration));

            float alpha = 1f - fadeT;

            if (renderers[i] != null)
                SetFade(renderers[i], alpha);

            // Done
            if (age >= total)
            {
                state[i].active = false;

                if (renderers[i] != null)
                {
                    SetFade(renderers[i], 0f);

                    if (disableRendererWhenDone)
                        renderers[i].enabled = false;
                }
            }
        }
    }

    void SetFade(Renderer rend, float a)
    {
        if (rend == null) return;

        rend.GetPropertyBlock(mpb);

        // URP/Lit uses _BaseColor alpha for transparency
        if (rend.sharedMaterial != null && rend.sharedMaterial.HasProperty(BaseColorID))
        {
            Color baseCol = rend.sharedMaterial.GetColor(BaseColorID);
            baseCol.a = a;
            mpb.SetColor(BaseColorID, baseCol);
        }

        // Fade emission too (prevents glowing after alpha fades)
        if (rend.sharedMaterial != null && rend.sharedMaterial.HasProperty(EmissionColorID))
        {
            Color em = rend.sharedMaterial.GetColor(EmissionColorID);
            mpb.SetColor(EmissionColorID, em * a);
        }

        rend.SetPropertyBlock(mpb);
    }
}