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
        npc.enabled = false;


        yield return null;

  
        bool dialogueDone = false;
        TextManager.Instance.OnDialogueEnded += () => dialogueDone = true;
        TextManager.Instance.StartDialogue(dialogueId);


        yield return new WaitUntil(() => dialogueDone);

  
        npc.enabled = true;

        Debug.Log("[DialogueTriggerZone] Dialogue done, NPC resumed.");
    }
}