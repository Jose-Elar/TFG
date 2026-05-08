using UnityEngine;

public class DescendingButton : MonoBehaviour
{
    [Header("Movement")]
    public float descendSpeed = 2f;
 
    public bool stopWhenExits = true;
 
    private bool _isDescending = false;
 
    void Update()
    {
        if (!_isDescending) return;
 
        transform.position += Vector3.down * descendSpeed * Time.deltaTime;
    }
 
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Soot_Sprite"))
        {
            _isDescending = true;
            Debug.Log("[DescendOnSootSprite] Soot_Sprite entered — descending.");
        }
    }
 
    void OnTriggerExit2D(Collider2D other)
    {
        if (stopWhenExits && other.CompareTag("Soot_Sprite"))
        {
            _isDescending = false;
            Debug.Log("[DescendOnSootSprite] Soot_Sprite exited — stopped.");
        }
    }
}