using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance;

    [Header("Scan Transition")]
    [SerializeField] private RectTransform scanLine;
    [SerializeField] private Image scanLineImage;
    [SerializeField] private Image fadeOverlay;
    [SerializeField] private float transitionDuration = 0.8f;
    [SerializeField] private float screenHalfWidth = 960f;
    [SerializeField] private float scanLineMaxAlpha = 1f;

    [Header("Opening Fade")]
    [SerializeField] private float openFadeDuration = 0.8f;


    private MainDrone_Actions _globalActions;


  private System.Action<UnityEngine.InputSystem.InputAction.CallbackContext> _quitAction;
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

        SetOverlayAlpha(0f);
        SetScanLineAlpha(0f);
        SetScanLinePosition(-screenHalfWidth);

        SceneManager.sceneLoaded += OnSceneLoaded;

        _quitAction = _ => QuitGame();

        _globalActions = new MainDrone_Actions();
        _globalActions.Enable();
        _globalActions.Drone.Quit.performed += _ => QuitGame();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

    if (_globalActions != null)
    {
        _globalActions.Drone.Quit.performed -= _quitAction;
        _globalActions.Disable();
    }
    }

    private void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "StartingMenu")
        {
            SetOverlayAlpha(0f);
            SetScanLineAlpha(0f);
            SetScanLinePosition(-screenHalfWidth);
            return;
        }

        StartCoroutine(OpenFadeOnly());
    }


    public void LoadScene(string sceneName)
    {
        StartCoroutine(CloseWithScanLine(sceneName));
    }

    private float blackScreenHoldTime = 0.3f;
    // ── Apertura simple: solo fade del overlay, sin línea ──────────
    private IEnumerator OpenFadeOnly()
    {
        // Reposiciona la línea para la próxima vez, sin animarla
        SetScanLinePosition(-screenHalfWidth);
        SetScanLineAlpha(0f);

        // Empieza completamente negro
        SetOverlayAlpha(1f);

        yield return new WaitForSeconds(blackScreenHoldTime);

        float elapsed = 0f;
        while (elapsed < openFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / openFadeDuration;
            SetOverlayAlpha(Mathf.Lerp(1f, 0f, t));
            yield return null;
        }

        SetOverlayAlpha(0f);
    }

    private IEnumerator CloseWithScanLine(string sceneName)
    {
        float elapsed = 0f;
        SetScanLinePosition(-screenHalfWidth);

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;

            float lineX = Mathf.Lerp(-screenHalfWidth, screenHalfWidth, t);
            SetScanLinePosition(lineX);

            SetOverlayAlpha(Mathf.Lerp(0f, 1f, t));
            SetScanLineAlpha(GetLineAlphaCurve(t));

            yield return null;
        }

        SetOverlayAlpha(1f);
        SetScanLineAlpha(0f);

        SceneManager.LoadScene(sceneName);
    }

    private float GetLineAlphaCurve(float t)
    {
        float curve = 1f - Mathf.Abs((t * 2f) - 1f);
        return curve * scanLineMaxAlpha;
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (fadeOverlay == null) return;
        Color c = fadeOverlay.color;
        c.a = alpha;
        fadeOverlay.color = c;
    }

    private void SetScanLineAlpha(float alpha)
    {
        if (scanLineImage == null) return;
        Color c = scanLineImage.color;
        c.a = alpha;
        scanLineImage.color = c;
    }

    private void SetScanLinePosition(float x)
    {
        if (scanLine == null) return;
        scanLine.anchoredPosition = new Vector2(x, scanLine.anchoredPosition.y);
    }
}