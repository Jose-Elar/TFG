using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public float moveDistance = 30f;
    public float moveDuration = 2f;
    public float delayBeforeMove = 1f;

    private bool isRunning = false;

    public void StartCameraSequence()
    {
        if (!isRunning)
            StartCoroutine(CameraSequence());
    }

    IEnumerator CameraSequence()
    {
        isRunning = true;


        yield return new WaitForSeconds(delayBeforeMove);


        Time.timeScale = 0f;

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + new Vector3(moveDistance, 0f, 0f);

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / moveDuration;

            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;

        // ▶️ Resume game
        Time.timeScale = 1f;

        isRunning = false;
    }
}