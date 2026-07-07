using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndSequence : MonoBehaviour
{
    [Header("Assist Drone")]
    [SerializeField] private Rigidbody2D assistDroneRb;

    [Header("References")]
    [SerializeField] private MovementBehaviour npc;
    [SerializeField] private Transform         cliffEdge;
    [SerializeField] private string            endDialogueId = "end_lvl1";

    [Header("Movement")]
    [SerializeField] private float jumpForce    = 8f;
    [SerializeField] private float gravityScale = 3f;

    [Header("Level 1 — Scene Transition")]
    [SerializeField] private string nextSceneName  = "Level2";
    [SerializeField] private float  offScreenDelay = 0.5f;

    [Header("Level 2 — Final Scene Settings")]
    [Tooltip("Nombre exacto de la escena final. Si la escena activa coincide con este nombre, se ejecuta la secuencia final.")]
    [SerializeField] private string finalLevelSceneName = "Level2";
    [SerializeField] private string finalSceneName      = "End_Scene";
    [SerializeField] private float  finalSceneDelay     = 2f;

    [Header("Level 2 — Alarm Sequence")]
    [SerializeField] private AudioSource       alarmAudioSource;
    [SerializeField] private LampStateLight[]  alarmLamps;

    private Rigidbody2D    _rb;
    private SpriteRenderer _sprite;
    private Animator       _animator;

    private enum EndState { None, Jumping, OffScreen }
    private EndState _endState = EndState.None;

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

    void Update()
    {
        if (_endState == EndState.OffScreen)
            CheckOffScreen();
    }

    private void OnLastWaypointReached()
    {
        npc.OnLastWaypointReached -= OnLastWaypointReached;
        npc.enabled = false;

        // Decide la rama según la escena activa 
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == finalLevelSceneName)
            StartCoroutine(Level2EndSequence());
        else
            StartCoroutine(Level1EndSequence());
    }


   

    private IEnumerator Level1EndSequence()
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

            if (Vector2.Distance(npc.transform.position, cliffEdge.position) <= 0.5f)
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
    if (assistDroneRb != null)
    {
        assistDroneRb.transform.SetParent(null);
        assistDroneRb.bodyType = RigidbodyType2D.Static;
    }

        yield return new WaitForSeconds(0.5f);

        _rb.bodyType     = RigidbodyType2D.Dynamic;
        _rb.gravityScale = gravityScale;

        yield return new WaitForFixedUpdate();

        _rb.linearVelocity = new Vector2(npc.moveSpeed, jumpForce);
        _animator.SetBool("isWalking", false);

        _endState = EndState.OffScreen;

        Debug.Log("[LevelEndSequence] NPC jumped off cliff.");
    }


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


    private IEnumerator Level2EndSequence()
    {
        // Phase 1 
        if (alarmAudioSource != null)
        {
            Debug.Log("[LevelEndSequence] Playing alarm sound.");
            alarmAudioSource.Play(); 
        }


        // Phase 2 
        foreach (LampStateLight lamp in alarmLamps)
        {
            Debug.Log("No Funciona" + lamp.name);
            if (lamp != null)
            {
                Debug.Log("Funciona" + lamp.name);
                lamp.SetState(LampStateLight.LightState.Alarm);
            }

        }

        // Phase 3
        bool dialogueDone = false;
        TextManager.Instance.OnDialogueEnded += () => dialogueDone = true;
        TextManager.Instance.StartDialogue(endDialogueId);

        yield return new WaitUntil(() => dialogueDone);

        // Phase 4 
        yield return new WaitForSeconds(finalSceneDelay);

        // Phase 5 
        SceneTransition.Instance.LoadScene(finalSceneName);

        Debug.Log("[LevelEndSequence] Final scene loaded.");
    }
}