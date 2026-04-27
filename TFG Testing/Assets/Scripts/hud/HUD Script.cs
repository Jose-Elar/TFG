using System.Collections;
using TMPro;
using UnityEngine;

public class HUDScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonPressedText;
    [SerializeField] private TextMeshProUGUI canPressButtonText;


    public void pressedButton()
    {
        buttonPressedText.gameObject.SetActive(true);

        StartCoroutine(HideButtonText(2));
    }

    public void SetCanPressButton(bool state)
    {
        if (state)
        {
            canPressButtonText.gameObject.SetActive(state);
            StartCoroutine(HideButtonText(1));
        }
        else
        {
            canPressButtonText.gameObject.SetActive(state);
        }
    }


    IEnumerator HideButtonText(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        buttonPressedText.gameObject.SetActive(false);
    }
}
