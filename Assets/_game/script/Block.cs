using System.Collections;
using MoreMountains.Tools;
using UnityEngine;

public class Block : MonoBehaviour
{
    public string color;
    public bool isAlreadyDestroyed = false;

    public MMTween.MMTweenCurve curve;
    public GameObject model;
    public void StartDestroy()
    {
        if(model == null)
        {
            Debug.Log("Model is not assigned for " + gameObject.name);
            return;
        }   
        StartCoroutine(DestroyCoroutine());
    }

    private IEnumerator DestroyCoroutine()
    {
        float durationUp = 0.2f;
        float durationDown = 0.2f;

        Vector3 startScale = model.transform.localScale;
        Vector3 maxScale = startScale * 1.5f;
        Vector3 endScale = Vector3.zero;

        float elapsed = 0;
        while (elapsed < durationUp)
        {
            elapsed += Time.deltaTime;
            model.transform.localScale = MMTween.Tween(elapsed, 0f, durationUp, startScale, maxScale, MMTween.MMTweenCurve.EaseOutQuadratic);
            yield return null;
        }
        model.transform.localScale = maxScale;

        elapsed = 0;
        while (elapsed < durationDown)
        {
            elapsed += Time.deltaTime;
            model.transform.localScale = MMTween.Tween(elapsed, 0f, durationDown, maxScale, endScale, MMTween.MMTweenCurve.EaseInQuadratic);
            yield return null;
        }

        model.transform.localScale = endScale;
        Destroy(gameObject);

    }
}
