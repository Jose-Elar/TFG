using UnityEngine;

public class TestDialogue : MonoBehaviour
{
    private void OnTriggerEnter2D (Collider2D other)
    {
        if (other.CompareTag("Main_Drone"))
        {
            TextManager.Instance.StartDialogue("intro_message");
        }
    }
}
