using UnityEngine;

public class LightCascadePoolTrailed : MonoBehaviour
{
    [Header("Assign your 12 Torus ROOT transforms (children of LightCascade)")]
    public Transform[] rings;

    [Header("Kick Trigger")]
    public float kickThreshold = 0.25f;
    public float rearmRatio = 0.5f;

    [Header("Shape")]
    public float startRadiusXZ = 0.2f;
    public float endRadiusXZ = 15f;
    [Tooltip("This is the torus thickness scale. Keep it constant. You can set it big if you want.")]
    public float yScale = 10.15f;

    [Header("Timing")]
    public float lifetime = 1.2f;
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Trail Ghosts")]
    [Range(0, 12)] public int ghostCount = 12;

    [Tooltip("Time delay between ghosts in seconds. Try 0.03 to 0.06.")]
    public float trailGap = 0.04f;

    [Tooltip("Multiplier applied per ghost (0.6 = fades fast, 0.85 = longer trail).")]
    [Range(0.1f, 0.95f)] public float trailAlphaFalloff = 0.65f;

    [Header("Cascade (true semicircle)")]
    [Tooltip("Height above the rim in local Y. Your parent is at ~11.54, so 12 is fine.")]
    public float startHeight = 12f;

    [Tooltip("Local Y where the rings must die. Your torus children are at y=0.")]
    public float deathYLocal = 0f;

    [Tooltip("1 = perfect semicircle. <1 hangs higher longer. >1 drops faster.")]
    [Range(0.3f, 2.5f)] public float arcPower = 1f;

    [Header("Renderer Behaviour")]
    public bool disableRendererWhenDone = true;

    [Header("Bake Playback (Animator drives transforms)")]
    public bool bakePlaybackMode = false;
    public bool fadeFromScale = true;

    struct RingState
    {
        public float age;
        public bool active;
        public Vector3 baseLocalPos;
    }

    RingState[] state;

    int index;
    bool armed = true;

    Transform[] mainRoot;
    Renderer[] mainR;

    Transform[][] ghostRoot;
    Renderer[][] ghostR;

    MaterialPropertyBlock mpb;
    static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        mpb = new MaterialPropertyBlock();

        int n = rings.Length;

        state = new RingState[n];
        mainRoot = new Transform[n];
        mainR = new Renderer[n];

        ghostRoot = new Transform[n][];
        ghostR = new Renderer[n][];

