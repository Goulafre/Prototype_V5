using UnityEngine;

public class ShockwavePoolTrailed : MonoBehaviour
{
    [Header("Assign your 12 Torus ROOT transforms")]
    public Transform[] rings;

    [Header("Kick Trigger")]
    public float kickThreshold = 0.25f;
    public float rearmRatio = 0.5f;

    [Header("Shape")]
    public float startRadiusXZ = 0.2f;
    public float endRadiusXZ = 60f;
    public float yScale = 0.015f;

    [Header("Timing")]
    public float lifetime = 1.2f;
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Trail Ghosts")]
    [Range(0, 12)] public int ghostCount = 6;
    [Tooltip("Distance behind the main ring between ghosts.")]
    public float trailGap = 1.5f;

    [Tooltip("Multiplier applied per ghost (0.6 = fades fast, 0.85 = longer trail).")]
    [Range(0.1f, 0.95f)] public float trailAlphaFalloff = 0.75f;

    [Header("Renderer Behaviour")]
    public bool disableRendererWhenDone = true;

    [Header("Bake Playback (Animator drives transforms)")]
    public bool bakePlaybackMode = false;   // turn ON when using Animator clip
    public bool fadeFromScale = true;

    [Header("Playback Visibility Gates")]
    [Tooltip("In bakePlaybackMode: if main ring radius <= startRadiusXZ + this, keep it hidden.")]
    public float startVisibleEpsilon = 0.05f;

    [Tooltip("In bakePlaybackMode: if main ring radius >= endRadiusXZ - this, hide it (finished).")]
    public float endHideEpsilon = 0.10f;

    struct RingState { public float age; public bool active; }
    RingState[] state;

    int index;
    bool armed = true;

    Transform[] mainRoot;
    Renderer[] mainR;

    // ghosts[i][g]
    Transform[][] ghostRoot;
    Renderer[][] ghostR;

    MaterialPropertyBlock mpb;
    static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        mpb = new MaterialPropertyBlock();

        int n = rings != null ? rings.Length : 0;

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

            var mr = root.GetComponent<MeshRenderer>();
            var mf = root.GetComponent<MeshFilter>();

            if (!mr || !mf)
            {
                mr = root.GetComponentInChildren<MeshRenderer>(true);
                mf = mr ? mr.GetComponent<MeshFilter>() : null;
            }

            if (!mr || !mf)
            {
                Debug.LogWarning($"Shockwave: No MeshRenderer/MeshFilter found on {root.name}");
                continue;
            }

            mainR[i] = mr;

            // Create ghosts as render-only clones (no ProBuilder components)
            int gc = Mathf.Max(0, ghostCount);
            ghostRoot[i] = new Transform[gc];
            ghostR[i] = new Renderer[gc];

            for (int g = 0; g < gc; g++)
            {
                ghostRoot[i][g] = CreateClone(root.parent, root, mf.sharedMesh, mr.sharedMaterials,
                    $"{root.name}_Ghost{g + 1}", out ghostR[i][g]);
            }

            Init(root, mainR[i]);

            for (int g = 0; g < gc; g++)
                Init(ghostRoot[i][g], ghostR[i][g]);

