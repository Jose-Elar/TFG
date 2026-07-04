using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PadInteractable : MonoBehaviour, IInteractable
{
    [Header("Extra Light")]
    [SerializeField] private Light2D doorLight;
    [SerializeField] private Light2D globalLight;
    [SerializeField] private Light2D droneLight;


    [Header("Door")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private BoxCollider2D doorCollider;
    private static readonly string PARAM_DEACTIVATED = "_deactivated";

    [Header("Audio Sources to Stop")]
    [SerializeField] private AudioSource[] audioSourcesToStop;

    [Header("Audio Sources to Play")]
    [SerializeField] private AudioSource audioSourceToPlay;

    [Header("Lamps to Activate")]
    [SerializeField] private LampStateLight[] lampsToActivate;

    private bool _activated = false;

    // ── Proximity detection ───────────────────────────────────────
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Main_Drone")) return;
        DroneMovement drone = other.GetComponent<DroneMovement>();
        drone?.SetNearbyInteractable(this);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Main_Drone")) return;
        DroneMovement drone = other.GetComponent<DroneMovement>();
        drone?.SetNearbyInteractable(null);
    }

    // ── Called by DroneMovement on input ─────────────────────────
    public void Activate()
    {
        if (_activated) return;
        _activated = true;

        if (doorAnimator != null)
            doorAnimator.SetBool(PARAM_DEACTIVATED, true);

        if (doorCollider != null)
            doorCollider.enabled = false;

        if (doorLight != null)
            doorLight.enabled = false;

        // ── Detener completamente todos los audio sources indicados ──
        foreach (AudioSource source in audioSourcesToStop)
        {
            if (source != null)
                source.Stop();
        }

        // ── Activar el script de cada lámpara y pasarlas a Flickering ──
        foreach (LampStateLight lamp in lampsToActivate)
        {
            if (lamp != null)
            {
                lamp.enabled = true;
                lamp.SetState(LampStateLight.LightState.Flickering);
            }
        }

        if (audioSourceToPlay != null)
            audioSourceToPlay.Play();

        //if (globalLight != null) globalLight.intensity = 0f;

        if (droneLight != null) droneLight.intensity = 0.1f;

        Debug.Log("[PadInteractable] Door deactivated, audio stopped, lamps activated.");
    }
}