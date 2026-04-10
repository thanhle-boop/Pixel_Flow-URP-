using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public LayerMask pigLayerMask;
    public LayerMask blockLayerMask;

    // private bool canClick = false;
    private bool isUseSuperCat = false;

    void OnEnable()
    {
        EventManager.OnUseSuperCat += UseSuperCat;
    }

    private void OnDisable()
    {
        EventManager.OnUseSuperCat -= UseSuperCat;
    }

    public void UseSuperCat()
    {
        isUseSuperCat = true;
    }
    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {

        if (Input.GetMouseButtonDown(0))
        {

            if (StaticUtils.IsClickOnUI(UIManager.Instance.eventSystem, Input.mousePosition, UIManager.Instance.tagValueTypes))
            {
                Debug.Log("Clicked on UI, ignoring pig click.");
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, pigLayerMask))
            {
                PigComponent clickedPig = hit.collider.GetComponent<PigComponent>();
                if (clickedPig != null)
                {
                    EventManager.OnClickPig?.Invoke(clickedPig);
                    // Debug.Log("Clicked on pig: ");

                }
            }
            if (Physics.Raycast(ray, out hit, 100f, blockLayerMask) && isUseSuperCat)
            {

                Block clickedBlock = hit.collider.GetComponent<Block>();
                if (clickedBlock != null)
                {
                    EventManager.OnClickBlock?.Invoke(clickedBlock.color);
                    Debug.Log("Clicked on block with color: " + clickedBlock.color);
                    isUseSuperCat = false;
                }

            }
        }
    }
}
