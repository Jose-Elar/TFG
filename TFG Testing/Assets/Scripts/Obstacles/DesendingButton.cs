using UnityEngine;

public class DescendingButton : MonoBehaviour
{
    [Header("Movement")]
    public float descendSpeed;
 
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
   
        }
    }
 
    void OnTriggerExit2D(Collider2D other)
    {
        if (stopWhenExits && other.CompareTag("Soot_Sprite"))
        {
            _isDescending = false;
        }
    }
}