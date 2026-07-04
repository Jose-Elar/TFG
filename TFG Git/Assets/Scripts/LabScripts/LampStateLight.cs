using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LampStateLight : MonoBehaviour
{
    public enum LightState { Base,Flickering, FullyOn, Alarm }

    [Header("Light Reference")]
    [SerializeField] private Light2D targetLight;   // ← añadido, asignar desde el Inspector

    [Header("State")]
    [SerializeField] private LightState currentState = LightState.Flickering;

    [Header("Flicker Settings (State 1)")]
    [SerializeField] private float flickerMin = 0.5f;
    [SerializeField] private float flickerMax = 0.9f;
    [SerializeField] private float flickerSpeed = 4f;

    [Header("Fully On Settings (State 2)")]
    [SerializeField] private float fullyOnIntensity = 0.9f;

    [Header("Alarm Settings (State 3)")]
    [SerializeField] private Color alarmColor = Color.red;
    [SerializeField] private float alarmMin = 0.6f;
    [SerializeField] private float alarmMax = 0.9f;
    [SerializeField] private float alarmPulseSpeed = 2f;

    private Color _originalColor;

    void Awake()
    {
        if (targetLight != null)
            _originalColor = targetLight.color;

    }

    void Update()
    {
        if (targetLight == null) return;

        switch (currentState)
        {
            case LightState.Flickering:
                UpdateFlickering();
                break;

            case LightState.FullyOn:
                UpdateFullyOn();
                break;

            case LightState.Alarm:
                UpdateAlarm();
                break;
            case LightState.Base:
                targetLight.intensity = 0f;
                break;
        }
    }

    // ── Estado 1 — Flicker aleatorio ───────────────────────────────
    private void UpdateFlickering()
    {
        targetLight.intensity = 0.98f;

        targetLight.color = _originalColor;
        targetLight.intensity = Mathf.Lerp(
            flickerMin,
            flickerMax,
            (Mathf.Sin(Time.time * flickerSpeed + Random.Range(-0.3f, 0.3f)) + 1f) / 2f
        );
    }

    // ── Estado 2 — Encendido fijo ───────────────────────────────────
    private void UpdateFullyOn()
    {
        targetLight.color = _originalColor;
        targetLight.intensity = fullyOnIntensity;
    }

    // ── Estado 3 — Alarma roja pulsante ─────────────────────────────
    private void UpdateAlarm()
    {
        targetLight.color = alarmColor;
        targetLight.intensity = Mathf.Lerp(
            alarmMin,
            alarmMax,
            (Mathf.Sin(Time.time * alarmPulseSpeed) + 1f) / 2f
        );
    }

    // ── Public API para cambiar de estado desde otros scripts ───────
    public void SetState(LightState newState)
    {
        currentState = newState;
    }
}