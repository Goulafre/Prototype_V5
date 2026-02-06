using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class RefreshReflectionAfterSkyboxVideo : MonoBehaviour
{
    public ReflectionProbe probe;
    public VideoPlayer skyboxPlayer;

    IEnumerator Start()
    {
        yield return null; // let RTs allocate

        // Ensure the player is actually running
        if (!skyboxPlayer.isPrepared) skyboxPlayer.Prepare();
        while (!skyboxPlayer.isPrepared) yield return null;

        skyboxPlayer.Play();

        // Wait until it has a valid texture/frame
        while (skyboxPlayer.texture == null) yield return null;

        // One extra frame so the skybox shader sees it
        yield return null;

        probe.RenderProbe();
    }
}