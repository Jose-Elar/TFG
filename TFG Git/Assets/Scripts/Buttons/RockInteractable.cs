using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RockInteractable : MonoBehaviour
{
    [Header("Animation")]
    private Animator _computerAnimator;
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string   doorAnimationClipName = "Activate_Anim";
    private static readonly string PARAM_ACTIVATE1 = "_activate";
    private static readonly string PARAM_ACTIVATE2 = "_deactivated";

    [Header("Audio")]
    [SerializeField] private AudioSource gateMechanismSource;

    [Header("Lights")]
    [SerializeField] private Light2D idleLight;
    [SerializeField] private Light2D deactivatedLight;
    [SerializeField] private float   flickerDuration = 0.8f;
    [SerializeField] private int     flickerCount    = 6;

    [Header("Door Collider")]
    [SerializeField] private BoxCollider2D doorCollider;

    [Header("Extra Collider")]
    [SerializeField] private BoxCollider2D extraCollider;   // ← added

    private bool _activated = false;

    void Awake()
    {
        _computerAnimator = GetComponent<Animator>();

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
        StartCoroutine(ActivateSequence());
    }

    private IEnumerator ActivateSequence()
    {
        // Phase 1 — Gate mechanism sound
        if (gateMechanismSource != null)
            gateMechanismSource.Play();

        // Phase 2 — Switch both animations at the same time
        _computerAnimator.SetBool(PARAM_ACTIVATE2, true);

        if (doorAnimator != null)
            doorAnimator.SetBool(PARAM_ACTIVATE1, true);

        // Phase 2.5 — Disable door collider while it opens
        if (doorCollider != null)
            doorCollider.enabled = false;

        // Phase 2.6 — Disable extra collider immediately
        if (extraCollider != null)
            extraCollider.enabled = false;   // ← added

        // Phase 3 — Turn off idle light
        idleLight.enabled = false;

        // Phase 4 — Flicker deactivated light
        yield return StartCoroutine(FlickerLight(deactivatedLight));

        // Phase 5 — Stay on permanently
        deactivatedLight.enabled = true;

        // Phase 6 — Wait for door animation to finish, then re-enable collider
        if (doorAnimator != null && doorCollider != null)
        {
            float doorAnimLength = GetAnimationLength(doorAnimator, doorAnimationClipName);
            yield return new WaitForSeconds(doorAnimLength);
            doorCollider.enabled = true;
        }

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

    private float GetAnimationLength(Animator animator, string clipName)
    {
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
                return clip.length;
        }
        return 1f; // fallback if not found
    }
}