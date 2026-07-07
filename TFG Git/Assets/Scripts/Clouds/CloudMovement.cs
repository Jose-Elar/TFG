using UnityEngine;

public class CloudMovement : MonoBehaviour
{
    [SerializeField] private float speed = 0.5f;
    [SerializeField] private float resetDelay = 2f;       
    [SerializeField] private float randomYMin = -2f;      
    [SerializeField] private float randomYMax = 2f;       

    private Camera _cam;
    private float _screenRightEdge;
    private float _screenLeftEdge;
    private float _offScreenTimer = 0f;
    private bool _isOffScreen = false;

    void Start()
    {
        _cam = Camera.main;
        UpdateScreenBounds();
    }

    void Update()
    {
        transform.position += new Vector3(speed * Time.deltaTime, 0, 0);

        UpdateScreenBounds();
        if (transform.position.x > _screenRightEdge + 2f)
        {
            if (!_isOffScreen)
            {
                _isOffScreen = true;
                _offScreenTimer = 0f;
            }

            _offScreenTimer += Time.deltaTime;

            if (_offScreenTimer >= resetDelay)
                ResetCloud();
        }
    }

    private void ResetCloud()
    {
        // Teleport to left of screen with random Y
        float newX = _screenLeftEdge - Random.Range(1f, 4f);
        float newY = Random.Range(randomYMin, randomYMax);

        transform.position = new Vector3(newX, newY, transform.position.z);

        _isOffScreen = false;
        _offScreenTimer = 0f;
    }

    private void UpdateScreenBounds()
    {
        if (_cam == null) return;
        float halfW = _cam.orthographicSize * _cam.aspect;
        _screenRightEdge = _cam.transform.position.x + halfW;
        _screenLeftEdge  = _cam.transform.position.x - halfW;
    }
}