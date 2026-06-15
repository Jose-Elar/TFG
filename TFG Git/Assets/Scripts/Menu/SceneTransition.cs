using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance;

    [SerializeField] private Image circleImage;
    [SerializeField] private float transitionDuration = 0.8f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Start fully transparent — no opening animation on main menu
        circleImage.fillAmount = 0f;

        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Don't open on the main menu scene
        if (scene.name == "StartingMenu") return;

        StartCoroutine(OpenTransition());
    }

    // ── Call this to transition to a new scene ────────────────────
    public void LoadScene(string sceneName)
    {
        StartCoroutine(TransitionRoutine(sceneName));
    }

    // ── Opens the circle on scene start ──────────────────────────
    private IEnumerator OpenTransition()
    {
        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            circleImage.fillAmount = Mathf.Lerp(1f, 0f, elapsed / transitionDuration);
            yield return null;
        }
        circleImage.fillAmount = 0f;
    }

    // ── Closes then loads scene ───────────────────────────────────
    private IEnumerator TransitionRoutine(string sceneName)
    {
        // Close circle
        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            circleImage.fillAmount = Mathf.Lerp(0f, 1f, elapsed / transitionDuration);
            yield return null;
        }
        circleImage.fillAmount = 1f;

        SceneManager.LoadScene(sceneName);
    }
}