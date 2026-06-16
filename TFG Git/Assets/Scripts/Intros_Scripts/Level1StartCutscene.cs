using System.Collections;
using UnityEngine;

public class LevelStartCutscene : MonoBehaviour
{
    [Header("Characters")]
    [SerializeField] private MovementBehaviour npc;
    [SerializeField] private DroneMovement     playerDrone;

    [Header("Assist Drones (children of NPC)")]
    [SerializeField] private GameObject assistDroneA;
    [SerializeField] private GameObject assistDroneB;

    [Header("Camera")]
    [SerializeField] private Camera        mainCamera;
    [SerializeField] private MonoBehaviour cameraScript;
    [SerializeField] private float         zoomedInSize  = 3f;
    [SerializeField] private float         zoomDuration  = 1.5f;

    [Header("Background")]
    [SerializeField] private DisableBackground background;

    [Header("Dialogue")]
    [SerializeField] private string introDialogueId = "intro_01";

    [Header("Timing")]
    [SerializeField] private float droneFlickerDelay = 0.3f;
    [SerializeField] private float flickerDuration   = 0.6f;

    private float _originalCameraSize;
    private float _originalCameraY;

    void Start()
    {
        _originalCameraSize         = mainCamera.orthographicSize;
        _originalCameraY            = mainCamera.transform.position.y;
        mainCamera.orthographicSize = zoomedInSize;

        StartCoroutine(CutsceneRoutine());
    }

    private IEnumerator CutsceneRoutine()
    {
        // ── Phase 1 — Lock everything ──────────────────────────────
        background.DisableParallax();
        npc.enabled                     = false;
        playerDrone.enabled             = false;
        playerDrone.gameObject.SetActive(false);
        assistDroneA.SetActive(false);
        assistDroneB.SetActive(false);
        cameraScript.enabled            = false;

        // ── Phase 2 — Snap camera to NPC ──────────────────────────
        mainCamera.transform.position = new Vector3(
            npc.transform.position.x,
            npc.transform.position.y,
            mainCamera.transform.position.z
        );

        yield return new WaitForSeconds(0.5f);

        // ── Phase 3 — NPC looks around ────────────────────────────
        yield return StartCoroutine(NPCLookAround());

        // ── Phase 4 — Start dialogue and WAIT for it to finish ────
        bool dialogueDone = false;
        TextManager.Instance.OnDialogueEnded += () => dialogueDone = true;
        TextManager.Instance.StartDialogue(introDialogueId);

        // Wait here until dialogue is fully finished
        yield return new WaitUntil(() => dialogueDone);

        // ── Phase 5 — Now zoom out AFTER dialogue finishes ────────
        yield return StartCoroutine(ZoomCameraAndRestoreY(zoomedInSize, _originalCameraSize, zoomDuration));

        background.EnableParallax();
        cameraScript.enabled = true;

        // ── Phase 6 — Flicker in drones ───────────────────────────
        yield return StartCoroutine(SpawnDronesAndStart());
    }

    private IEnumerator SpawnDronesAndStart()
    {
        // Flicker in assist drones
        yield return StartCoroutine(FlickerIn(assistDroneA));
        yield return new WaitForSeconds(droneFlickerDelay);
        yield return StartCoroutine(FlickerIn(assistDroneB));

        yield return new WaitForSeconds(droneFlickerDelay);

        // Flicker in player drone
        yield return StartCoroutine(FlickerIn(playerDrone.gameObject));

        yield return new WaitForSeconds(0.3f);

        // Unlock everything
        npc.enabled         = true;
        playerDrone.enabled = true;

        Debug.Log("[LevelStartCutscene] Cutscene complete — game started!");
    }

    // ── Flicker a GameObject in ───────────────────────────────────
    private IEnumerator FlickerIn(GameObject obj)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();

        obj.SetActive(true);

        if (sr != null)
        {
            int flickers = 5;
            for (int i = 0; i < flickers; i++)
            {
                sr.enabled = !sr.enabled;
                yield return new WaitForSeconds(flickerDuration / flickers);
            }
            sr.enabled = true;
        }

        yield return new WaitForSeconds(0.1f);
    }

    // ── Zoom and restore Y simultaneously ─────────────────────────
    private IEnumerator ZoomCameraAndRestoreY(float fromSize, float toSize, float duration)
    {
        float elapsed = 0f;
        float startY  = mainCamera.transform.position.y;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t  = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            mainCamera.orthographicSize   = Mathf.Lerp(fromSize, toSize, t);
            mainCamera.transform.position = new Vector3(
                mainCamera.transform.position.x,
                Mathf.Lerp(startY, _originalCameraY, t),
                mainCamera.transform.position.z
            );

            yield return null;
        }

        mainCamera.orthographicSize   = toSize;
        mainCamera.transform.position = new Vector3(
            mainCamera.transform.position.x,
            _originalCameraY,
            mainCamera.transform.position.z
        );
    }

    // ── NPC looks left then right ─────────────────────────────────
    private IEnumerator NPCLookAround()
    {
        SpriteRenderer npcSprite = npc.GetComponent<SpriteRenderer>();
        if (npcSprite == null) yield break;

        npcSprite.flipX = true;
        yield return new WaitForSeconds(0.6f);

        npcSprite.flipX = false;
        yield return new WaitForSeconds(0.6f);

        npcSprite.flipX = false;
    }
}