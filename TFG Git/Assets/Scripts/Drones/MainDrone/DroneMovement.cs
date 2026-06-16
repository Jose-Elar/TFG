using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DroneMovement : MonoBehaviour
{
    private Animator animator;

    [Header("References")]
    [SerializeField] private ButtonBehaviour nearbyButton;

    [SerializeField] public float speed = 5.0f;
    public Vector2 movementInput;

    private Rigidbody2D rb;
    private MainDrone_Actions mainDrone_Actions;
    private SpriteRenderer sprite;

    private bool _nearBridge      = false;
    private bool _isScanning      = false;
    private bool _nearInteractable = false;                         // ← added

    private RockInteractable _nearbyInteractable = null;           // ← added

    public event Action OnBridgeAction;

    void Awake()
    {
        rb                = GetComponent<Rigidbody2D>();
        mainDrone_Actions = new MainDrone_Actions();
        sprite            = GetComponent<SpriteRenderer>();
        animator          = GetComponent<Animator>();
    }

    void Start()
    {
        mainDrone_Actions.Drone.Enable();

        mainDrone_Actions.Drone.Movement.performed           += Movement_performed;
        mainDrone_Actions.Drone.Movement.canceled            += Movement_canceled;
        mainDrone_Actions.Drone.Interact_Button.performed    += Interact_performed;
        mainDrone_Actions.Drone.Interact_Bridge.performed    += BridgeAction_performed;
        mainDrone_Actions.Drone.MainDrone_Interact.performed += MainInteract_performed; // ← added
    }

    void OnDestroy()
    {
        mainDrone_Actions.Drone.Movement.performed           -= Movement_performed;
        mainDrone_Actions.Drone.Movement.canceled            -= Movement_canceled;
        mainDrone_Actions.Drone.Interact_Button.performed    -= Interact_performed;
        mainDrone_Actions.Drone.Interact_Bridge.performed    -= BridgeAction_performed;
        mainDrone_Actions.Drone.MainDrone_Interact.performed -= MainInteract_performed; // ← added
    }

    // ── Input callbacks ────────────────────────────────────────────────────

    private void Interact_performed(InputAction.CallbackContext context)
    {
        if (!context.ReadValueAsButton()) return;
        if (_isScanning) return;

        nearbyButton?.tryPress();
        if (nearbyButton != null)
            StartCoroutine(ScanRoutine());
    }

    private void BridgeAction_performed(InputAction.CallbackContext context)
    {
        if (!context.ReadValueAsButton()) return;
        if (!_nearBridge) return;
        if (_isScanning) return;

        OnBridgeAction?.Invoke();
        StartCoroutine(ScanRoutine());
    }

    private void MainInteract_performed(InputAction.CallbackContext context) // ← added
    {
        if (!context.ReadValueAsButton()) return;
        if (_isScanning) return;
        if (!_nearInteractable) return;

        _nearbyInteractable?.Activate();
        StartCoroutine(ScanRoutine());
    }

    private void Movement_canceled(InputAction.CallbackContext context)  => movementInput = Vector2.zero;
    private void Movement_performed(InputAction.CallbackContext context) => movementInput = context.ReadValue<Vector2>();

    // ── Physics ────────────────────────────────────────────────────────────
    void FixedUpdate()
    {
        if (_isScanning)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if      (movementInput.x > 0) sprite.flipX = false;
        else if (movementInput.x < 0) sprite.flipX = true;

        rb.linearVelocity = movementInput * speed;

        float targetAngle = movementInput.x * -15f;
        float smoothAngle = Mathf.LerpAngle(transform.rotation.eulerAngles.z, targetAngle, Time.fixedDeltaTime * 8f);
        transform.rotation = Quaternion.Euler(0, 0, smoothAngle);

        bool moving = movementInput != Vector2.zero;
        animator.SetBool("isMoving", moving);
    }

    // ── Scan coroutine ─────────────────────────────────────────────────────
    private IEnumerator ScanRoutine()
    {
        _isScanning = true;

        movementInput     = Vector2.zero;
        rb.linearVelocity = Vector2.zero;

        animator.SetTrigger("doScan");

        float scanLength = GetAnimationLength("Drone_Scan");
        yield return new WaitForSeconds(scanLength);

        _isScanning = false;
    }

    private float GetAnimationLength(string clipName)
    {
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
                return clip.length;
        }
        return 1f;
    }

    // ── Public API ─────────────────────────────────────────────────────────
    public void SetNearbyButton(ButtonBehaviour button)
    {
        nearbyButton = button;
    }

    public void SetNearBridge(bool inRange)
    {
        _nearBridge = inRange;
    }

    public void SetNearbyInteractable(RockInteractable interactable) // ← added
    {
        _nearbyInteractable  = interactable;
        _nearInteractable    = interactable != null;
    }
}