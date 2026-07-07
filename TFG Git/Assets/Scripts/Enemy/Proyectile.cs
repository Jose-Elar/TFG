using UnityEngine;
 
public class Proyectile : MonoBehaviour
{
    private Vector2 _direction;
    private float   _speed;
    private float   _maxDistance;
    private Vector3 _startPosition;
    private bool    _initialized = false;
 

    public void Initialize(Vector2 direction, float speed, float maxDistance)
    {
        _direction     = direction;
        _speed         = speed;
        _maxDistance   = maxDistance;
        _startPosition = transform.position;
        _initialized   = true;
 
        transform.SetParent(null);
    }
 
    void Update()
    {
        if (!_initialized) return;
 
        transform.position += (Vector3)_direction * _speed * Time.deltaTime;
 
        if (Vector3.Distance(_startPosition, transform.position) >= _maxDistance)
        {
            Debug.Log("[Proyectile] Max distance reached — destroying.");
            Destroy(gameObject);
        }
    }
}
 