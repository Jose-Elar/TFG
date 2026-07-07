using System.Collections;
using UnityEngine;

public class WanderStateDrones : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float waypointReachedDistance = 0.2f;
    public float minWaitTime = 0.3f;
    public float maxWaitTime = 1f;

    private Vector2 targetPosition;
    private bool isWaiting = false;
    private bool isPaused = false;

    [SerializeField] private BoxCollider2D zoneCollider;
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator animator;                   
    [Header("Tilt")]
    [SerializeField] private float maxTiltAngle = 15f;
    [SerializeField] private float tiltSpeed = 8f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        sprite   = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();          

        zoneCollider = GetComponentInParent<BoxCollider2D>();
        if (zoneCollider == null)
        {
            Debug.LogError("No se encontró un BoxCollider2D en el padre del drone.");
            return;
        }

        PickNewtarget();
    }

    void Update()
    {
        if (isWaiting || isPaused)
        {
            ApplyTilt(0f);
            animator.SetFloat("velocityX", 0f);       
            return;
        }

        MoveTowardsTarget();

        if (Vector2.Distance(transform.position, targetPosition) <= waypointReachedDistance)
            StartCoroutine(WaitThenPickTarget());
    }

    private void MoveTowardsTarget()
    {
        Vector2 direction = ((Vector2)targetPosition - (Vector2)transform.position).normalized;

        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    
        animator.SetFloat("velocityX", direction.x);

        ApplyTilt(direction.x * maxTiltAngle);
    }

    private void ApplyTilt(float targetAngle)
    {
        float smoothAngle = Mathf.LerpAngle(
            transform.rotation.eulerAngles.z,
            -targetAngle,
            Time.deltaTime * tiltSpeed
        );
        transform.rotation = Quaternion.Euler(0, 0, smoothAngle);
    }

    private void PickNewtarget()
    {
        Bounds b = zoneCollider.bounds;
        float padding = 0.1f;
        float x = Random.Range(b.min.x + padding, b.max.x - padding);
        float y = Random.Range(b.min.y + padding, b.max.y - padding);
        targetPosition = new Vector2(x, y);
    }

    IEnumerator WaitThenPickTarget()
    {
        isWaiting = true;
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));
        PickNewtarget();
        isWaiting = false;
    }

    public void PauseWander()
    {
        isPaused = true;
        isWaiting = false;
        StopAllCoroutines();
        rb.linearVelocity = Vector2.zero;
    }

    public void ResumeWander()
    {
        isPaused = false;
        PickNewtarget();
    }

    void OnDrawGizmosSelected()
    {
        if (zoneCollider != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(zoneCollider.bounds.center, zoneCollider.bounds.size);
        }
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(targetPosition, 0.1f);
        Gizmos.DrawLine(transform.position, targetPosition);
    }
}