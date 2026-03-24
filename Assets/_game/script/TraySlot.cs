using Unity.Mathematics;
using UnityEngine;

public class TraySlot : MonoBehaviour
{
    [Header("Cấu hình đích đến")]
    public Transform target;          // Kéo mục tiêu (điểm B) vào đây
    public float speed = 15f;         // Tốc độ bay
    public float rotationSpeed = 900f; // Tốc độ xoay (độ/giây)

    private bool isMoving = false;
    private Vector3 initialPosition;

    private quaternion initialRotation;
    private Vector3 initialScale;

    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialScale = transform.localScale;
    }

    void OnEnable()
    {

        isMoving = true;

    }
    void Update()
    {

        if (isMoving && target != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target.position,
                speed * Time.deltaTime
            );

            transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(110, 110, 110), Time.deltaTime * speed);
            if (Vector3.Distance(transform.position, target.position) < 0.4f)
            {
                OnReachedTarget();
            }

        }
    }

    void OnReachedTarget()
    {
        isMoving = false;
        this.gameObject.SetActive(false);
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        transform.localScale = initialScale;
    }
}