            state[i].active = false;
            state[i].age = 999f;
        }

        // Hard hide on startup to prevent “visible before kicks”
        ForceHideAll();
    }

    void OnEnable()
    {
        ForceHideAll();
    }

    void Start()
    {
        // One more pass: some platforms reorder init / animator evaluation
        ForceHideAll();
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

    void Init(Transform root, Renderer r)
    {
        if (root)
            root.localScale = new Vector3(startRadiusXZ, yScale, startRadiusXZ);

        if (r)
        {
            r.enabled = false;
            SetFade(r, 0f);
        }
    }

    void ForceHideAll()
    {
        if (mainRoot == null) return;

        for (int i = 0; i < mainRoot.Length; i++)
        {
            if (mainRoot[i])
                mainRoot[i].localScale = new Vector3(startRadiusXZ, yScale, startRadiusXZ);

            if (mainR[i] != null)
            {
                SetFade(mainR[i], 0f);
                mainR[i].enabled = false;
            }

            int gc = ghostRoot[i]?.Length ?? 0;
            for (int g = 0; g < gc; g++)
            {
                if (ghostRoot[i][g])
                    ghostRoot[i][g].localScale = new Vector3(startRadiusXZ, yScale, startRadiusXZ);

                if (ghostR[i][g] != null)
                {
                    SetFade(ghostR[i][g], 0f);
                    ghostR[i][g].enabled = false;
                }
            }

            if (state != null && i < state.Length)
            {
                state[i].active = false;
                state[i].age = 999f;
            }
        }
    }

    // Klak OSC calls this with 0..1
    public void SetKick(float k)
    {
        // In baked playback we ignore OSC
        if (bakePlaybackMode) return;

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
        if (rings == null || rings.Length == 0) return;

        int i = index;
        index = (index + 1) % rings.Length;

        state[i].active = true;
        state[i].age = 0f;

        SetScale(mainRoot[i], startRadiusXZ);
        Enable(mainR[i]);

        int gc = ghostRoot[i]?.Length ?? 0;
        for (int g = 0; g < gc; g++)
        {
            SetScale(ghostRoot[i][g], startRadiusXZ);
            Enable(ghostR[i][g]);
        }
    }

    void Update()
    {
        // Animator playback: rebuild ghosts + alpha + do visibility gating
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

            float mainAlpha = 1f - t;
            SetFade(mainR[i], mainAlpha);

            int gc = ghostRoot[i]?.Length ?? 0;
            for (int g = 0; g < gc; g++)
            {
                float rg = Mathf.Max(startRadiusXZ, rMain - (g + 1) * trailGap);
                SetScale(ghostRoot[i][g], rg);

                float a = mainAlpha * Mathf.Pow(trailAlphaFalloff, g + 1);
                SetFade(ghostR[i][g], a);
            }

            if (t >= 1f)
            {
                state[i].active = false;

                Hide(mainR[i]);
                for (int g = 0; g < gc; g++)
                    Hide(ghostR[i][g]);
            }
        }
    }

    // Playback mode: main ring scale comes from Animator.
    // We rebuild ghost scales + alpha AND we hide when idle/finished.
    void ApplyPlaybackFromAnimatedMain()
    {
        float denom = Mathf.Max(0.0001f, endRadiusXZ - startRadiusXZ);

        float showThreshold = startRadiusXZ + Mathf.Max(0f, startVisibleEpsilon);
        float hideThreshold = endRadiusXZ - Mathf.Max(0f, endHideEpsilon);

        for (int i = 0; i < mainRoot.Length; i++)
        {
            if (!mainRoot[i] || mainR[i] == null) continue;

            float rMain = mainRoot[i].localScale.x;

            bool idle = rMain <= showThreshold;
            bool finished = rMain >= hideThreshold;

            // If idle or finished: hard hide everything
            if ((idle || finished) && disableRendererWhenDone)
            {
                SetFade(mainR[i], 0f);
                mainR[i].enabled = false;

                int gcHide = ghostRoot[i]?.Length ?? 0;
                for (int g = 0; g < gcHide; g++)
                {
                    if (ghostR[i][g] == null) continue;
                    SetFade(ghostR[i][g], 0f);
                    ghostR[i][g].enabled = false;
                }

                continue;
            }

            // Active: enable & fade
            float t = Mathf.Clamp01((rMain - startRadiusXZ) / denom);
            float mainAlpha = 1f - t;

            mainR[i].enabled = true;
            SetFade(mainR[i], mainAlpha);

            int gc = ghostRoot[i]?.Length ?? 0;
            for (int g = 0; g < gc; g++)
            {
                if (!ghostRoot[i][g] || ghostR[i][g] == null) continue;

                float rg = Mathf.Max(startRadiusXZ, rMain - (g + 1) * trailGap);

                // force scale so they trail during baked playback
                ghostRoot[i][g].localScale = new Vector3(rg, yScale, rg);

                ghostR[i][g].enabled = true;

                float ga = mainAlpha * Mathf.Pow(trailAlphaFalloff, g + 1);
                SetFade(ghostR[i][g], ga);
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

        // Keep original behaviour (this matched your “looked good before” state)
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