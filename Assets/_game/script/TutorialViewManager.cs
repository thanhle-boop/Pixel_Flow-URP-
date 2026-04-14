using UnityEngine;
using UnityEngine.UI;

public class TutorialViewManager : MonoBehaviour, ICanvasRaycastFilter
{
    [Header("Hole Settings")]
    [Tooltip("The RectTransform to spotlight (e.g. a button)")]
    public RectTransform target;

    [Tooltip("Extra radius padding around the target (pixels)")]
    public float radiusPadding = 40f;

    [Tooltip("Softness of the hole edge")]
    public float edgeSoftness = 0.005f;

    public Material _mat;
    private Canvas _canvas;
    private Camera _cam;

    void Awake()
    {
        var image = GetComponent<Image>();
        // Create a material instance so we don't modify the shared asset
        _mat = new Material(Shader.Find("UI/TutorialMask"));
        image.material = _mat;

        _canvas = GetComponentInParent<Canvas>();
        _cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : _canvas.worldCamera;
    }

    void Update()
    {
        if (target == null)
        {
            Debug.LogWarning("[TutorialOverlay] target is NULL!");
            return;
        }

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(_cam, target.position);
        Vector2 viewportPos = new Vector2(screenPos.x / Screen.width, screenPos.y / Screen.height);

        float worldRadius = Mathf.Max(target.rect.width, target.rect.height) * 0.5f
                            * target.lossyScale.x + radiusPadding;
        float uvRadius = worldRadius / Screen.height;

        _mat.SetVector("_HoleCenter", new Vector4(viewportPos.x, viewportPos.y, 0, 0));
        _mat.SetFloat("_HoleRadius", uvRadius);
        _mat.SetFloat("_EdgeSoftness", edgeSoftness);

        if (!IsRaycastLocationValid(Input.mousePosition, _cam) && Input.GetMouseButtonDown(0))
        {
            Debug.Log("congthah");
        }
        //  Debug.Log($"[TutorialOverlay] target={target.name} | screenPos={screenPos} | viewportPos={viewportPos} | worldRadius={worldRadius} | uvRadius={uvRadius}");
    }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (target == null) return true;

        Vector2 targetScreenPos = RectTransformUtility.WorldToScreenPoint(_cam, target.position);
        float worldRadius = Mathf.Max(target.rect.width, target.rect.height) * 0.5f
                            * target.lossyScale.x + radiusPadding;

        float dist = Vector2.Distance(screenPoint, targetScreenPos);
        bool isBlocked = dist > worldRadius;

        // Debug.Log($"[TutorialOverlay] Click at {screenPoint} | targetScreen={targetScreenPos} | dist={dist:F1} | radius={worldRadius:F1} | blocked={isBlocked}");

        return isBlocked;
    }

    void OnDestroy()
    {
        if (_mat != null) Destroy(_mat);
    }

}
