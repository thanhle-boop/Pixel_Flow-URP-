using System.Collections;
using MoreMountains.Tools;
using UnityEngine;

public class Block : MonoBehaviour
{
    public string color;
    public bool isAlreadyDestroyed = false;

    public MMTween.MMTweenCurve curve;
    public void StartDestroy()
    {
        StartCoroutine(DestroyCoroutine());
    }

    private IEnumerator DestroyCoroutine()
    {
        float durationUp = 0.2f;
        float durationDown = 0.2f;

        Vector3 startScale = Vector3.one;
        Vector3 maxScale = Vector3.one * 1.8f;
        Vector3 endScale = Vector3.zero;

        float elapsed = 0;
        while (elapsed < durationUp)
        {
            elapsed += Time.deltaTime;
            transform.localScale = MMTween.Tween(elapsed, 0f, durationUp, startScale, maxScale, MMTween.MMTweenCurve.EaseOutQuadratic);
            yield return null;
        }
        transform.localScale = maxScale;

        elapsed = 0;
        while (elapsed < durationDown)
        {
            elapsed += Time.deltaTime;
            transform.localScale = MMTween.Tween(elapsed, 0f, durationDown, maxScale, endScale, MMTween.MMTweenCurve.EaseInQuadratic);
            yield return null;
        }

        transform.localScale = endScale;
        Destroy(gameObject);

    }
}
