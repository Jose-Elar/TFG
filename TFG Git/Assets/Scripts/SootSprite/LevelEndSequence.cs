using System.Collections;
using UnityEngine;

public class LevelEndSequence : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MovementBehaviour npc;
    [SerializeField] private Transform         cliffEdge;
    [SerializeField] private string            endDialogueId = "end_lvl1";

    [Header("Movement")]
    [SerializeField] private float walkSpeed    = 2f;
    [SerializeField] private float jumpForce    = 8f;
    [SerializeField] private float gravityScale = 3f;

    private Rigidbody2D    _rb;
    private SpriteRenderer _sprite;
    private Animator       _animator;

    void Awake()
    {
        _rb      = npc.GetComponent<Rigidbody2D>();
        _sprite  = npc.GetComponent<SpriteRenderer>();
        _animator = npc.GetComponent<Animator>();
    }

    void Start()
    {
        npc.OnLastWaypointReached += OnLastWaypointReached;
    }

    void OnDestroy()
    {
        npc.OnLastWaypointReached -= OnLastWaypointReached;
    }

    private void OnLastWaypointReached()
    {
        // Unsubscribe immediately so it only fires once
        npc.OnLastWaypointReached -= OnLastWaypointReached;

        // Disable NPC movement script, we take over
        npc.enabled = false;

        StartCoroutine(EndSequence());
    }

    private IEnumerator EndSequence()
    {
        // ── Phase 1 — Walk to cliff edge ──────────────────────────
        yield return StartCoroutine(WalkToEdge());

        // ── Phase 2 — Start dialogue and wait ─────────────────────
        bool dialogueDone = false;
        TextManager.Instance.OnDialogueEnded += () => dialogueDone = true;
        TextManager.Instance.StartDialogue(endDialogueId);

        yield return new WaitUntil(() => dialogueDone);

        // ── Phase 3 — Jump off ────────────────────────────────────
        yield return StartCoroutine(JumpRoutine());
    }

    private IEnumerator WalkToEdge()
    {
        while (true)
        {
            Vector2 direction = ((Vector2)cliffEdge.position - (Vector2)transform.position).normalized;

            _sprite.flipX = direction.x < 0;
            _animator.SetBool("isWalking", true);

            float newX = Mathf.MoveTowards(
                npc.transform.position.x,
                cliffEdge.position.x,
                walkSpeed * Time.deltaTime
            );
            npc.transform.position = new Vector3(newX, npc.transform.position.y, npc.transform.position.z);

            float distance = Vector2.Distance(npc.transform.position, cliffEdge.position);

            if (distance <= 0.5f)
            {
                _rb.linearVelocity = Vector2.zero;
                _animator.SetBool("isWalking", false);
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator JumpRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        _rb.bodyType       = RigidbodyType2D.Dynamic;
        _rb.gravityScale   = gravityScale;

        yield return new WaitForFixedUpdate();

        _rb.linearVelocity = new Vector2(walkSpeed, jumpForce);
        _animator.SetBool("isWalking", false);

        Debug.Log("[LevelEndSequence] NPC jumped off cliff.");
    }
}