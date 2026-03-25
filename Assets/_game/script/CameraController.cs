using UnityEngine;
public class CameraController : MonoBehaviour
{
    public bool isTranslated = false;
    private Vector3 initialPosition;
    private Vector3 targetPosition;

    private void Start() // Dùng Start thay vì Awake để các object khác ổn định vị trí đã
    {
        initialPosition = transform.position;

        EventManager.OnUseHand += () =>
        {
            if (isTranslated) return;

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                
            }

            targetPosition = initialPosition + new Vector3(0, 0, -0.82f);
            isTranslated = true;
        };

        EventManager.OnEndHand += () =>
        {
            targetPosition = initialPosition;
            isTranslated = true;
        };
    }

    void Update()
    {
        if (isTranslated)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isTranslated = false;
            }
        }
    }
}