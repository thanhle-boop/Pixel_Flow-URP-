using UnityEngine;

public class CameraController : MonoBehaviour
{
    public bool isTranslated = false;
    public bool pingpong = false;
    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.position;
        EventManager.OnHand += () =>
        {
            isTranslated = true;
        };
    }

    // Update is called once per frame
    void Update()
    {
        if (isTranslated)
        {
            if (pingpong)
            {
                MoveToPosition(initialPosition + new Vector3(0, 0, -0.82f));
            }
            else
            {
                MoveToPosition(initialPosition);
            }
        }
    }

    public void MoveToPosition(Vector3 targetPosition)
    {
        transform.transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);

        if(Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            transform.position = targetPosition;
            isTranslated = false;
            pingpong = !pingpong;
        }

    }
}
