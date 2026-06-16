using UnityEngine;
using System.Collections;

public class DialogueTriggerZone : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private string dialogueId;

    [Header("References")]
    [SerializeField] private MovementBehaviour npc;

    private bool _triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Soot_Sprite")) return;

        _triggered = true;
        StartCoroutine(DialogueRoutine());
    }

    private IEnumerator DialogueRoutine()
    {
        // Stop NPC
        npc.enabled = false;

        // Wait one frame to make sure movement stops cleanly
        yield return null;

        // Subscribe and start dialogue
        bool dialogueDone = false;
        TextManager.Instance.OnDialogueEnded += () => dialogueDone = true;
        TextManager.Instance.StartDialogue(dialogueId);

        // Wait until text is fully finished
        yield return new WaitUntil(() => dialogueDone);

        // Re-enable NPC
        npc.enabled = true;

        Debug.Log("[DialogueTriggerZone] Dialogue done, NPC resumed.");
    }
}