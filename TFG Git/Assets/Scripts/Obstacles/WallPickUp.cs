using UnityEngine;
 
public class WallPickUp : MonoBehaviour
{
    [SerializeField] private GameObject[] pickUps;
 
    [Header("Descend Settings")]
    [SerializeField] private float descendSpeed    = 2f;
    [SerializeField] private float descendDistance = 5f;
 
    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private bool    _isDescending = false;
    private bool    _triggered    = false;
 
    void Start()
    {
        _startPosition  = transform.position;
        _targetPosition = _startPosition + Vector3.down * descendDistance;
    }
 
    void Update()
    {
        // Check if all pickups have been removed from the scene
        if (!_triggered && AreAllPickUpsGone())
        {
            _triggered    = true;
            _isDescending = true;
        }
 
        if (!_isDescending) return;
 
        transform.position = Vector3.MoveTowards(
            transform.position,
            _targetPosition,
            descendSpeed * Time.deltaTime
        );
 
        if (Vector3.Distance(transform.position, _targetPosition) < 0.01f)
        {
            Destroy(gameObject);
        }
    }
 
    private bool AreAllPickUpsGone()
    {
        if (pickUps == null || pickUps.Length == 0) return false;
 
        foreach (GameObject pickUp in pickUps)
        {
            if (pickUp != null) return false; // at least one still exists
        }
 
        return true; // all are null (destroyed)
    }
}
 