using UnityEngine;

[ExecuteInEditMode] // Cho phép script chạy ngay cả khi chưa bấm Play
public class ScalePlaneToCamera : MonoBehaviour
{
    public Camera orthoCamera;
    public bool isUnityPlane = true;
    public bool updateOnResize = true;

    private Vector2 lastScreenSize;

    void Start()
    {
        if (orthoCamera == null)
            orthoCamera = Camera.main;

        ScaleToFit();
        lastScreenSize = new Vector2(Screen.width, Screen.height);
    }

    void Update()
    {
        if (updateOnResize)
        {
            Vector2 currentScreenSize = new Vector2(Screen.width, Screen.height);
            if (currentScreenSize != lastScreenSize)
            {
                ScaleToFit();
                lastScreenSize = currentScreenSize;
            }
        }
    }

    [ContextMenu("Force Scale To Fit Now")] // Click chuột phải vào tên Script trong Inspector để chạy test nhanh
    public void ScaleToFit()
    {
        if (orthoCamera == null || !orthoCamera.orthographic)
        {
            return;
        }

        float worldScreenHeight = orthoCamera.orthographicSize * 2f;

        float worldScreenWidth = worldScreenHeight * orthoCamera.aspect;

        float divider = isUnityPlane ? 10f : 1f;

        // 4. Áp dụng Scale
        transform.localScale = new Vector3(worldScreenWidth / divider, 1f, worldScreenHeight / divider + 0.4f);


    }
}