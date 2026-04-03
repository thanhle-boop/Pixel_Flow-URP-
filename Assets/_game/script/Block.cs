using System.Collections;
using UnityEngine;

public class Block : MonoBehaviour
{
    public string color;
    public bool isAlreadyDestroyed = false;

    public void StartDestroy()
    {
        StartCoroutine(DestroyCoroutine());
    }
    private IEnumerator DestroyCoroutine()
    {
        float elapsed = 0;
        float durationUp = 0.2f;
        float durationDown = 0.2f;
        float totalDuration = durationUp + durationDown;

        Vector3 startScale = Vector3.one;
        Vector3 maxScale = Vector3.one * 1.8f;
        Vector3 endScale = Vector3.zero;

        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;

            if (elapsed <= durationUp)
            {
                float tUp = elapsed / durationUp;
                transform.localScale = Vector3.Lerp(startScale, maxScale, tUp);
            }
            else
            {
                float tDown = (elapsed - durationUp) / durationDown;
                transform.localScale = Vector3.Lerp(maxScale, endScale, tDown);
            }

            yield return new WaitForFixedUpdate();
        }

        transform.localScale = endScale;
        Destroy(gameObject);
    }
}
