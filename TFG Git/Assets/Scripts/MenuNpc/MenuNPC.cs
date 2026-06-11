using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuNPC : MonoBehaviour
{
    [Header("Wander Zone")]
    [SerializeField] private BoxCollider2D wanderZone;
    [SerializeField] private float wanderSpeed = 1.5f;
    [SerializeField] private float wanderWaitMin = 1f;
    [SerializeField] private float wanderWaitMax = 3f;

    [Header("Jump")]
    [SerializeField] private Transform cliffEdge;
    [SerializeField] private float walkToEdgeSpeed = 2f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravityScale = 3f;

    [Header("Transition")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private float offScreenDelay = 0.5f;
    [SerializeField] private CanvasGroup fadeCanvas;

    [Header("Animations")]
    private static readonly string ANIM_WALKING = "isWalking";
    private static readonly string ANIM_JUMPING = "isJumping";

    private Rigidbody2D    _rb;
    private SpriteRenderer _sprite;
    private Animator       _animator;

    private enum NPCState { Wandering, WalkingToEdge, Jumping, OffScreen }
    private NPCState _state = NPCState.Wandering;

    private enum WanderState { Waiting, Moving }
    private WanderState _wanderState    = WanderState.Waiting;
    private float       _wanderTimer    = 0f;
    private int         _wanderDirection = 1;
    private Vector3     _wanderTarget;

    void Awake()
    {
        _rb       = GetComponent<Rigidbody2D>();
        _sprite   = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        _rb.freezeRotation = true;
        _rb.gravityScale   = 0f;

        _wanderTimer = Random.Range(wanderWaitMin, wanderWaitMax);
    }

    void Update()
    {
        switch (_state)
        {
            case NPCState.Wandering:
                WanderBehaviour();
                break;

            case NPCState.WalkingToEdge:
                WalkToEdge();
                break;

            case NPCState.Jumping:
                CheckOffScreen();
                break;
        }
    }

    // ── Called by the Start button ────────────────────────────────
    public void OnStartPressed()
    {
        Debug.Log("[MenuNPC] Start button pressed, transitioning NPC.");
        if (_state != NPCState.Wandering) return;
        _state = NPCState.WalkingToEdge;
        _rb.linearVelocity = Vector2.zero;
        _animator.SetBool(ANIM_WALKING, false);
    }

    // ── Walk to cliff edge ────────────────────────────────────────

private void WalkToEdge()
{
    if (cliffEdge == null) return;

    Vector2 direction = ((Vector2)cliffEdge.position - (Vector2)transform.position).normalized;

    _sprite.flipX = direction.x < 0;
    _animator.SetBool(ANIM_WALKING, true);

    float newX = Mathf.MoveTowards(
        transform.position.x,
        cliffEdge.position.x,
        walkToEdgeSpeed * Time.deltaTime
    );
    transform.position = new Vector3(newX, transform.position.y, transform.position.z);

    // Calculate distance AFTER moving                    ← key fix
    float distance = Vector2.Distance(transform.position, cliffEdge.position);

    if (distance <= 0.5f)
    {
        Debug.Log("[MenuNPC] Starting jump routine.");
        _rb.linearVelocity = Vector2.zero;
        _animator.SetBool(ANIM_WALKING, false);
        StartCoroutine(JumpRoutine());
    }
}
    // ── Jump off cliff ────────────────────────────────────────────
private IEnumerator JumpRoutine()
{
    Debug.Log("[MenuNPC] Jump routine started.");
    _state = NPCState.Jumping;

    // Small pause at edge for drama
    yield return new WaitForSeconds(1f);

    // Switch to dynamic FIRST then wait one physics frame
    _rb.bodyType = RigidbodyType2D.Dynamic;
    _rb.gravityScale = gravityScale;

    yield return new WaitForFixedUpdate(); // ← wait for physics to register the body type change

    // Now apply velocity
    _rb.linearVelocity = new Vector2(walkToEdgeSpeed, jumpForce);

    _animator.SetBool(ANIM_JUMPING, true);
}

    // ── Check if off screen then transition ──────────────────────
    private void CheckOffScreen()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float halfW        = cam.orthographicSize * cam.aspect;
        float screenBottom = cam.transform.position.y - cam.orthographicSize;
        float screenRight  = cam.transform.position.x + halfW;

        bool offScreen = transform.position.y < screenBottom - 1f ||
                         transform.position.x > screenRight  + 1f;

        if (offScreen)
        {
            _state = NPCState.OffScreen;
            StartCoroutine(TransitionRoutine());
        }
    }

    // ── Fade and load scene ───────────────────────────────────────
    private IEnumerator TransitionRoutine()
    {
        yield return new WaitForSeconds(offScreenDelay);

        if (fadeCanvas != null)
        {
            float elapsed  = 0f;
            float duration = 0.6f;

            while (elapsed < duration)
            {
                elapsed          += Time.deltaTime;
                fadeCanvas.alpha  = Mathf.Lerp(0f, 1f, elapsed / duration);
                yield return null;
            }
        }

        SceneManager.LoadScene(gameSceneName);
    }

    // ── Wander behaviour ──────────────────────────────────────────
    private void WanderBehaviour()
    {
        switch (_wanderState)
        {
            case WanderState.Waiting:
                _rb.linearVelocity = Vector2.zero;
                _animator.SetBool(ANIM_WALKING, false);
                _wanderTimer -= Time.deltaTime;

                if (_wanderTimer <= 0f)
                {
                    _wanderDirection = Random.value > 0.5f ? 1 : -1;
                    _wanderDirection = ClampToZone(_wanderDirection);

                    // Only set X target, Y never changes
                    _wanderTarget = new Vector3(
                        transform.position.x + (_wanderDirection * Random.Range(1f, 3f)),
                        transform.position.y,
                        transform.position.z
                    );

                    // Clamp target inside zone bounds
                    if (wanderZone != null)
                    {
                        Bounds b        = wanderZone.bounds;
                        _wanderTarget.x = Mathf.Clamp(_wanderTarget.x, b.min.x, b.max.x);
                    }

                    _wanderState = WanderState.Moving;
                }
                break;

            case WanderState.Moving:
                int clamped = ClampToZone(_wanderDirection);
                if (clamped != _wanderDirection)
                {
                    _rb.linearVelocity = Vector2.zero;
                    _wanderState       = WanderState.Waiting;
                    _wanderTimer       = Random.Range(wanderWaitMin, wanderWaitMax);
                    break;
                }

                // Flip sprite to face movement direction
                _sprite.flipX = _wanderDirection < 0;
                _animator.SetBool(ANIM_WALKING, true);

                // Only move on X axis, Y locked completely
                float newX = Mathf.MoveTowards(
                    transform.position.x,
                    _wanderTarget.x,
                    wanderSpeed * Time.deltaTime
                );
                transform.position = new Vector3(newX, transform.position.y, transform.position.z);

                if (Mathf.Abs(transform.position.x - _wanderTarget.x) < 0.05f)
                {
                    _rb.linearVelocity = Vector2.zero;
                    _wanderState       = WanderState.Waiting;
                    _wanderTimer       = Random.Range(wanderWaitMin, wanderWaitMax);
                }
                break;
        }
    }

    // ── Clamp direction to wander zone ────────────────────────────
    private int ClampToZone(int direction)
    {
        if (wanderZone == null) return direction;

        Bounds b = wanderZone.bounds;
        if (direction > 0 && transform.position.x >= b.max.x) return -1;
        if (direction < 0 && transform.position.x <= b.min.x) return  1;
        return direction;
    }
}