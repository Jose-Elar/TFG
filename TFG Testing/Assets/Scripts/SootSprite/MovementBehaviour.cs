using UnityEngine;

public class MovementBehaviour : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private Transform itemTarget;        // drag the item here
    [SerializeField] private float rayDistance = 80f;
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stopDistance = 0.3f;

    private bool itemDetected = false;
    private Rigidbody2D rb;
    private SpriteRenderer sprite;

    private bool hasReachedItem = false;
    private bool endSequenceStarted = false;

    [SerializeField] private CameraController cameraController;
    private bool triggered = false;

    void OnBecameInvisible()
    {
        if (triggered) return;

        if (CompareTag("Soot_Sprite"))
        {
            triggered = true;
            cameraController.StartCameraSequence();
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (hasReachedItem) return;

        if (!itemDetected)
            CheckRaycast();

        if (itemDetected)
            MoveTowardsItem();
    }

    private void CheckRaycast()
    {
        Vector2 direction = (itemTarget.position - transform.position).normalized;
        Vector2 rayOrigin = (Vector2)transform.position + direction * 1f;

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, rayDistance);

        // ← check for null BEFORE accessing hit.collider
        if (hit.collider == null) return;

        Debug.Log("Hit: " + hit.collider.gameObject.name);

        if (hit.collider.CompareTag("Target"))
            itemDetected = true;
    }
    private void MoveTowardsItem()
    {
        float distance = Vector2.Distance(transform.position, itemTarget.position);

        if (distance <= stopDistance)
        {
            rb.linearVelocity = Vector2.zero;
            hasReachedItem = true;
            return;
        }

        Vector2 direction = ((Vector2)itemTarget.position - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    void OnDrawGizmosSelected()
    {
        if (itemTarget == null) return;

        // Shows the ray in Scene view
        Gizmos.color = itemDetected ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, itemTarget.position);
    }
}