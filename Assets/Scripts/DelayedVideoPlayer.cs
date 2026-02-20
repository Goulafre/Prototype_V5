using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class DelayedVideoPlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public float startDelay = 2f;

    [Header("Fade Settings")]
    public float fadeInTime = 1f;
    public float fadeOutTime = 1f;

    Renderer rend;
    Material mat;
    RenderTexture rt;

    void Awake()
    {
        rend = GetComponent<Renderer>();

        if (rend)
        {
            mat = rend.material;
            SetAlpha(0f);
            rend.enabled = false;
        }

        if (!videoPlayer) return;

        videoPlayer.playOnAwake = false;
        videoPlayer.Stop();

        rt = videoPlayer.targetTexture;
        if (rt != null)
        {
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            GL.Clear(true, true, new Color(0, 0, 0, 0));
            RenderTexture.active = prev;
        }
    }

    void Start()
    {
        if (videoPlayer != null)
            StartCoroutine(PrepareThenPlay());
    }

    IEnumerator PrepareThenPlay()
    {
        yield return new WaitForSeconds(startDelay);

        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
            yield return null;

        rend.enabled = true;

        videoPlayer.Play();

        yield return StartCoroutine(Fade(0f, 1f, fadeInTime));

        // Wait until near end of video
        while (videoPlayer.isPlaying &&
               videoPlayer.time < videoPlayer.length - fadeOutTime)
        {
            yield return null;
        }

        yield return StartCoroutine(Fade(1f, 0f, fadeOutTime));

        rend.enabled = false;
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / duration);
            SetAlpha(a);
            yield return null;
        }

        SetAlpha(to);
    }

    void SetAlpha(float a)
    {
        if (!mat) return;

        Color c = mat.color;
        c.a = a;
        mat.color = c;
    }
}