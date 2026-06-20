using System.Collections;
using UnityEngine;

public class LevelEndSequence : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MovementBehaviour npc;
    [SerializeField] private Transform         cliffEdge;
    [SerializeField] private string            endDialogueId = "end_lvl1";

    [Header("Movement")]
    [SerializeField] private float jumpForce    = 8f;        // ← walkSpeed eliminado de aquí
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
        npc.OnLastWaypointReached -= OnLastWaypointReached;
        npc.enabled = false;
        StartCoroutine(EndSequence());
    }

    private IEnumerator EndSequence()
    {
        yield return StartCoroutine(WalkToEdge());

        bool dialogueDone = false;
        TextManager.Instance.OnDialogueEnded += () => dialogueDone = true;
        TextManager.Instance.StartDialogue(endDialogueId);

        yield return new WaitUntil(() => dialogueDone);

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
                npc.moveSpeed * Time.deltaTime          
            );
            npc.transform.position = new Vector3(newX, npc.transform.position.y, npc.transform.position.z);

            npc.HandleFootsteps(npc.transform.position); 

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

        _rb.bodyType     = RigidbodyType2D.Dynamic;
        _rb.gravityScale = gravityScale;

        yield return new WaitForFixedUpdate();

        _rb.linearVelocity = new Vector2(npc.moveSpeed, jumpForce);  
        _animator.SetBool("isWalking", false);

        Debug.Log("[LevelEndSequence] NPC jumped off cliff.");
    }
}