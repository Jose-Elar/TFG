using System;
using UnityEngine;

public class BridgeEvents : MonoBehaviour
{
    [Header("Bridge Object")]
    [Tooltip("The Tilemap child that has the BridgeReveal script.")]
    [SerializeField] private BridgeReveal bridgeReveal;        // ← changed

    public event Action OnBridgePressed;

    private DroneMovement _trackedDrone;

    // Called by GroundDrone when it arrives at the bridge
    public void RevealBridge()
    {
        if (bridgeReveal == null)
        {
            Debug.LogWarning("[BridgeEvents] No BridgeReveal assigned.");
            return;
        }

        bridgeReveal.RevealBridge();                           // ← changed
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