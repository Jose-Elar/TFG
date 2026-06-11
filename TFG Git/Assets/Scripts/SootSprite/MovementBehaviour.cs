using UnityEngine;

public class MovementBehaviour : MonoBehaviour
{
    [Header("SavePoint Lights")]
    [SerializeField] private SavePointLight[] allSaveLights;

    [Header("Waypoints")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float stopDistance = 0.3f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("Detection")]
    [SerializeField] private float rayDistance = 80f;
    [Tooltip("Layers the obstacle raycast can hit. Exclude trigger-only layers like bridge zones.")]
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Gap Detection")]
    [SerializeField] private float gapCheckDistance = 1.5f;
    [SerializeField] private float groundCheckDepth = 2.5f;
    [SerializeField] private int gapRayCount = 3;
    [SerializeField] private float rayVerticalOffset = -0.4f;

    [Tooltip("Layer(s) considered as ground.")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Jump")]
    [Tooltip("Upward force applied when jumping over a hole.")]
    [SerializeField] private float jumpForce = 6f;

    [Tooltip("Seconds to wait before jumping and after landing.")]
    [SerializeField] private float landingPauseTime = 0.5f;

    [Tooltip("Layer(s) considered as ground for landing detection.")]
    [SerializeField] private LayerMask landingLayer;

    [Tooltip("How far below to check if the NPC has landed.")]
    [SerializeField] private float landingCheckDepth = 0.6f;

    [Header("Idle Wander")]
    [SerializeField] private bool enableIdleWander = true;
    [SerializeField] private float wanderSpeed = 1f;

    [Tooltip("Min seconds to wait between wander moves.")]
    [SerializeField] private float wanderWaitMin = 1f;

    [Tooltip("Max seconds to wait between wander moves.")]
    [SerializeField] private float wanderWaitMax = 3f;

    [Tooltip("How far the character moves in one wander step.")]
    [SerializeField] private float wanderMoveDistance = 1.5f;

    [Tooltip("How close to the screen edge before reversing wander direction.")]
    [SerializeField] private float screenEdgeMargin = 1f;

    // ── internals ──────────────────────────────────────────────────────────
    private int _currentWaypointIndex = 0;

    [SerializeField] private bool _waypointInSight = false;
    [SerializeField] private bool _gapAhead        = false;
    [SerializeField] private bool _isJumping       = false;

    private bool  _waitingToJump = false;
    private bool  _jumpApplied   = false;
    private bool  _waitingToLand = false;
    private float _jumpTimer     = 0f;
    private float _landTimer     = 0f;
    private float _airborneTimer = 0f;

    // ── Wander state ───────────────────────────────────────────────────────
    private enum WanderState { Waiting, Moving }
    private WanderState _wanderState     = WanderState.Waiting;
    private float       _wanderTimer     = 0f;
    private int         _wanderDirection = 1;
    private Vector3     _wanderTarget;

    // ── Checkpoint ────────────────────────────────────────────────────────
    private Vector3? _lastCheckpointPosition = null;

    private Rigidbody2D    _rb;
    private SpriteRenderer _sprite;
    private Animator       _animator;                          // ← added

    void Awake()
    {
        _rb       = GetComponent<Rigidbody2D>();
        _sprite   = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();                  // ← added

        _rb.freezeRotation = true;
        _wanderTimer = Random.Range(wanderWaitMin, wanderWaitMax);
    }

    void Update()
    {
        if (_currentWaypointIndex >= waypoints.Length) return;

        // ── Post-land wait ─────────────────────────────────────────────────
        if (_waitingToLand)
        {
            _landTimer -= Time.unscaledDeltaTime;
            if (_landTimer <= 0f)
            {
                _waitingToLand = false;
                _isJumping     = false;
            }
            return;
        }

        // ── In the air after jump ──────────────────────────────────────────
        if (_isJumping && _jumpApplied)
        {
            _airborneTimer -= Time.unscaledDeltaTime;
            if (_airborneTimer <= 0f && IsGrounded())
            {
                _rb.linearVelocity = Vector2.zero;
                _waitingToLand     = true;
                _landTimer         = landingPauseTime;
            }
            return;
        }

        // ── Pre-jump wait ──────────────────────────────────────────────────
        if (_waitingToJump)
        {
            _jumpTimer -= Time.unscaledDeltaTime;
            if (_jumpTimer <= 0f)
            {
                Vector2 moveDir = ((Vector2)CurrentTarget.position - (Vector2)transform.position).normalized;
                _rb.linearVelocity = new Vector2(moveDir.x * moveSpeed, jumpForce);
                _waitingToJump = false;
                _jumpApplied   = true;
                _airborneTimer = 0.3f;
            }
            return;
        }

        // ── Normal movement ────────────────────────────────────────────────
        _gapAhead = IsGapAhead(out bool isJumpHole);

        if (_gapAhead)
        {
            _rb.linearVelocity = Vector2.zero;
            _animator.SetBool("isWalking", false);             // ← idle at gap
            if (isJumpHole)
            {
                _waitingToJump = true;
                _isJumping     = true;
                _jumpApplied   = false;
                _jumpTimer     = landingPauseTime;
            }
            return;
        }

        if (!IsGrounded()) return;

        _waypointInSight = CheckRaycast();

        if (!_waypointInSight)
        {
            if (enableIdleWander)
                WanderBehaviour();
            else
            {
                _rb.linearVelocity = Vector2.zero;
                _animator.SetBool("isWalking", false);         // ← idle when stopped
            }
            return;
        }

        MoveTowardsWaypoint();
    }

    // ── Waypoint logic ─────────────────────────────────────────────────────

    private Transform CurrentTarget => waypoints[_currentWaypointIndex];

    private bool CheckRaycast()
    {
        Vector2 direction          = (CurrentTarget.position - transform.position).normalized;
        Vector2 rayOrigin          = (Vector2)transform.position + direction * 1f;
        float   distanceToWaypoint = Vector2.Distance(transform.position, CurrentTarget.position);

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, distanceToWaypoint, obstacleLayer);

        if (hit.collider == null) return true;

        Debug.Log("[MovementBehaviour] Path blocked by: " + hit.collider.gameObject.name);
        return false;
    }

