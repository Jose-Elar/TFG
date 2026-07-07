using UnityEngine;

public class DroneHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHp = 3;

    [Header("Respawn")]
    [Tooltip("The save point to return to when hit. Assign later.")]
    [SerializeField] private MovementBehaviour movementBehScript;



    private int _currentHp;
    private Rigidbody2D _rb;

    void Awake()
    {
        _rb        = GetComponent<Rigidbody2D>();
        _currentHp = maxHp;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Proyectile")) return;
        Destroy(collision.gameObject); 
        TakeHit();
    }

    private void TakeHit()
    {
        _currentHp--;
        Debug.Log($"[DroneHealth] Hit! HP remaining: {_currentHp}");

        if (_currentHp <= 0)
        {
            GameOver();
            return;
        }

        Respawn();
    }

    private void Respawn()
    {
       
        if (_rb != null)
            _rb.linearVelocity = Vector2.zero;

        transform.position = movementBehScript.GetLastCheckpointPosition();
        Debug.Log($"[DroneHealth] Respawned at save point: {movementBehScript.GetLastCheckpointPosition()}");
    }

    private void GameOver()
    {
        Debug.Log("[DroneHealth] HP reached 0 — Game Over.");
        Time.timeScale = 0f;
        Application.Quit();
    }

    public int getCurrentHp()
    {
        return _currentHp;
    }   
}