using UnityEngine;
 
public class Proyectile : MonoBehaviour
{
    private Vector2 _direction;
    private float   _speed;
    private float   _maxDistance;
    private Vector3 _startPosition;
    private bool    _initialized = false;
 
    /// <summary>
    /// Called by EnemyBehaviour right after spawning.
    /// </summary>
    public void Initialize(Vector2 direction, float speed, float maxDistance)
    {
        _direction     = direction;
        _speed         = speed;
        _maxDistance   = maxDistance;
        _startPosition = transform.position;
        _initialized   = true;
 
        // Detach from enemy so it moves freely in the scene
        transform.SetParent(null);
    }
 
    void Update()
    {
        if (!_initialized) return;
 
        transform.position += (Vector3)_direction * _speed * Time.deltaTime;
 
        // Destroy after travelling max distance
        if (Vector3.Distance(_startPosition, transform.position) >= _maxDistance)
        {
            Debug.Log("[Proyectile] Max distance reached — destroying.");
            Destroy(gameObject);
        }
    }
}
 