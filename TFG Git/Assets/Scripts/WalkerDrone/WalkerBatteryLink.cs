using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WalkerBatteryLink : MonoBehaviour
{
    [Header("Battery Settings")]
    [SerializeField] private float batteryMin   = 0.1f;
    [SerializeField] private float batteryMax    = 1f;
    [SerializeField] private float chargeRate    = 0.3f;  
    [SerializeField] private float drainRate     = 0.15f; 

    [Header("Detection")]
    [SerializeField] private float detectionRadius = 2f;
    [SerializeField] private Transform droneTransform; 

    [Header("References")]
    [SerializeField] private Light2D walkerLight;
    [SerializeField] private MovementBehaviour walkerMovement;
    private float _baseMoveSpeed;

    [Header("Dialogue Warning")]
    [SerializeField] private string lowBatteryDialogueId;
    [SerializeField] private float lowBatteryThreshold = 0.15f;

    private float _battery;
    private bool  _warningFired = false;

    void Awake()
    {
        _battery = batteryMax;

        if (walkerMovement != null)
            _baseMoveSpeed = walkerMovement.moveSpeed;
    }

    void Update()
    {
        bool droneInside = IsDroneInRange();

        if (droneInside)
            _battery += chargeRate * Time.deltaTime;
        else
            _battery -= drainRate * Time.deltaTime;

        _battery = Mathf.Clamp(_battery, batteryMin, batteryMax);

        if (walkerLight != null)
            walkerLight.intensity = _battery;

        if (walkerMovement != null)
            walkerMovement.moveSpeed = _baseMoveSpeed * _battery;

        if (!_warningFired && _battery <= lowBatteryThreshold)
        {
            _warningFired = true;
            TriggerLowBatteryDialogue();
        }
    }

    private bool IsDroneInRange()
    {
        if (droneTransform == null) return false;

        float distance = Vector2.Distance(transform.position, droneTransform.position);
        return distance <= detectionRadius;
    }

    private void TriggerLowBatteryDialogue()
    {
        if (string.IsNullOrEmpty(lowBatteryDialogueId)) return;
        if (TextManager.Instance == null) return;

        TextManager.Instance.StartDialogue(lowBatteryDialogueId);
        Debug.Log("[WalkerBatteryLink] Low battery warning triggered.");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}