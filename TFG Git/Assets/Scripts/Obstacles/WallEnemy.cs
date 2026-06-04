using UnityEngine;
 
public class WallEnemy : MonoBehaviour
{
    [Header("Wall Movement")]
    [Tooltip("How fast the wall moves down.")]
    [SerializeField] private float descendSpeed ;
 
    [Tooltip("How far down the wall moves before stopping.")]
    [SerializeField] private float descendDistance; 
 
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
        if (_isDescending)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                _targetPosition,
                descendSpeed * Time.deltaTime
            );
 
            if (Vector3.Distance(transform.position, _targetPosition) < 0.01f)
            {
                transform.position = _targetPosition;
                _isDescending      = false;
                Debug.Log("[WallEnemy] Wall fully descended.");
            }
            return;
        }
 
        if (_triggered) return;
 
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0)
        {
            Debug.Log("[WallEnemy] All enemies gone — wall descending.");
            _triggered    = true;
            _isDescending = true;
        }
    }
}
 