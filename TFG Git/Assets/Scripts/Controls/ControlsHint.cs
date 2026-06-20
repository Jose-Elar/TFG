using UnityEngine;

public class ControlsHintTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private string dialogueId = "controls_hint";
    [SerializeField] private float delayBeforeShowing = 0.5f;

    public void StartHint()
    {
        Invoke(nameof(ShowHint), delayBeforeShowing);
    }

    private void ShowHint()
    {
        if (TextManager.Instance == null)
        {
            Debug.LogWarning("[ControlsHintTrigger] TextManager not found.");
            return;
        }

        TextManager.Instance.StartDialogue(dialogueId);
    }
}