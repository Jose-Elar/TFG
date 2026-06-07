using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Drag the PlayerDrone GameObject here.")]
    [SerializeField] private Transform player;

    [Header("Shooting")]
    [Tooltip("The Proyectile prefab to shoot.")]
    [SerializeField] private GameObject proyectilePrefab;

    [Tooltip("Speed of the projectile.")]
    [SerializeField] private float proyectileSpeed = 3f;

    [Tooltip("Seconds between each shot.")]
    [SerializeField] private float shootInterval = 3f;

    [Tooltip("Max distance the projectile travels before being destroyed.")]
    [SerializeField] private float proyectileMaxDistance = 20f;

    [Tooltip("How far from the enemy center the projectile spawns.")]
    [SerializeField] private float spawnOffset = 0.8f;

    [Header("Activation")]
    [Tooltip("Distance from the enemy at which shooting activates.")]
    [SerializeField] private float activationDistance = 15f;

    private float _shootTimer = 0f;
    private bool  _playerInRange = false;
    private SpriteRenderer _sprite;              // ← added

    void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>(); // ← added
    }

    void Update()
    {
        if (player == null) return;

        // Flip sprite to face player                          ← added
        if (_sprite != null)
            _sprite.flipX = player.position.x < transform.position.x;

        float distance = Vector2.Distance(transform.position, player.position);
        _playerInRange = distance <= activationDistance;

        if (!_playerInRange)
        {
            _shootTimer = 0f;
            return;
        }

        _shootTimer += Time.deltaTime;
        if (_shootTimer >= shootInterval)
        {
            Shoot();
            _shootTimer = 0f;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Main_Drone"))
        {
            Debug.Log("[EnemyBehaviour] Collided with player. Destroying self.");
            Destroy(gameObject);
        }
    }

    private void Shoot()
    {
        if (proyectilePrefab == null) return;

        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        Vector3 spawnPosition = transform.position + (Vector3)(direction * spawnOffset);

        GameObject proj = Instantiate(proyectilePrefab, spawnPosition, Quaternion.identity, transform);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        proj.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);

        Proyectile tracker = proj.GetComponent<Proyectile>();
        if (tracker == null)
            tracker = proj.AddComponent<Proyectile>();

        tracker.Initialize(direction, proyectileSpeed, proyectileMaxDistance);

        Debug.Log("[EnemyBehaviour] Shot projectile towards player.");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = _playerInRange ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationDistance);
    }
}