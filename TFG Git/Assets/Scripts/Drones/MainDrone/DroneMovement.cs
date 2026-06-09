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
 
    // Whether the drone is currently inside a BridgeEvents trigger
    private bool _nearBridge = false;

    private bool _isScanning = false;
 
    /// <summary>
    /// Subscribed to by BridgeEvents when the drone is in range.
    /// Fired only when O is pressed AND the drone is near the bridge.
    /// </summary>
    public event Action OnBridgeAction;
 
    void Awake()
    {
        rb                = GetComponent<Rigidbody2D>();
        mainDrone_Actions = new MainDrone_Actions();
        sprite            = GetComponent<SpriteRenderer>();

        animator = GetComponent<Animator>();
    }
 
    void Start()
    {
        mainDrone_Actions.Drone.Enable();
 
        mainDrone_Actions.Drone.Movement.performed        += Movement_performed;
        mainDrone_Actions.Drone.Movement.canceled         += Movement_canceled;
        mainDrone_Actions.Drone.Interact_Button.performed += Interact_performed;
        mainDrone_Actions.Drone.Interact_Bridge.performed   += BridgeAction_performed;
    }
 
    void OnDestroy()
    {
        mainDrone_Actions.Drone.Movement.performed        -= Movement_performed;
        mainDrone_Actions.Drone.Movement.canceled         -= Movement_canceled;
        mainDrone_Actions.Drone.Interact_Button.performed -= Interact_performed;
        mainDrone_Actions.Drone.Interact_Bridge.performed   -= BridgeAction_performed;
    }
 
    // ── Input callbacks ────────────────────────────────────────────────────
 
    private void Interact_performed(InputAction.CallbackContext context)
    {
        if (!context.ReadValueAsButton()) return;
        if (_isScanning)
        {
            return; // prevent spamming the scan action
        }

        nearbyButton?.tryPress();
        if (nearbyButton != null)
        {
            StartCoroutine(ScanRoutine());
        }
    }
 
    private void BridgeAction_performed(InputAction.CallbackContext context)
    {
        if (!context.ReadValueAsButton()) return;
 
        if (!_nearBridge)
        {
            return;
        }

        if (_isScanning)
        {
            return; // prevent spamming the scan action
        }
 
        OnBridgeAction?.Invoke();
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
            return; // skip everything else
        }

        if      (movementInput.x > 0) sprite.flipX = false;
        else if (movementInput.x < 0) sprite.flipX = true;

        rb.linearVelocity = movementInput * speed;

        // Tilt 
        float targetAngle = movementInput.x * -15f; // 15 degrees max tilt
        float smoothAngle = Mathf.LerpAngle(transform.rotation.eulerAngles.z, targetAngle, Time.fixedDeltaTime * 8f ) ;// 8 = tilt speed, adjust to taste)
        transform.rotation = Quaternion.Euler(0, 0, smoothAngle);

        bool moving = movementInput != Vector2.zero;
        animator.SetBool("isMoving", moving);
    }

// The scan coroutine
private IEnumerator ScanRoutine()
{
    _isScanning = true;
    
    // Stop movement instantly
    movementInput = Vector2.zero;
    rb.linearVelocity = Vector2.zero;
    
    // Trigger the animation
    animator.SetTrigger("doScan");
    
    // Wait for scan animation to finish
    // Get the clip length automatically
    float scanLength = GetAnimationLength("Drone_Scan");
    yield return new WaitForSeconds(scanLength);
    
    _isScanning = false;
}

// Helper to get clip duration by name
private float GetAnimationLength(string clipName)
{
    foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
    {
        if (clip.name == clipName)
            return clip.length;
    }
    return 1f; // fallback if not found
}



 
    // ── Public API ─────────────────────────────────────────────────────────
 
    public void SetNearbyButton(ButtonBehaviour button)
    {
        nearbyButton = button;
    }
 
    /// <summary>
    /// Called by BridgeEvents when the drone enters or exits its trigger.
    /// </summary>
    public void SetNearBridge(bool inRange)
    {
        _nearBridge = inRange;
    }
}
 