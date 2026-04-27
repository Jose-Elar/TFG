using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class DroneMovement : MonoBehaviour
{
    //References
    [SerializeField] private ButtonBehaviour nearbyButton; // assigned at runtime via trigger

    [SerializeField] public float speed = 5.0f;
    public Vector2 movementInput;

    private Rigidbody2D rb;
    private MainDrone_Actions mainDrone_Actions;

    private SpriteRenderer sprite;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainDrone_Actions = new MainDrone_Actions();  
        sprite = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        mainDrone_Actions.Drone.Enable();

        mainDrone_Actions.Drone.Movement.performed += Movement_performed;
        mainDrone_Actions.Drone.Movement.canceled += Movement_canceled;
        mainDrone_Actions.Drone.Interact.performed += Interact_performed;
    }

    private void Interact_performed(InputAction.CallbackContext context)
    {
        bool interact = context.ReadValueAsButton();
        Debug.Log($"Interact fired | interact: {interact} | nearbyButton: {nearbyButton}");
        if (interact)
        {
            nearbyButton?.tryPress();
        }
    }

    private void Movement_canceled(InputAction.CallbackContext context)
    {
        movementInput = Vector2.zero;
    }

    private void Movement_performed(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        if (movementInput.x > 0)
        {
            sprite.flipX = false;
        }
        else if (movementInput.x < 0)
        {
            sprite.flipX = true; 
        }

        rb.linearVelocity = movementInput * speed ;
    }

    public void SetNearbyButton(ButtonBehaviour button)
    {
        nearbyButton = button;
        Debug.Log($"SetNearbyButton called | button: {button}");
    }

}
