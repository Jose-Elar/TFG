using UnityEngine;

public class AutoQuit : MonoBehaviour
{
    [SerializeField] private float delaySeconds = 20f;

    void Start()
    {
        Invoke(nameof(QuitGame), delaySeconds);
    }

    private void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}