using UnityEngine;

public class ShockwavePoolTrailedUmbrella : MonoBehaviour
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
    public float trailGap = 1.5f;
    [Range(0.1f, 0.95f)] public float trailAlphaFalloff = 0.75f;

    [Header("Umbrella Curve (height over expansion)")]
    [Tooltip("Local Y offset at the start of the wave.")]
    public float startHeight = 1.2f;

    [Tooltip("Local Y offset at the end of the wave (usually below startHeight).")]
    public float endHeight = -0.4f;

    [Tooltip("Controls how the wave drops. Flat early, steep late = umbrella drip.")]
    public AnimationCurve dropCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 0f),
        new Keyframe(0.7f, 0.25f, 0f, 0f),
        new Keyframe(1f, 1f, 0f, 0f)
    );

    [Tooltip("Extra steepness near the end (multiplies the curve effect).")]
    [Range(0.5f, 3f)] public float dropStrength = 1.2f;

    [Header("Renderer Behaviour")]
    public bool disableRendererWhenDone = true;

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

            root.localScale = Vector3.one;
            mainRoot[i] = root;

            // Cache the "spawn" local position so we can offset Y without drifting
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
                Debug.LogWarning($"Umbrella shockwave: No MeshRenderer/MeshFilter found on {root.name}");
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
            root.localPosition = basePos; // reset
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

        // reset
        SetScale(mainRoot[i], startRadiusXZ);
        SetPosY(mainRoot[i], state[i].baseLocalPos, startHeight);

        Enable(mainR[i]);

        int gc = ghostRoot[i]?.Length ?? 0;
        for (int g = 0; g < gc; g++)
        {
            SetScale(ghostRoot[i][g], startRadiusXZ);
            SetPosY(ghostRoot[i][g], state[i].baseLocalPos, startHeight);
            Enable(ghostR[i][g]);
        }
    }

    void Update()
    {
        float lt = Mathf.Max(0.01f, lifetime);

        for (int i = 0; i < rings.Length; i++)
        {
            if (!state[i].active) continue;

            state[i].age += Time.deltaTime;

            float t = Mathf.Clamp01(state[i].age / lt);
            float e = ease.Evaluate(t);

            // Radius
            float rMain = Mathf.Lerp(startRadiusXZ, endRadiusXZ, e);
            SetScale(mainRoot[i], rMain);

            // Umbrella drop: 0..1, slow early, steep late
            float d = Mathf.Clamp01(dropCurve.Evaluate(t) * dropStrength);
            float y = Mathf.Lerp(startHeight, endHeight, d);

            SetPosY(mainRoot[i], state[i].baseLocalPos, y);

            // Fade
            float mainAlpha = 1f - t;
            SetFade(mainR[i], mainAlpha);

            // Ghosts: stay behind in radius AND follow the same drop by their own "effective t"
            int gc = ghostRoot[i]?.Length ?? 0;
            for (int g = 0; g < gc; g++)
            {
                float rg = Mathf.Max(startRadiusXZ, rMain - (g + 1) * trailGap);
                SetScale(ghostRoot[i][g], rg);

                // Optional: make ghosts slightly "older" so they sit lower than main
                float ghostT = Mathf.Clamp01(t + (g + 1) * 0.04f);
                float gd = Mathf.Clamp01(dropCurve.Evaluate(ghostT) * dropStrength);
                float gy = Mathf.Lerp(startHeight, endHeight, gd);

                SetPosY(ghostRoot[i][g], state[i].baseLocalPos, gy);

                float a = mainAlpha * Mathf.Pow(trailAlphaFalloff, g + 1);
                SetFade(ghostR[i][g], a);
            }

            if (t >= 1f)
            {
                state[i].active = false;

                Hide(mainR[i]);

                for (int g = 0; g < gc; g++)
                    Hide(ghostR[i][g]);

                // reset position so it doesn't stay dropped when disabled
                if (mainRoot[i]) mainRoot[i].localPosition = state[i].baseLocalPos;
            }
        }
    }

    void SetScale(Transform root, float r)
    {
        if (!root) return;
        root.localScale = new Vector3(r, yScale, r);
    }

    void SetPosY(Transform root, Vector3 basePos, float localY)
    {
        if (!root) return;
        root.localPosition = new Vector3(basePos.x, basePos.y + localY, basePos.z);
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