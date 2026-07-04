using UnityEngine;

public class LabConsoleInteractable : MonoBehaviour, IInteractable
{
    [Header("Walker Reference")]
    [SerializeField] private MovementBehaviour walkerMovement;

    [Header("Dialogue")]
    [SerializeField] private string arrivalDialogueId;     // primer texto, al llegar el Walker
    [SerializeField] private string activationDialogueId;  // segundo texto, al activar la consola

    [Header("Audio")]
    [SerializeField] private AudioSource lightsOnAudioSource;
    [SerializeField] private AudioSource lightsFlickerAudioSource;

    [Header("Lamps to Activate")]
    [SerializeField] private LampStateLight[] lampsToActivate;   // ← añadido

    private bool _walkerArrived  = false;
    private bool _activated      = false;

    // ── Detección del Walker llegando a la zona ─────────────────────
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!_walkerArrived && other.gameObject == walkerMovement.gameObject)
        {
            _walkerArrived = true;

            walkerMovement.enabled = false;

            if (!string.IsNullOrEmpty(arrivalDialogueId) && TextManager.Instance != null)
                TextManager.Instance.StartDialogue(arrivalDialogueId);

            return;
        }

        if (other.CompareTag("Main_Drone"))
        {
            DroneMovement drone = other.GetComponent<DroneMovement>();
            drone?.SetNearbyInteractable(this);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Main_Drone")) return;

        DroneMovement drone = other.GetComponent<DroneMovement>();
        drone?.SetNearbyInteractable(null);
    }

    // ── Llamado por DroneMovement al pulsar E ───────────────────────
    public void Activate()
    {
        if (_activated) return;
        if (!_walkerArrived) return;

        _activated = true;

        if (lightsOnAudioSource != null && lightsFlickerAudioSource != null)
        {
            lightsFlickerAudioSource.Stop();
            lightsOnAudioSource.Play();

        }


        // ── Activar las lámparas y pasarlas a FullyOn ────────────────
        foreach (LampStateLight lamp in lampsToActivate)
        {
            if (lamp != null)
            {
                lamp.enabled = true;
                lamp.SetState(LampStateLight.LightState.FullyOn);
            }
        }

        if (!string.IsNullOrEmpty(activationDialogueId) && TextManager.Instance != null)
        {
            TextManager.Instance.OnDialogueEnded += OnActivationDialogueEnded;
            TextManager.Instance.StartDialogue(activationDialogueId);
        }
        else
        {
            ResumeWalker();
        }

        Debug.Log("[LabConsoleInteractable] Console activated.");
    }

    private void OnActivationDialogueEnded()
    {
        TextManager.Instance.OnDialogueEnded -= OnActivationDialogueEnded;
        ResumeWalker();
    }

    private void ResumeWalker()
    {
        if (walkerMovement != null)
            walkerMovement.enabled = true;

        Debug.Log("[LabConsoleInteractable] Walker resumed movement.");
    }
}