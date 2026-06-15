using System.Collections;
using UnityEngine;

/// <summary>
/// Camera follows the player horizontally at all times.
/// Vertically, it waits a moment after the player leaves the screen,
/// pauses the game, then pans to reveal the next area.
/// </summary>
public class CameraMovement : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Horizontal Follow")]
    public bool followX = true;
    public float followSpeed = 8f;

    [Header("Vertical Pan Settings")]
    public float delayBeforePan = 1.5f;
    public float panDistanceY = 3f;
    public float panSpeed = 6f;

    [Header("Edge Padding")]
    public float edgePadding = 0.5f;

    // ── internals ──────────────────────────────────────────────────────────
    private Camera _cam;
    private bool _isPanning = false;
    private bool _waitingToStart = false;
    private Vector3 _panTarget;
    private Coroutine _delayCoroutine;

    // ── Unity lifecycle ────────────────────────────────────────────────────

    void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    void Update()
    {
        if (target == null || _isPanning || _waitingToStart)
            return;

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
        if (target == null)
            return;

        // Continuous horizontal follow
        if (followX)
        {
            Vector3 pos = transform.position;

            pos.x = Mathf.Lerp(
                pos.x,
                target.position.x,
                followSpeed * Time.unscaledDeltaTime
            );

            transform.position = pos;
        }

        // Vertical pan movement
        if (!_isPanning)
            return;

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

        Vector3 camPos = transform.position;
        Vector3 targetPos = target.position;

        float dy = targetPos.y - camPos.y;

        float exitY = 0f;

        if (Mathf.Abs(dy) > halfH)
            exitY = Mathf.Sign(dy);

        return new Vector2(0f, exitY);
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

        Vector3 targetPos = target.position;
        Vector3 camPos = transform.position;

        float destinationY = targetPos.y + exitDir.y * panDistanceY;

        _panTarget = new Vector3(
            transform.position.x,
            destinationY,
            camPos.z
        );

        _waitingToStart = false;
        _isPanning = true;
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
        if (_cam == null)
            _cam = GetComponent<Camera>();

        if (_cam == null || !_cam.orthographic)
            return;

        float halfH = _cam.orthographicSize + edgePadding;
        float halfW = halfH * _cam.aspect + edgePadding;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(halfW * 2f, halfH * 2f, 0f)
        );

        if (_isPanning)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, _panTarget);
            Gizmos.DrawSphere(_panTarget, 0.3f);
        }
    }
#endif
}