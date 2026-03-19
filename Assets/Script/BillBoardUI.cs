using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    private Transform _mainCameraTransform;

    void Start()
    {
        if (Camera.main != null)
            _mainCameraTransform = Camera.main.transform;
    }
    
    void LateUpdate()
    {
        if (Camera.main == null) return;
        
        transform.rotation = Camera.main.transform.rotation;
    }
}