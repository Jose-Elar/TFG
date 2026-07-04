using UnityEngine;

public class GravitiyMultiplier : MonoBehaviour
{
    [SerializeField] private Rigidbody2D targetRigidbody;
    [SerializeField] private float gravityMultiplier = 2f;   // ← añadido

    private float _originalGravityScale;
    private bool _hasDoubled = false;

    void Awake()
    {
        if (targetRigidbody != null)
            _originalGravityScale = targetRigidbody.gravityScale;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (targetRigidbody == null) return;
        if (_hasDoubled) return;

        if (!collision.gameObject.CompareTag("Soot_Sprite")) return;

        if (collision.rigidbody == targetRigidbody)
        {
            targetRigidbody.gravityScale = _originalGravityScale * gravityMultiplier;   // ← usa la variable
            _hasDoubled = true;
        }
    }
}
