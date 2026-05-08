using System.Collections;
using UnityEngine;
 
/// <summary>
/// Retro-style 2D camera that waits a moment after the Soot_Sprite
/// leaves the screen, then pans in the direction it exited.
/// Attach this script to your Main Camera.
/// </summary>
public class CameraMovement : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
 
    [Header("Pan Settings")]
    public float delayBeforePan = 1.5f;
    public float panDistance = 10f;
    public float panSpeed = 6f;
 
    [Header("Edge Padding")]
    public float edgePadding = 0.5f;
 
    // ── internals ──────────────────────────────────────────────────────────
    private Camera      _cam;
    private bool        _isPanning       = false;
    private bool        _waitingToStart  = false;
    private Vector3     _panTarget;          // world-space destination of the camera
    private Coroutine   _delayCoroutine;
 
    // ── Unity lifecycle ────────────────────────────────────────────────────
    void Awake()
    {
        _cam = GetComponent<Camera>();
 
    }
 
    void Update()
    {
        if (target == null || _isPanning || _waitingToStart) return;
 
        Vector2 exitDir = GetExitDirection();
 
        if (exitDir != Vector2.zero)
        {
            // Target just left the screen — freeze the game and start the delay
            PauseGame();
            _waitingToStart = true;
            _delayCoroutine = StartCoroutine(DelayThenPan(exitDir));
        }
    }
 
    void LateUpdate()
    {
        if (!_isPanning) return;
 
        // Camera moves independently of Time.timeScale using unscaled delta time
        transform.position = Vector3.MoveTowards(
            transform.position,
            _panTarget,
            panSpeed * Time.unscaledDeltaTime
        );
 
        // Arrived?
        if (Vector3.Distance(transform.position, _panTarget) < 0.01f)
        {
            transform.position = _panTarget;
            _isPanning = false;
            ResumeGame();
        }
    }
 
    // ── helpers ────────────────────────────────────────────────────────────
 
    /// Returns the 2-D direction in which the target exited the camera view,
    /// or Vector2.zero if the target is still on-screen.
    /// Diagonal exits (e.g. top-right corner) return a diagonal direction.
    Vector2 GetExitDirection()
    {
        // Camera half-extents in world space
        float halfH = _cam.orthographicSize + edgePadding;
        float halfW = halfH * _cam.aspect   + edgePadding;
 
        Vector3 camPos    = transform.position;
        Vector3 targetPos = target.position;
 
        float dx = targetPos.x - camPos.x;
        float dy = targetPos.y - camPos.y;
 
        float exitX = 0f;
        float exitY = 0f;
 
        if (Mathf.Abs(dx) > halfW) exitX = Mathf.Sign(dx);
        if (Mathf.Abs(dy) > halfH) exitY = Mathf.Sign(dy);
 
        return new Vector2(exitX, exitY);
    }
 
    /// Waits <see cref="delayBeforePan"/> seconds, then kicks off the pan.
    /// While waiting, if the target returns to view the pan is cancelled.
    IEnumerator DelayThenPan(Vector2 exitDir)
    {
        float elapsed = 0f;
 
        // Use unscaled time so the delay ticks while the game is paused
        while (elapsed < delayBeforePan)
        {
            elapsed += Time.unscaledDeltaTime;
 
            // If the target came back on-screen during the delay, cancel
            if (GetExitDirection() == Vector2.zero)
            {
                _waitingToStart = false;
                ResumeGame();
                yield break;
            }
 
            yield return null;
        }
 
        // Build the pan destination (keep the Z axis of the camera unchanged)
        _panTarget = transform.position
                   + new Vector3(exitDir.x, exitDir.y, 0f) * panDistance;
 
        _waitingToStart = false;
        _isPanning      = true;
        // Game stays paused — ResumeGame() is called once the pan finishes
    }
 
    // ── pause / resume ─────────────────────────────────────────────────────
 
    void PauseGame()
    {
        Time.timeScale = 0f;
    }
 
    void ResumeGame()
    {
        Time.timeScale = 1f;
        Debug.Log("[CameraMovement] Game resumed.");
    }
 
    // ── debug visualisation ────────────────────────────────────────────────
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (_cam == null) _cam = GetComponent<Camera>();
        if (_cam == null || !_cam.orthographic) return;
 
        float halfH = _cam.orthographicSize + edgePadding;
        float halfW = halfH * _cam.aspect   + edgePadding;
 
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position,
            new Vector3(halfW * 2f, halfH * 2f, 0f));
 
        if (_isPanning)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, _panTarget);
            Gizmos.DrawSphere(_panTarget, 0.3f);
        }
    }
#endif
}