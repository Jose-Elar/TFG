using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SavePointLight : MonoBehaviour
{
    public enum LightState { Deactivated, Flashing }

    [Header("Light Settings")]
    [SerializeField] private float flashMinIntensity = 0.3f;
    [SerializeField] private float flashMaxIntensity = 1.2f;
    [SerializeField] private float flashSpeed = 4f;

    [SerializeField]private Light2D _light;
    private LightState _currentState = LightState.Deactivated;

    void Awake()
    {
        ApplyState();
    }

    void Update()
    {
        if (_currentState == LightState.Flashing)
        {
            _light.intensity = Mathf.Lerp(
                flashMinIntensity,
                flashMaxIntensity,
                (Mathf.Sin(Time.time * flashSpeed) + 1f) / 2f
            );
        }
    }

    // ── Public API ────────────────────────────────────────────────
    public void SetState(LightState state)
    {
        _currentState = state;
        ApplyState();
    }

    private void ApplyState()
    {
        switch (_currentState)
        {
            case LightState.Deactivated:
                _light.enabled = false;
                break;

            case LightState.Flashing:
                _light.enabled = true;
                break;
        }
    }
}