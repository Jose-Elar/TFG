using System;
using UnityEngine;
 
public class BridgeEvents : MonoBehaviour
{
    [Header("Bridge Object")]
    [Tooltip("The child GameObject named Bridge. Set inactive in the Inspector.")]
    [SerializeField] private GameObject Bridge;
 
    /// <summary>
    /// Fired when the player presses O while Main_Drone is in range.
    /// GroundDrone subscribes to this exactly like ButtonDrone subscribes to ButtonBehaviour.
    /// </summary>
    public event Action OnBridgePressed;
 
    private DroneMovement _trackedDrone;
 
    // Called by GroundDrone when it arrives at the bridge
    public void RevealBridge()
    {
        if (Bridge == null)
        {
            Debug.LogWarning("[BridgeEvents] No bridge object assigned.");
            return;
        }
 
        Bridge.SetActive(true);
        Debug.Log("[BridgeEvents] Bridge revealed.");
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
 