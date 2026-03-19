using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public LayerMask pigLayerMask;
    public Transform target;

    private bool canClick = false;

    void OnEnable()
    {
        EventManager.OnStartGame += EnableInput;
        EventManager.OnContinueGame += EnableInput;

        EventManager.OnLoseGame += DisableInput;
    }
    private void OnDisable()
    {
        EventManager.OnStartGame -= EnableInput;
        EventManager.OnContinueGame -= EnableInput;
        EventManager.OnLoseGame -= DisableInput;
    }

    private void DisableInput()
    {
        canClick = false;
    }

    private void EnableInput()
    {
        canClick = true;
    }

    void Update()
    {
        if (canClick && Input.GetMouseButtonDown(0))
        {
            HandleInput(Input.mousePosition);
        }
    }
    
    void HandleInput(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit hit;
        

        if (Physics.Raycast(ray, out hit, 100f, pigLayerMask))
        {
            PigComponent clickedPig = hit.collider.GetComponent<PigComponent>();

            if (clickedPig != null)
            {
                EventManager.OnClickPig?.Invoke(clickedPig);
            }
        }
    }
}
