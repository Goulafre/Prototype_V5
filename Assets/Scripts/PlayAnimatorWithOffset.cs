using UnityEngine;

public class PlayAnimatorWithOffset : MonoBehaviour
{
    public Animator animator;
    public string stateName = "Base Layer.YourStateName";
    public float startOffsetSeconds = 1.0f;

    void Start()
    {
        if (!animator) animator = GetComponent<Animator>();
        if (!animator) return;

        var clips = animator.runtimeAnimatorController.animationClips;
        if (clips == null || clips.Length == 0) return;

        // Find the first clip length (or the one you care about)
        float clipLen = clips[0].length;
        float normalized = clipLen > 0.0001f ? Mathf.Clamp01(startOffsetSeconds / clipLen) : 0f;

        animator.Play(stateName, 0, normalized);
        animator.Update(0f); // apply immediately
    }
}