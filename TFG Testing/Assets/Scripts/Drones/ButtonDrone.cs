using System.Collections;
using UnityEngine;

public class ButtonDrone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ButtonBehaviour button;

    [Header("Behaviour")]
    [SerializeField] private float activationDistance = 1.5f;
    [SerializeField] private float activationDelay = 1f; // ← delay before firing event

    private WanderStateDrones wander;
    private bool isOnMission = false;

    void Awake()
    {
        wander = GetComponent<WanderStateDrones>();
    }

    void OnEnable()
    {
        button.OnButtonPressed += OnButtonPressed;
    }

    void OnDisable()
    {
        button.OnButtonPressed -= OnButtonPressed;
    }

    private void OnButtonPressed()
    {
        if (isOnMission) return;
        StartCoroutine(GoToButton());
    }

    IEnumerator GoToButton()
    {
        isOnMission = true;
        wander.PauseWander();

        Vector2 buttonPos = button.transform.position;

        // Move towards button
        while (Vector2.Distance(transform.position, buttonPos) > activationDistance)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                buttonPos,
                wander.moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        // Drone is close — wait before activating
        yield return new WaitForSeconds(activationDelay);

        // Now activate — color stays permanently since isActivated blocks future presses
        button.SetActivated(true);

        // Resume wander
        wander.ResumeWander();
        isOnMission = false;
    }
}