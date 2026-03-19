using System.Collections;
using UnityEngine;

public class FlashObject : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private float flashSpeed = 3f;
    [SerializeField] private float minAlpha = 0f;
    [SerializeField] private float maxAlpha = 0.8f;
    [SerializeField] private float flashDuration = 2f;

    private Renderer objectRenderer;
    private Material objectMaterial;

    private bool isFlashing = false;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        objectRenderer = GetComponent<MeshRenderer>();
        objectMaterial = objectRenderer.material;
    }

    private void OnEnable()
    {
        EventManager.OnStartGame += ResetFlash;
        EventManager.OnQueueFull += StartFlashing;
        EventManager.OnQueueNotFull += StopFlashing;
    }

    private void OnDisable()
    {
        EventManager.OnStartGame -= ResetFlash;
        EventManager.OnQueueFull -= StartFlashing;
        EventManager.OnQueueNotFull -= StopFlashing;
    }

    private void ResetFlash()
    {
        StopFlashing();
        if (objectMaterial != null)
        {
            Color currentColor = objectMaterial.GetColor("_BaseColor");
            currentColor.a = 0f;
            objectMaterial.SetColor("_BaseColor", currentColor);
        }
    }

    private void StartFlashing()
    {
        if (!isFlashing)
        {
            isFlashing = true;

            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }

            flashCoroutine = StartCoroutine(FlashCoroutine());
        }
    }

    private void StopFlashing()
    {
        isFlashing = false;
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        if (objectRenderer != null)
        {
            Color currentColor = objectMaterial.GetColor("_BaseColor");
            currentColor.a = 0f;
            objectMaterial.SetColor("_BaseColor", currentColor);
        }
    }

    private IEnumerator FlashCoroutine()
    {
        float elapsedTime = 0f;
        float totalElapsed = 0f;

        while (isFlashing && totalElapsed < flashDuration)
        {
            elapsedTime += Time.deltaTime * flashSpeed;
            totalElapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, Mathf.PingPong(elapsedTime, 1f));
            if (objectMaterial != null)
            {
                Color newColor = objectMaterial.GetColor("_BaseColor");
                newColor.a = alpha;
                objectMaterial.SetColor("_BaseColor", newColor);
            }
            yield return null;
        }

        StopFlashing();
    }
}

