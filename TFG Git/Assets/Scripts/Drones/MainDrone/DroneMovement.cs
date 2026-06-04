using System;
using UnityEngine;
using UnityEngine.InputSystem;
 
public class DroneMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ButtonBehaviour nearbyButton;
 
    [SerializeField] public float speed = 5.0f;
    public Vector2 movementInput;
 
    private Rigidbody2D rb;
    private MainDrone_Actions mainDrone_Actions;
    private SpriteRenderer sprite;
 
    // Whether the drone is currently inside a BridgeEvents trigger
    private bool _nearBridge = false;
 
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
        nearbyButton?.tryPress();
    }
 
    private void BridgeAction_performed(InputAction.CallbackContext context)
    {
        if (!context.ReadValueAsButton()) return;
 
        if (!_nearBridge)
        {
            return;
        }
 
        OnBridgeAction?.Invoke();
    }
 
    private void Movement_canceled(InputAction.CallbackContext context)  => movementInput = Vector2.zero;
    private void Movement_performed(InputAction.CallbackContext context) => movementInput = context.ReadValue<Vector2>();
 
    // ── Physics ────────────────────────────────────────────────────────────
 
    void FixedUpdate()
    {
        if      (movementInput.x > 0) sprite.flipX = false;
        else if (movementInput.x < 0) sprite.flipX = true;
 
        rb.linearVelocity = movementInput * speed;
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
 