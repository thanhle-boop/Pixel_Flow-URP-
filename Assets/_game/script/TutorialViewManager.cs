using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;

public class TutorialViewManager : MonoBehaviour, ICanvasRaycastFilter
{
    public Sprite hiddenSprite;
    public Sprite linkedSprite;
    public RectTransform target;
    public float radiusPadding = 40f;
    public float edgeSoftness = 0.005f;
    public Material _mat;
    private Canvas _canvas;
    private Camera _cam;

    private IDisposable _disposable;

    void Awake()
    {
        var image = GetComponent<Image>();
        // _mat = new Material(Shader.Find("UI/TutorialMask"));
        image.material = _mat;

        _canvas = GetComponentInParent<Canvas>();
        _cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : _canvas.worldCamera;

        EventManager.OnStartGame += HandleStartGame;
        EventManager.OnUseHand += ProcessUseHand;

        EventManager.OnEndHand += ProcessEndHand;

        EventManager.OnUseSuperCat += ProcessUseSuperCat;

        EventManager.OnClickBlock += ProcessClickBlock;

        // EventManager.oncompleteStep1Level1 += () =>
        // {
        //     if (LevelController.GetMaxLevelUnlock() == 1)
        //     {
        //         TutorialController.AdvanceStep(GuideTutorialType.Level_1.ToString());
        //         SetTargetZeroSize(); // Giữ
        //     }
        // };
        // EventManager.oncompleteStep2Level1 += () =>
        // {
        //     if (LevelController.GetMaxLevelUnlock() == 1)
        //     {
        //         TutorialController.AdvanceStep(GuideTutorialType.Level_1.ToString());
        //         SetTargetCustomSize(new Vector2(-317f, -359f), new Vector2(60, 80));
        //     }
        // };

        // EventManager.oncompleteStep3Level1 += () =>
        // {
        //     if (LevelController.GetMaxLevelUnlock() == 1)
        //     {
        //         TutorialController.AdvanceStep(GuideTutorialType.Level_1.ToString());
        //     }
        // };
        EventManager.closeClickBlockPopup += () =>
        {

            if (!string.IsNullOrEmpty(BoosterTutorialType.Booster_Super.ToString()) && !TutorialController.IsCompleted(BoosterTutorialType.Booster_Super.ToString()))
            {
                SetTargetFullSize();
                ExecuteTutorialLogic(LevelController.GetMaxLevelUnlock(), BoosterTutorialType.Booster_Super.ToString());
            }
        };


    }

    void OnDisable()
    {
        EventManager.OnStartGame -= HandleStartGame;
        EventManager.OnUseHand -= ProcessUseHand;
        EventManager.OnEndHand -= ProcessEndHand;
        EventManager.OnUseSuperCat -= ProcessUseSuperCat;
        EventManager.OnClickBlock -= ProcessClickBlock;
    }
    private void ProcessUseHand()
    {
        if (SceneGameplayUI.instance.currentTutorial == BoosterTutorialType.Booster_Balloon)
        {
            TutorialController.AdvanceStep(BoosterTutorialType.Booster_Balloon.ToString());
            SetTargetCustomSize(new Vector2(0.5f, -712f), new Vector2(400, 800));
        }
    }

    private void ProcessEndHand()
    {
        if (SceneGameplayUI.instance.currentTutorial != BoosterTutorialType.Booster_Balloon) return;
        TutorialController.AdvanceStep(BoosterTutorialType.Booster_Balloon.ToString());
        SceneGameplayUI.instance.ResetButton();

    }

    private void ProcessUseSuperCat()
    {
        if (SceneGameplayUI.instance.currentTutorial == BoosterTutorialType.Booster_Super)
        {
            PopupManager.instance.OpenPopup<PopupClickBlock>().Forget();
            TutorialController.AdvanceStep(BoosterTutorialType.Booster_Super.ToString());
            SetTargetCustomSize(new Vector2(0F, 300f), new Vector2(600, 800));
        }
    }

    private void ProcessClickBlock(string s)
    {
        if (SceneGameplayUI.instance.currentTutorial != BoosterTutorialType.Booster_Super) return;
        TutorialController.AdvanceStep(BoosterTutorialType.Booster_Super.ToString());
        SceneGameplayUI.instance.ResetButton();

        var popup = PopupManager.instance.GetPopup<PopupClickBlock>();

        if (popup != null)
        {
            PopupManager.instance.ClosePopup(popup, true);
        }
    }
    private void HandleStartGame()
    {
        int level = LevelController.GetMaxLevelUnlock();
        string tutorialKey = "";

        switch (level)
        {
            case 1: tutorialKey = GuideTutorialType.Level_1.ToString(); break;
            // case 2: tutorialKey = GuideTutorialType.Level_2.ToString(); break;
            case 6: tutorialKey = BoosterTutorialType.Booster_AddTray.ToString(); break;
            case 7: tutorialKey = MechanicTutorialType.Mechanic_Hidden.ToString(); break;
            case 12: tutorialKey = BoosterTutorialType.Booster_Balloon.ToString(); break;
            case 13: tutorialKey = MechanicTutorialType.Mechanic_Link.ToString(); break;
            case 15: tutorialKey = BoosterTutorialType.Booster_Shuffle.ToString(); break;
            case 18: tutorialKey = BoosterTutorialType.Booster_Super.ToString(); break;
        }

        if (!string.IsNullOrEmpty(tutorialKey))
        {
            if (!TutorialController.IsCompleted(tutorialKey))
            {
                // NẾU CHƯA HOÀN THÀNH: Reset về step 0 trước khi chạy logic
                TutorialController.ResetTutorialStep(tutorialKey);

                ExecuteTutorialLogic(level, tutorialKey);
            }
            else
            {
                SetTargetFullSize();
            }
        }
        else
        {
            SetTargetFullSize();
        }
    }

