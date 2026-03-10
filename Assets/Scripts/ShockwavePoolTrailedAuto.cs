using UnityEngine;

public class ShockwavePoolTrailedAuto : MonoBehaviour
{
    [Header("Rings (assign your 12 Torus transforms)")]
    public Transform[] rings;

    [Header("Kick Trigger")]
    public float kickThreshold = 0.25f;
    public float rearmRatio = 0.5f;

    [Header("Shape")]
    public float startRadiusXZ = 0.2f;
    public float endRadiusXZ = 50f;
    public float yScale = 0.02f;

    [Header("Timing")]
    public float lifetime = 1.1f;
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Trail (2 copies behind the main)")]
    public float trailGap = 1.0f;
    [Range(0f, 1f)] public float trail1Alpha = 0.45f;
    [Range(0f, 1f)] public float trail2Alpha = 0.22f;

    [Header("Renderer Behaviour")]
    public bool disableRendererWhenDone = true;

    struct RingState { public float age; public bool active; }
    RingState[] state;

    int index;
    bool armed = true;

    Renderer[] rMain, rT1, rT2;
    Transform[] tMain, tT1, tT2;

    MaterialPropertyBlock mpb;
    static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        mpb = new MaterialPropertyBlock();

        if (rings == null) rings = new Transform[0];

        state = new RingState[rings.Length];
        rMain = new Renderer[rings.Length];
        rT1   = new Renderer[rings.Length];
        rT2   = new Renderer[rings.Length];

        tMain = new Transform[rings.Length];
        tT1   = new Transform[rings.Length];
        tT2   = new Transform[rings.Length];

        for (int i = 0; i < rings.Length; i++)
        {
            var root = rings[i];
            if (!root) continue;

            root.localScale = Vector3.one;

            // Find the first renderer under the torus
            var mainR = root.GetComponentInChildren<Renderer>(true);
            if (!mainR)
            {
                Debug.LogWarning($"ShockwavePoolTrailedAuto: No Renderer found under ring {root.name}");
                continue;
            }

            rMain[i] = mainR;
            tMain[i] = mainR.transform;

            // Clone Trail1 and Trail2 from the main renderer object
            var trail1GO = Instantiate(mainR.gameObject, root);
            trail1GO.name = "Trail1";
            var trail2GO = Instantiate(mainR.gameObject, root);
            trail2GO.name = "Trail2";

            // Match pose to main renderer
            trail1GO.transform.localPosition = mainR.transform.localPosition;
            trail1GO.transform.localRotation = mainR.transform.localRotation;
            trail1GO.transform.localScale = Vector3.one;

            trail2GO.transform.localPosition = mainR.transform.localPosition;
            trail2GO.transform.localRotation = mainR.transform.localRotation;
            trail2GO.transform.localScale = Vector3.one;

            rT1[i] = trail1GO.GetComponent<Renderer>();
            rT2[i] = trail2GO.GetComponent<Renderer>();

            tT1[i] = trail1GO.transform;
            tT2[i] = trail2GO.transform;

            // Start hidden
            InitOne(rMain[i], tMain[i]);
            InitOne(rT1[i],   tT1[i]);
            InitOne(rT2[i],   tT2[i]);

            state[i].active = false;
            state[i].age = 999f;
        }
    }

    void InitOne(Renderer r, Transform t)
    {
        if (t) t.localScale = new Vector3(startRadiusXZ, yScale, startRadiusXZ);
        if (r)
        {
            r.enabled = false;
            SetFade(r, 0f);
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

        state[i].active = true;
        state[i].age = 0f;

        SetRingScale(tMain[i], startRadiusXZ);
        SetRingScale(tT1[i], startRadiusXZ);
        SetRingScale(tT2[i], startRadiusXZ);

        Enable(rMain[i]);
        Enable(rT1[i]);
        Enable(rT2[i]);

        SetFade(rMain[i], 1f);
        SetFade(rT1[i], 0f);
        SetFade(rT2[i], 0f);
    }

    void Update()
    {
        float lt = Mathf.Max(0.01f, lifetime);

        for (int i = 0; i < rings.Length; i++)
        {
            if (!state[i].active) continue;

            state[i].age += Time.deltaTime;
            float age = state[i].age;

            float t = Mathf.Clamp01(age / lt);
            float e = ease.Evaluate(t);

            float r = Mathf.Lerp(startRadiusXZ, endRadiusXZ, e);

            float r1 = Mathf.Max(startRadiusXZ, r - trailGap);
            float r2 = Mathf.Max(startRadiusXZ, r - trailGap * 2f);

            SetRingScale(tMain[i], r);
            SetRingScale(tT1[i], r1);
            SetRingScale(tT2[i], r2);

            float a = 1f - t;
            SetFade(rMain[i], a);
            SetFade(rT1[i], a * trail1Alpha);
            SetFade(rT2[i], a * trail2Alpha);

            if (age >= lt)
            {
                state[i].active = false;
                Hide(rMain[i]);
                Hide(rT1[i]);
                Hide(rT2[i]);
            }
        }
    }

    void SetRingScale(Transform t, float radiusXZ)
    {
        if (!t) return;
        t.localScale = new Vector3(radiusXZ, yScale, radiusXZ);
    }

    void Enable(Renderer r)
    {
        if (!r) return;
        r.enabled = true;
    }

    void Hide(Renderer r)
    {
        if (!r) return;
        SetFade(r, 0f);
        if (disableRendererWhenDone) r.enabled = false;
    }

    void SetFade(Renderer rend, float a)
    {
        if (!rend) return;

        rend.GetPropertyBlock(mpb);

        if (rend.sharedMaterial != null && rend.sharedMaterial.HasProperty(BaseColorID))
        {
            Color baseCol = rend.sharedMaterial.GetColor(BaseColorID);
            baseCol.a = a;
            mpb.SetColor(BaseColorID, baseCol);
        }

        if (rend.sharedMaterial != null && rend.sharedMaterial.HasProperty(EmissionColorID))
        {
            Color em = rend.sharedMaterial.GetColor(EmissionColorID);
            mpb.SetColor(EmissionColorID, em * a);
        }

        rend.SetPropertyBlock(mpb);
    }
}