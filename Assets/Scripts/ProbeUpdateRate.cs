using UnityEngine;

[RequireComponent(typeof(ReflectionProbe))]
public class ProbeUpdateRate : MonoBehaviour
{
    public float updatesPerSecond = 1f; // Quest-friendly: 1–4
    private ReflectionProbe probe;
    private float nextTime;

    void Awake()
    {
        probe = GetComponent<ReflectionProbe>();
        probe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.ViaScripting;
    }

    void Update()
    {
        if (Time.time >= nextTime)
        {
            probe.RenderProbe();
            nextTime = Time.time + 1f / Mathf.Max(0.1f, updatesPerSecond);
        }
    }
}