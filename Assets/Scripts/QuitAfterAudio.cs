using UnityEngine;

public class QuitAfterAudio : MonoBehaviour
{
    private AudioSource audioSource;
    private bool hasQuit = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!audioSource.isPlaying && !hasQuit)
        {
            hasQuit = true;
            QuitApp();
        }
    }

    void QuitApp()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}