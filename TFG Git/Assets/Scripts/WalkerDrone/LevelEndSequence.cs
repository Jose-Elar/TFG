using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndSequence : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MovementBehaviour npc;
    [SerializeField] private Transform         cliffEdge;
    [SerializeField] private string            endDialogueId = "end_lvl1";

    [Header("Movement")]
    [SerializeField] private float jumpForce    = 8f;
    [SerializeField] private float gravityScale = 3f;

    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName       = "Level2";
    [SerializeField] private float  offScreenDelay      = 0.5f;

    [Header("Final Scene Settings")]
    [Tooltip("Nombre exacto de la escena en la que esta secuencia debe terminar en los créditos en vez de saltar.")]
    [SerializeField] private string finalLevelSceneName = "Level2";
    [SerializeField] private string finalSceneName       = "End_Scene";
    [SerializeField] private float  finalSceneDelay      = 2f;

    private Rigidbody2D    _rb;
    private SpriteRenderer _sprite;
    private Animator       _animator;

    private enum EndState { None, Jumping, OffScreen }
    private EndState _endState = EndState.None;

    void Awake()
    {
        _rb       = npc.GetComponent<Rigidbody2D>();
        _sprite   = npc.GetComponent<SpriteRenderer>();
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

    void Update()
    {
        if (_endState == EndState.OffScreen)
            CheckOffScreen();
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

        // ── Decide el comportamiento según la escena activa ────────
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == finalLevelSceneName)
        {
            yield return StartCoroutine(FinalSceneRoutine());
        }
        else
        {
            yield return StartCoroutine(JumpRoutine());
        }
    }

    // ── Caminar hasta el borde del precipicio ──────────────────────
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

    // ── Caso normal: salto al precipicio + transición offscreen ────
    private IEnumerator JumpRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        _rb.bodyType     = RigidbodyType2D.Dynamic;
        _rb.gravityScale = gravityScale;

        yield return new WaitForFixedUpdate();

        _rb.linearVelocity = new Vector2(npc.moveSpeed, jumpForce);
        _animator.SetBool("isWalking", false);

        _endState = EndState.OffScreen;

        Debug.Log("[LevelEndSequence] NPC jumped off cliff.");
    }

    // ── Comprueba si el NPC ha salido de pantalla tras el salto ─────
    private void CheckOffScreen()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float halfW        = cam.orthographicSize * cam.aspect;
        float screenBottom = cam.transform.position.y - cam.orthographicSize;
        float screenRight  = cam.transform.position.x + halfW;

        bool offScreen = npc.transform.position.y < screenBottom - 1f ||
                         npc.transform.position.x > screenRight  + 1f;

        if (offScreen)
        {
            _endState = EndState.None;
            StartCoroutine(TransitionToNextLevel());
        }
    }

    private IEnumerator TransitionToNextLevel()
    {
        yield return new WaitForSeconds(offScreenDelay);
        SceneTransition.Instance.LoadScene(nextSceneName);
    }

    // ── Caso especial: estamos en Level2, vamos a la escena final ───
    private IEnumerator FinalSceneRoutine()
    {
        Debug.Log("[LevelEndSequence] Final level reached, going to End_Scene.");

        yield return new WaitForSeconds(finalSceneDelay);

        SceneTransition.Instance.LoadScene(finalSceneName);
    }
}