    private void MoveTowardsWaypoint()
    {
        float distance = Vector2.Distance(transform.position, CurrentTarget.position);

        if (distance <= stopDistance)
        {
            _rb.linearVelocity      = Vector2.zero;
            _lastCheckpointPosition = CurrentTarget.position;
            _animator.SetBool("isWalking", false);             // ← idle on arrival

            Debug.Log("[MovementBehaviour] Reached waypoint " + _currentWaypointIndex
                      + " (" + CurrentTarget.name + ")");

            ActivateSavePoint(_currentWaypointIndex);
            _currentWaypointIndex++;

            if (_currentWaypointIndex >= waypoints.Length)
            {
                Debug.Log("[MovementBehaviour] All waypoints reached.");
                return;
            }

            Debug.Log("[MovementBehaviour] Moving to waypoint " + _currentWaypointIndex
                      + " (" + waypoints[_currentWaypointIndex].name + ")");
            return;
        }

        Vector2 direction = ((Vector2)CurrentTarget.position - (Vector2)transform.position).normalized;
        _rb.linearVelocity = direction * moveSpeed;

        // Flip sprite
        if      (direction.x > 0) _sprite.flipX = false;
        else if (direction.x < 0) _sprite.flipX = true;

        _animator.SetBool("isWalking", true);                  // ← walk while moving
    }

    public Vector3 GetLastCheckpointPosition()
    {
        if (_lastCheckpointPosition == null) return Vector3.zero;
        Vector3 pos = _lastCheckpointPosition.Value;
        return new Vector3(pos.x, pos.y + 2f, pos.z);
    }

    // ── Wander behaviour ───────────────────────────────────────────────────

    private void WanderBehaviour()
    {
        switch (_wanderState)
        {
            case WanderState.Waiting:
                _rb.linearVelocity = Vector2.zero;
                _animator.SetBool("isWalking", false);         // ← idle while waiting
                _wanderTimer -= Time.deltaTime;

                if (_wanderTimer <= 0f)
                {
                    _wanderDirection = Random.value > 0.5f ? 1 : -1;
                    _wanderDirection = ClampDirectionToScreen(_wanderDirection);
                    _wanderTarget    = transform.position
                                     + new Vector3(_wanderDirection * wanderMoveDistance, 0f, 0f);
                    _wanderState     = WanderState.Moving;
                }
                break;

            case WanderState.Moving:
                int clamped = ClampDirectionToScreen(_wanderDirection);
                if (clamped != _wanderDirection)
                {
                    _rb.linearVelocity = Vector2.zero;
                    _animator.SetBool("isWalking", false);     // ← idle when stopping
                    _wanderState = WanderState.Waiting;
                    _wanderTimer = Random.Range(wanderWaitMin, wanderWaitMax);
                    break;
                }

                // Flip sprite during wander
                if      (_wanderDirection > 0) _sprite.flipX = false;
                else if (_wanderDirection < 0) _sprite.flipX = true;

                _animator.SetBool("isWalking", true);          // ← walk while wandering

                transform.position = Vector2.MoveTowards(
                    transform.position,
                    _wanderTarget,
                    wanderSpeed * Time.deltaTime
                );

                if (Vector2.Distance(transform.position, _wanderTarget) < 0.05f)
                {
                    _rb.linearVelocity = Vector2.zero;
                    _animator.SetBool("isWalking", false);     // ← idle on wander arrival
                    _wanderState = WanderState.Waiting;
                    _wanderTimer = Random.Range(wanderWaitMin, wanderWaitMax);
                }
                break;
        }
    }

