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
    [Tooltip("How far the camera pans horizontally.")]
    public float panDistanceX = 10f;
    [Tooltip("How far the camera pans vertically — keep this smaller than X.")]
    public float panDistanceY = 3f;
    public float panSpeed = 6f;
 
    [Header("Edge Padding")]
    public float edgePadding = 0.5f;
 
    // ── internals ──────────────────────────────────────────────────────────
    private Camera    _cam;
    private bool      _isPanning      = false;
    private bool      _waitingToStart = false;
    private Vector3   _panTarget;
    private Coroutine _delayCoroutine;
 
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
            PauseGame();
            _waitingToStart = true;
            _delayCoroutine = StartCoroutine(DelayThenPan(exitDir));
        }
    }
 
    void LateUpdate()
    {
        if (!_isPanning) return;
 
        transform.position = Vector3.MoveTowards(
            transform.position,
            _panTarget,
            panSpeed * Time.unscaledDeltaTime
        );
 
        if (Vector3.Distance(transform.position, _panTarget) < 0.01f)
        {
            transform.position = _panTarget;
            _isPanning = false;
            ResumeGame();
        }
    }
 
    // ── helpers ────────────────────────────────────────────────────────────
 
    Vector2 GetExitDirection()
    {
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
 
    IEnumerator DelayThenPan(Vector2 exitDir)
    {
        float elapsed = 0f;
 
        while (elapsed < delayBeforePan)
        {
            elapsed += Time.unscaledDeltaTime;
 
            if (GetExitDirection() == Vector2.zero)
            {
                _waitingToStart = false;
                ResumeGame();
                yield break;
            }
 
            yield return null;
        }
 
        // Build the pan destination based on where the character actually is
        // so one pan covers the exact distance needed regardless of axis
        Vector3 targetPos  = target.position;
        Vector3 camPos     = transform.position;
 
        float destinationX = camPos.x + exitDir.x * panDistanceX;
 
        float destinationY;
        if (exitDir.y != 0f)
        {
            // Offset a few units in the direction of travel so the character
            // isn't at the very edge of the screen after the pan
            // exitDir.y is -1 (falling down) or +1 (going up)
            destinationY = targetPos.y + exitDir.y * panDistanceY;
        }
        else
        {
            destinationY = camPos.y; // no vertical exit — keep current Y
        }
 
        _panTarget = new Vector3(destinationX, destinationY, camPos.z);
 
        _waitingToStart = false;
        _isPanning      = true;
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
 