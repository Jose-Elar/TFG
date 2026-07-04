using UnityEngine;
using TMPro;

public class TitleFlicker : MonoBehaviour
{
    [SerializeField] private TMP_Text flickerText;
    [SerializeField] private float flickerSpeed = 1f;

    void Update()
    {
        if (flickerText == null) return;

        float alpha = (Mathf.Sin(Time.time * flickerSpeed) + 1f) / 2f;
        Color c = flickerText.color;
        c.a = alpha;
        flickerText.color = c;
    }
}