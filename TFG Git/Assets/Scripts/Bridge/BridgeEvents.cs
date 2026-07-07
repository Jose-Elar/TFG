using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BridgeEvents : MonoBehaviour
{
    [Header("Bridge Object")]
    [Tooltip("The Tilemap child that has the BridgeReveal script.")]
    [SerializeField] private BridgeReveal bridgeReveal;

    [Header("Indicator Light")]
    [SerializeField] private Light2D indicatorLight;
    [SerializeField] private float pulseSpeed = 1f; 

    private bool _lightDeactivated = false;

    public event Action OnBridgePressed;

    private DroneMovement _trackedDrone;

    void Update()
    {
        if (_lightDeactivated || indicatorLight == null) return;

   
        indicatorLight.intensity = (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI) + 1f) / 2f;
    }

    public void RevealBridge()
    {
        if (bridgeReveal == null)
        {
            Debug.LogWarning("[BridgeEvents] No BridgeReveal assigned.");
            return;
        }

        if (indicatorLight != null)
        {
            _lightDeactivated = true;
            indicatorLight.intensity = 0f;
            indicatorLight.enabled = false;
        }

        bridgeReveal.RevealBridge();
        Debug.Log("[BridgeEvents] Bridge reveal triggered.");
    }

    private void FireBridgePressed()
    {
        Debug.Log("[BridgeEvents] Bridge action confirmed.");
        OnBridgePressed?.Invoke();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Main_Drone")) return;

        _trackedDrone = other.GetComponent<DroneMovement>();
        if (_trackedDrone != null)
        {
            _trackedDrone.SetNearBridge(true);
            _trackedDrone.OnBridgeAction += FireBridgePressed;
        }

        Debug.Log("[BridgeEvents] Main_Drone in range.");
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Main_Drone")) return;

        if (_trackedDrone != null)
        {
            _trackedDrone.SetNearBridge(false);
            _trackedDrone.OnBridgeAction -= FireBridgePressed;
            _trackedDrone = null;
        }

        Debug.Log("[BridgeEvents] Main_Drone out of range.");
    }
}