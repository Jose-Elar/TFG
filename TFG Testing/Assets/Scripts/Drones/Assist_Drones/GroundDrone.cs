using System.Collections;
using UnityEngine;
 
public class GroundDrone : MonoBehaviour
{
    [SerializeField] private BridgeEvents bridgeEvents;
 
    [SerializeField] private float activationDistance = 1.5f;
    [SerializeField] private float activationDelay    = 1f;
 
    private WanderStateDrones _wander;
    private bool _isOnMission = false;
 
    void Awake()
    {
        _wander = GetComponent<WanderStateDrones>();
    }
 
    void OnEnable()
    {
        bridgeEvents.OnBridgePressed += OnBridgePressed;
    }
 
    void OnDisable()
    {
        bridgeEvents.OnBridgePressed -= OnBridgePressed;
    }
 
    private void OnBridgePressed()
    {
        if (_isOnMission) return;
        StartCoroutine(GoToBridge());
    }
 
    IEnumerator GoToBridge()
    {
        _isOnMission = true;
        _wander.PauseWander();
 
        Vector2 bridgePos = bridgeEvents.transform.position;
 
        while (Vector2.Distance(transform.position, bridgePos) > activationDistance)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                bridgePos,
                _wander.moveSpeed * Time.deltaTime
            );
            yield return null;
        }
 
        // Arrived — wait then reveal the bridge
        yield return new WaitForSeconds(activationDelay);
 
        bridgeEvents.RevealBridge();
 
        Debug.Log("[GroundDrone] Bridge revealed — resuming wander.");
 
        _wander.ResumeWander();
        _isOnMission = false;
    }
 
    void OnDrawGizmosSelected()
    {
        if (bridgeEvents == null) return;
        Gizmos.color = _isOnMission ? Color.yellow : Color.gray;
        Gizmos.DrawLine(transform.position, bridgeEvents.transform.position);
        Gizmos.DrawSphere(bridgeEvents.transform.position, 0.15f);
    }
}