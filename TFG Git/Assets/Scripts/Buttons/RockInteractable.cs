using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RockInteractable : MonoBehaviour
{
    [Header("Animation")]
    private Animator _animator;
    private static readonly string PARAM_DEACTIVATED = "_deactivated";

    [Header("Audio")]
    [SerializeField] private AudioSource earthRumbleSource;

    [Header("Lights")]
    [SerializeField] private Light2D idleLight;
    [SerializeField] private Light2D deactivatedLight;
    [SerializeField] private float   flickerDuration = 0.8f;
    [SerializeField] private int     flickerCount    = 6;

    [Header("Rock Tilemap")]
    [SerializeField] private GameObject rockTilemap;

    private bool _activated = false;

    void Awake()
    {
        _animator = GetComponent<Animator>();

        idleLight.enabled        = true;
        deactivatedLight.enabled = false;
    }

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
        StartCoroutine(DeactivateSequence());
    }

    private IEnumerator DeactivateSequence()
    {
        // Phase 1 — Earth rumble
        if (earthRumbleSource != null)
            earthRumbleSource.Play();

        // Phase 2 — Switch animation
        _animator.SetBool(PARAM_DEACTIVATED, true);

        // Phase 3 — Turn off idle light
        idleLight.enabled = false;

        // Phase 4 — Flicker deactivated light
        yield return StartCoroutine(FlickerLight(deactivatedLight));

        // Phase 5 — Stay on permanently
        deactivatedLight.enabled = true;

        // Phase 6 — Disable rocks
        if (rockTilemap != null)
            rockTilemap.SetActive(false);

        Debug.Log("[RockInteractable] Sequence complete.");
    }

    private IEnumerator FlickerLight(Light2D light2D)
    {
        light2D.enabled = true;

        for (int i = 0; i < flickerCount; i++)
        {
            light2D.enabled = !light2D.enabled;
            yield return new WaitForSeconds(flickerDuration / flickerCount);
        }

        light2D.enabled = true;
    }
}