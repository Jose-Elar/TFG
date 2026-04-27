using System.Collections;
using UnityEngine;

public class WallBehaviour : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ButtonBehaviour button;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float disappearDistance = 5f; // how far down before deactivating

    private Vector2 startPosition;
    private bool isMoving = false;

    void Awake()
    {
        startPosition = transform.position;
    }

    void OnEnable()
    {
        button.OnButtonActivated += OnButtonActivated;
    }

    void OnDisable()
    {
        button.OnButtonActivated -= OnButtonActivated;
    }

    private void OnButtonActivated()
    {
        if (isMoving) return;
        StartCoroutine(SlideDown());
    }

    IEnumerator SlideDown()
    {
        isMoving = true;

        Vector2 targetPosition = startPosition + Vector2.down * disappearDistance;

        while (Vector2.Distance(transform.position, targetPosition) > 0.05f)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        gameObject.SetActive(false);
    }
}