    private int ClampDirectionToScreen(int direction)
    {
        Camera cam = Camera.main;
        if (cam == null) return direction;

        float halfW = cam.orthographicSize * cam.aspect;
        float camX  = cam.transform.position.x;

        float leftEdge  = camX - halfW + screenEdgeMargin;
        float rightEdge = camX + halfW - screenEdgeMargin;

        if (direction > 0 && transform.position.x >= rightEdge) return -1;
        if (direction < 0 && transform.position.x <= leftEdge)  return  1;

        return direction;
    }

    // ── Grounded check ─────────────────────────────────────────────────────

    private bool IsGrounded()
    {
        float halfWidth = 0.3f;

        Vector2 centerOrigin = (Vector2)transform.position + new Vector2(0f,         rayVerticalOffset);
        Vector2 leftOrigin   = (Vector2)transform.position + new Vector2(-halfWidth,  rayVerticalOffset);
        Vector2 rightOrigin  = (Vector2)transform.position + new Vector2( halfWidth,  rayVerticalOffset);

        RaycastHit2D center = Physics2D.Raycast(centerOrigin, Vector2.down, landingCheckDepth, landingLayer);
        RaycastHit2D left   = Physics2D.Raycast(leftOrigin,   Vector2.down, landingCheckDepth, landingLayer);
        RaycastHit2D right  = Physics2D.Raycast(rightOrigin,  Vector2.down, landingCheckDepth, landingLayer);

        return center.collider != null || left.collider != null || right.collider != null;
    }

    // ── Gap detection ──────────────────────────────────────────────────────

    private bool IsGapAhead(out bool isJumpHole)
    {
        isJumpHole = false;
        if (waypoints == null || waypoints.Length == 0) return false;

        Vector2 moveDir = ((Vector2)CurrentTarget.position - (Vector2)transform.position).normalized;

        for (int i = 0; i < gapRayCount; i++)
        {
            float t       = gapRayCount == 1 ? 0f : (float)i / (gapRayCount - 1);
            float spreadX = moveDir.x * gapCheckDistance * (1f + t);

            Vector2 probeOrigin = (Vector2)transform.position
                                + new Vector2(spreadX, rayVerticalOffset);

            RaycastHit2D hit = Physics2D.Raycast(probeOrigin, Vector2.down,
                                                 groundCheckDepth, groundLayer);

            if (hit.collider == null)
            {
                RaycastHit2D jumpCheck = Physics2D.Raycast(probeOrigin, Vector2.down,
                                                           groundCheckDepth, ~0);
                if (jumpCheck.collider != null && jumpCheck.collider.CompareTag("JumpHole"))
                    isJumpHole = true;

                return true;
            }
        }

        return false;
    }

    public void ActivateSavePoint(int index)
    {
        for (int i = 0; i < allSaveLights.Length; i++)
        {
            if (i == index)
                allSaveLights[i].SetState(SavePointLight.LightState.Flashing);
            else
                allSaveLights[i].SetState(SavePointLight.LightState.Deactivated);
        }
    }

    // ── Gizmos ─────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            bool isCurrent = (i == _currentWaypointIndex);
            Gizmos.color = isCurrent ? Color.green : Color.gray;

            Vector3 from = (i == 0) ? transform.position : waypoints[i - 1].position;
            Gizmos.DrawLine(from, waypoints[i].position);
            Gizmos.DrawSphere(waypoints[i].position, 0.15f);
        }

        if (waypoints.Length > _currentWaypointIndex)
        {
            Vector2 dir            = (CurrentTarget.position - transform.position).normalized;
            float   distToWaypoint = Vector2.Distance(transform.position, CurrentTarget.position);
            Gizmos.color = _waypointInSight ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + dir * distToWaypoint);

            Vector2 moveDir = ((Vector2)CurrentTarget.position - (Vector2)transform.position).normalized;

            for (int i = 0; i < gapRayCount; i++)
            {
                float t       = gapRayCount == 1 ? 0f : (float)i / (gapRayCount - 1);
                float spreadX = moveDir.x * gapCheckDistance * (1f + t);

                Vector2 probeOrigin = (Vector2)transform.position
                                    + new Vector2(spreadX, rayVerticalOffset);

                RaycastHit2D hit = Physics2D.Raycast(probeOrigin, Vector2.down,
                                                     groundCheckDepth, groundLayer);
                Gizmos.color = hit.collider == null ? Color.red : Color.cyan;
                Gizmos.DrawLine(probeOrigin, probeOrigin + Vector2.down * groundCheckDepth);
            }
        }

        Gizmos.color = Color.magenta;
        float halfWidth  = 0.3f;
        Vector2[] groundOrigins = {
            (Vector2)transform.position + new Vector2(-halfWidth, rayVerticalOffset),
            (Vector2)transform.position + new Vector2(0f,         rayVerticalOffset),
            (Vector2)transform.position + new Vector2( halfWidth,  rayVerticalOffset)
        };
        foreach (Vector2 origin in groundOrigins)
            Gizmos.DrawLine(origin, origin + Vector2.down * landingCheckDepth);
    }
}