    private void ExecuteTutorialLogic(int level, string key)
    {
        switch (level)
        {
            case 1:
                WatchhStepChange(GuideTutorialType.Level_1.ToString());
                return; // Các bước sẽ được xử lý trong WatchhStepChange
            case 6:
                SetTargetZeroSize(); // Giữ màn hình tối để focus
                PopupManager.instance.OpenPopup<PopupAddTray>().Forget();
                SceneGameplayUI.instance.InTutorial(BoosterTutorialType.Booster_AddTray);
                break;
            case 7:
                SetTargetFullSize();
                PopupManager.instance.OpenPopup<PopupHiddenMechanic>().Forget();
                break;
            case 12:
                SetTargetZeroSize();
                PopupManager.instance.OpenPopup<PopupBalloon>().Forget();
                SceneGameplayUI.instance.InTutorial(BoosterTutorialType.Booster_Balloon);
                break;
            case 13:
                SetTargetFullSize();
                PopupManager.instance.OpenPopup<PopupLinkMechanic>().Forget();
                break;
            case 15:
                SetTargetZeroSize();
                PopupManager.instance.OpenPopup<PopupShuffle>().Forget();
                SceneGameplayUI.instance.InTutorial(BoosterTutorialType.Booster_Shuffle);
                break;
            case 18:
                SetTargetZeroSize();
                PopupManager.instance.OpenPopup<PopupSuperCat>().Forget();
                SceneGameplayUI.instance.InTutorial(BoosterTutorialType.Booster_Super);
                break;
        }

        WatchTutorialStatus(key);
    }
    void Update()
    {
        if (target == null) return;

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(_cam, target.position);
        Vector2 viewportPos = new Vector2(screenPos.x / Screen.width, screenPos.y / Screen.height);

        float halfWidth = (target.rect.width * target.lossyScale.x * 0.5f + radiusPadding) / Screen.width;
        float halfHeight = (target.rect.height * target.lossyScale.y * 0.5f + radiusPadding) / Screen.height;

        _mat.SetVector("_HoleCenter", new Vector4(viewportPos.x, viewportPos.y, 0, 0));
        _mat.SetVector("_HoleSize", new Vector4(halfWidth, halfHeight, 0, 0));
        _mat.SetFloat("_EdgeSoftness", edgeSoftness);
        if (!IsRaycastLocationValid(Input.mousePosition, _cam) && Input.GetMouseButtonDown(0))
        {
            Debug.Log("Click inside rounded rectangle!");
        }
    }
    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (target == null) return true;
        Vector2 targetScreenPos = RectTransformUtility.WorldToScreenPoint(_cam, target.position);

        float halfW = (target.rect.width * target.lossyScale.x * 0.5f) + radiusPadding;
        float halfH = (target.rect.height * target.lossyScale.y * 0.5f) + radiusPadding;
        bool isInside = screenPoint.x >= targetScreenPos.x - halfW &&
                        screenPoint.x <= targetScreenPos.x + halfW &&
                        screenPoint.y >= targetScreenPos.y - halfH &&
                        screenPoint.y <= targetScreenPos.y + halfH;
        return !isInside;
    }
    public void SetTargetFullSize()
    {
        if (target == null) return;
        target.anchorMin = Vector2.zero;
        target.anchorMax = Vector2.one;
        target.offsetMin = Vector2.zero;
        target.offsetMax = Vector2.one;
        target.localScale = Vector3.one;
    }

    public void SetTargetZeroSize()
    {
        if (target == null) return;

        target.sizeDelta = Vector2.zero;

        target.localScale = Vector3.zero;
        target.anchoredPosition = new Vector2(2000f, 2000f);
    }

    void OnDestroy()
    {
        // if (_mat != null) Destroy(_mat);
    }

    public void WatchTutorialStatus(string key)
    {
        _disposable?.Dispose();

        var isCompletedRx = TutorialController.IsCompletedRx(key);
        if (isCompletedRx == null) return;

        _disposable = isCompletedRx
            .Where(done => done == true)
            .Subscribe(_ =>
            {
                Debug.Log($"Tutorial {key} finished!");
                SetTargetFullSize();
            })
            .AddTo(this);
    }

    private void WatchhStepChange(string key)
    {
        _disposable?.Dispose();

        var item = TutorialController.GetTutorialItem(key);
        if (item == null) return;
        item.currentStep
            .Subscribe(step =>
            {
                Debug.Log($"Step changed: {step} for {key}");
                if (key == GuideTutorialType.Level_1.ToString())
                {
                    if (step == 0) SetTargetCustomSize(new Vector2(0.5f, -800f), new Vector2(250, 700));
                    else if (step == 1) SetTargetCustomSize(new Vector2(0F, 300f), new Vector2(800, 900));
                    else if (step == 2) SetTargetCustomSize(new Vector2(0f, -359f), new Vector2(700, 80));
                    // else SetTargetFullSize();
                }
            })
            .AddTo(this);

        // 2. Lắng nghe Hoàn thành (Thay thế cho WatchTutorialStatus)
        item.isCompleted
            .Where(done => done == true)
            .Subscribe(_ =>
            {
                Debug.Log($"Tutorial {key} finished!");
                SetTargetFullSize();
            })
            .AddTo(this);
    }

    public void SetTargetCustomSize(Vector2 position, Vector2 size)
    {
        if (target == null) return;

        target.anchorMin = new Vector2(0.5f, 0.5f);
        target.anchorMax = new Vector2(0.5f, 0.5f);

        target.pivot = new Vector2(0.5f, 0.5f);

        target.sizeDelta = size;

        target.anchoredPosition = position;
        target.localScale = Vector3.one;
    }

}
