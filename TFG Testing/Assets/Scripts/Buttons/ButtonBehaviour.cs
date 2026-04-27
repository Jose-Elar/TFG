using UnityEngine;

public class ButtonBehaviour : MonoBehaviour
{
    public event System.Action OnButtonActivated;
    public event System.Action OnButtonPressed;

    public HUDScript hud;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer buttonSprite;
    [SerializeField] private Material materialRed;
    [SerializeField] private Material materialGreen;

    private bool droneInRange = false;      // main drone in range
    private bool assistDroneInRange = false;// assist drone in range

    private bool isActivated = false;

    public void tryPress()
    {
        if (!droneInRange) return;
        if(isActivated) return;

        OnButtonPressed?.Invoke();
        hud.pressedButton();
    }

    // Called by ButtonDrone when it arrives
    public void SetActivated(bool activated)
    {
        isActivated = activated;
        buttonSprite.material = activated ? materialGreen : materialRed;
        if (activated)
            OnButtonActivated?.Invoke();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Main_Drone"))
        {
            droneInRange = true;
            hud.SetCanPressButton(true);
            collision.GetComponent<DroneMovement>()?.SetNearbyButton(this);
        }

        if (collision.gameObject.CompareTag("Assist_Drone"))
        {
            assistDroneInRange = true;
            SetActivated(true);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Main_Drone"))
        {
            droneInRange = false;
            hud.SetCanPressButton(false);
            collision.GetComponent<DroneMovement>()?.SetNearbyButton(null);
        }


    }
}