        for (int i = 0; i < n; i++)
        {
            var root = rings[i];
            if (!root) continue;

            mainRoot[i] = root;
            state[i].baseLocalPos = root.localPosition;

            var mr = root.GetComponent<MeshRenderer>();
            var mf = root.GetComponent<MeshFilter>();

            if (!mr || !mf)
            {
                mr = root.GetComponentInChildren<MeshRenderer>(true);
                mf = mr ? mr.GetComponent<MeshFilter>() : null;
            }

            if (!mr || !mf)
            {
                Debug.LogWarning($"LightCascade: No MeshRenderer/MeshFilter found on {root.name}");
                continue;
            }

            mainR[i] = mr;

            int gc = Mathf.Max(0, ghostCount);
            ghostRoot[i] = new Transform[gc];
            ghostR[i] = new Renderer[gc];

            for (int g = 0; g < gc; g++)
            {
                ghostRoot[i][g] = CreateClone(root.parent, root, mf.sharedMesh, mr.sharedMaterials,
                    $"{root.name}_Ghost{g + 1}", out ghostR[i][g]);
            }

            Init(root, mainR[i], state[i].baseLocalPos);

            for (int g = 0; g < gc; g++)
                Init(ghostRoot[i][g], ghostR[i][g], state[i].baseLocalPos);

            state[i].active = false;
            state[i].age = 999f;
        }
    }

    Transform CreateClone(Transform parent, Transform main, Mesh mesh, Material[] mats, string name, out Renderer rend)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        go.transform.localPosition = main.localPosition;
        go.transform.localRotation = main.localRotation;
        go.transform.localScale = Vector3.one;

        var mf = go.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;

        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterials = mats;

        rend = mr;
        return go.transform;
    }

    void Init(Transform root, Renderer r, Vector3 basePos)
    {
        if (root)
        {
            root.localScale = new Vector3(startRadiusXZ, yScale, startRadiusXZ);
            root.localPosition = new Vector3(basePos.x, deathYLocal, basePos.z);
        }

        if (r)
        {
            r.enabled = false;
            SetFade(r, 0f);
        }
    }

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
        int i = index;
        index = (index + 1) % rings.Length;

        state[i].active = true;
        state[i].age = 0f;

        SetScale(mainRoot[i], startRadiusXZ);
        SetCascadeYFromRadius(i, mainRoot[i], startRadiusXZ);
        Enable(mainR[i]);

        int gc = ghostRoot[i]?.Length ?? 0;
        for (int g = 0; g < gc; g++)
        {
            SetScale(ghostRoot[i][g], startRadiusXZ);
            SetCascadeYFromRadius(i, ghostRoot[i][g], startRadiusXZ);
            Enable(ghostR[i][g]);
        }
    }

    void Update()
    {
        if (bakePlaybackMode)
        {
            if (fadeFromScale)
                ApplyPlaybackFromAnimatedMain();
            return;
        }

        float lt = Mathf.Max(0.01f, lifetime);

        for (int i = 0; i < rings.Length; i++)
        {
            if (!state[i].active) continue;

            state[i].age += Time.deltaTime;

            float t = Mathf.Clamp01(state[i].age / lt);
            float e = ease.Evaluate(t);

            float rMain = Mathf.Lerp(startRadiusXZ, endRadiusXZ, e);

            SetScale(mainRoot[i], rMain);
            SetCascadeYFromRadius(i, mainRoot[i], rMain);

            float mainAlpha = 1f - t;
            SetFade(mainR[i], mainAlpha);

            // Trail spacing fix:
            // ghosts are the main ring evaluated slightly earlier in time
            int gc = ghostRoot[i]?.Length ?? 0;
            float dt = Mathf.Max(0.001f, trailGap);

            for (int g = 0; g < gc; g++)
            {
                float tg = Mathf.Clamp01(t - (g + 1) * (dt / lt));
                float eg = ease.Evaluate(tg);

                float rg = Mathf.Lerp(startRadiusXZ, endRadiusXZ, eg);

                SetScale(ghostRoot[i][g], rg);
                SetCascadeYFromRadius(i, ghostRoot[i][g], rg);

                float a = (1f - tg) * Mathf.Pow(trailAlphaFalloff, g + 1);
                SetFade(ghostR[i][g], a);
            }

            if (t >= 1f)
            {
                state[i].active = false;

                Hide(mainR[i]);
                for (int g = 0; g < gc; g++)
                    Hide(ghostR[i][g]);

                if (mainRoot[i])
                {
                    var bp = state[i].baseLocalPos;
                    mainRoot[i].localPosition = new Vector3(bp.x, deathYLocal, bp.z);
                }
                for (int g = 0; g < gc; g++)
                {
                    if (!ghostRoot[i][g]) continue;
                    var bp = state[i].baseLocalPos;
                    ghostRoot[i][g].localPosition = new Vector3(bp.x, deathYLocal, bp.z);
                }
            }
        }
    }

    // True semicircle:
    // u = radius progress (0..1)
    // y = startHeight * sqrt(1 - u^2)
    // u=0 => y=startHeight, u=1 => y=0
    void SetCascadeYFromRadius(int i, Transform root, float radiusXZ)
    {
        if (!root) return;

        float u = Mathf.InverseLerp(startRadiusXZ, endRadiusXZ, radiusXZ);
        u = Mathf.Clamp01(u);

        float y01 = Mathf.Sqrt(Mathf.Max(0f, 1f - (u * u)));
        y01 = Mathf.Pow(y01, arcPower);

        float y = deathYLocal + startHeight * y01;

        var bp = state[i].baseLocalPos;
        root.localPosition = new Vector3(bp.x, y, bp.z);
    }

    // Playback mode:
    // Animator drives ONLY main ring scales.
    // We reconstruct ghosts and their alpha from main scale.
    void ApplyPlaybackFromAnimatedMain()
    {
        float denom = Mathf.Max(0.0001f, endRadiusXZ - startRadiusXZ);

        for (int i = 0; i < mainRoot.Length; i++)
        {
            if (!mainRoot[i] || mainR[i] == null) continue;

            float rMain = mainRoot[i].localScale.x;
            float t = Mathf.Clamp01((rMain - startRadiusXZ) / denom);
            float mainAlpha = 1f - t;

            mainR[i].enabled = true;
            SetFade(mainR[i], mainAlpha);
            SetCascadeYFromRadius(i, mainRoot[i], rMain);

            int gc = ghostRoot[i]?.Length ?? 0;
            float dt = Mathf.Max(0.001f, trailGap);

            for (int g = 0; g < gc; g++)
            {
                if (!ghostRoot[i][g] || ghostR[i][g] == null) continue;

                float tg = Mathf.Clamp01(t - (g + 1) * (dt / Mathf.Max(0.01f, lifetime)));
                float eg = ease.Evaluate(tg);

                float rg = Mathf.Lerp(startRadiusXZ, endRadiusXZ, eg);

                ghostRoot[i][g].localScale = new Vector3(rg, yScale, rg);
                SetCascadeYFromRadius(i, ghostRoot[i][g], rg);

                ghostR[i][g].enabled = true;

                float a = (1f - tg) * Mathf.Pow(trailAlphaFalloff, g + 1);
                SetFade(ghostR[i][g], a);
            }
        }
    }

    void SetScale(Transform root, float r)
    {
        if (!root) return;
        root.localScale = new Vector3(r, yScale, r);
    }

    void Enable(Renderer r)
    {
        if (r) r.enabled = true;
    }

    void Hide(Renderer r)
    {
        if (!r) return;

        SetFade(r, 0f);

        if (disableRendererWhenDone)
            r.enabled = false;
    }

    void SetFade(Renderer rend, float a)
    {
        if (!rend) return;

        rend.GetPropertyBlock(mpb);

        if (rend.sharedMaterial != null && rend.sharedMaterial.HasProperty(BaseColorID))
        {
            var c = rend.sharedMaterial.GetColor(BaseColorID);
            c.a = a;
            mpb.SetColor(BaseColorID, c);
        }

        if (rend.sharedMaterial != null && rend.sharedMaterial.HasProperty(EmissionColorID))
        {
            var em = rend.sharedMaterial.GetColor(EmissionColorID);
            mpb.SetColor(EmissionColorID, em * a);
        }

        rend.SetPropertyBlock(mpb);
    }
}