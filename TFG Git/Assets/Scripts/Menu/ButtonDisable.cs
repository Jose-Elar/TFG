using UnityEngine;

public class ButtonDisable : MonoBehaviour
{
    [SerializeField] private GameObject titleObject;

    public void Hide()
    {
        if (titleObject != null)
            titleObject.SetActive(false);

        gameObject.SetActive(false);
    }
}