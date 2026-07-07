using UnityEngine;

public class RandomAmbientSound : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Randomness")]
    [Tooltip("Probability (0 to 1) of playing the sound each check.")]
    [SerializeField] private float playChance = 0.3f;

    [Tooltip("How often (in seconds) to roll the chance.")]
    [SerializeField] private float checkInterval = 1f;

    [Header("Pitch Variation")]
    [SerializeField] private float pitchMin = 0.9f;
    [SerializeField] private float pitchMax = 1.1f;

    private float _timer = 0f;

    void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= checkInterval)
        {
            _timer = 0f;
            TryPlaySound();
        }
    }

    private void TryPlaySound()
    {
        if (audioSource.isPlaying) return;

        // Roll the dice
        if (Random.value <= playChance)
        {
            audioSource.pitch = Random.Range(pitchMin, pitchMax);
            audioSource.Play();
        }
    }
}