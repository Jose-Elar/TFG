using UnityEngine;

public class PickUpDelete : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Main_Drone"))
        {
            Destroy(gameObject);
        }
    }
}
