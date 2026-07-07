using System.Collections;
using UnityEngine;

public class ButtonDrone : MonoBehaviour
{
    [SerializeField] private ButtonBehaviour button;


    [SerializeField] private float activationDistance = 1.5f;
    [SerializeField] private float activationDelay = 1f; 

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

        while (Vector2.Distance(transform.position, buttonPos) > activationDistance)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                buttonPos,
                wander.moveSpeed * Time.deltaTime
            );
            yield return null;
        }

    
        yield return new WaitForSeconds(activationDelay);
        
        button.SetActivated(true);

        wander.ResumeWander();
        isOnMission = false;
    }
}