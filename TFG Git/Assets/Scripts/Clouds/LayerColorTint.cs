using UnityEngine;

public class LayerColorTint : MonoBehaviour
{
    [SerializeField] private Color tintColor = Color.white;

    void Awake()
    {
        ApplyTint();
    }

    public void ApplyTint()
    {
        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sr in sprites)
            sr.color = tintColor;
    }

    // Lets you preview in editor without hitting play
    void OnValidate() => ApplyTint();
}