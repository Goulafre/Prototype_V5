using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class TimedLoopVideoCard : MonoBehaviour
{
    [Header("References")]
    public VideoPlayer videoPlayer;
    public Renderer targetRenderer; // drag the plane/quad renderer here

    [Header("Timing")]
    public float startDelay = 2f;
    public float loopForSeconds = 10f;   // how long it should keep looping
    public float fadeInTime = 0.5f;
    public float fadeOutTime = 0.5f;

    [Header("Behaviour")]
    public bool useUnscaledTime = false; // use this if timescale might be 0

    Material mat;
    RenderTexture rt;

    void Awake()
    {
        if (!targetRenderer)
            targetRenderer = GetComponentInChildren<Renderer>(true);

        if (!videoPlayer)
            videoPlayer = GetComponent<VideoPlayer>();

        if (!videoPlayer)
        {
            Debug.LogError($"{nameof(TimedLoopVideoCard)}: No VideoPlayer assigned/found.", this);
            enabled = false;
            return;
        }

        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = true;
        videoPlayer.Stop();

        if (targetRenderer)
        {
            mat = targetRenderer.material;
            SetAlpha(0f);
            targetRenderer.enabled = false;
        }

        rt = videoPlayer.targetTexture;
        ClearRT();
    }

    void Start()
    {
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        yield return Wait(startDelay);

        // Prepare first to avoid black flash
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared) yield return null;

        if (targetRenderer) targetRenderer.enabled = true;

        videoPlayer.Play();
        if (fadeInTime > 0f) yield return Fade(0f, 1f, fadeInTime);
        else SetAlpha(1f);

        // Keep looping for a fixed time
        yield return Wait(loopForSeconds);

        if (fadeOutTime > 0f) yield return Fade(1f, 0f, fadeOutTime);
        else SetAlpha(0f);

        // Stop + hide + clear so it becomes truly blank
        videoPlayer.Stop();
        if (targetRenderer) targetRenderer.enabled = false;
        ClearRT();
    }

    IEnumerator Wait(float seconds)
    {
        if (seconds <= 0f) yield break;

        if (useUnscaledTime)
            yield return new WaitForSecondsRealtime(seconds);
        else
            yield return new WaitForSeconds(seconds);
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        if (!mat || duration <= 0f)
        {
            SetAlpha(to);
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
        SetAlpha(to);
    }

    void SetAlpha(float a)
    {
        if (!mat) return;
        var c = mat.color;
        c.a = a;
        mat.color = c;
    }

    void ClearRT()
    {
        if (!rt) return;

        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        GL.Clear(true, true, new Color(0, 0, 0, 0));
        RenderTexture.active = prev;
    }
}