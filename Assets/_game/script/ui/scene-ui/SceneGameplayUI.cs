using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SceneGameplayUI : SingletonMonoBehaviour<SceneGameplayUI>
{
    [SerializeField] Button btnAddTray;
    [SerializeField] Button btnHand;
    [SerializeField] Button btnUseShuff;
    [SerializeField] Button btnUseSuper;
    [SerializeField] Button btnSetting;

    [SerializeField] TextMeshProUGUI coinText;
    [SerializeField] TextMeshProUGUI levelText;

    [SerializeField] RectTransform rectBottomUI;
    public BoosterTutorialType currentTutorial = BoosterTutorialType.None;

    public bool isBottomUiTranslate = false;

    private void OnEnable()
    {
        EventManager.OnLoseGame += GameOver;
        EventManager.OnStartGame += StartGame;
        EventManager.OnFullConveyorSlot += OnInvalidExecution;
        EventManager.OnWinGame += WinGame;
        EventManager.OnEndHand += OnEndHand;
    }

    private void OnDisable()
    {
        EventManager.OnLoseGame -= GameOver;
        EventManager.OnStartGame -= StartGame;
        EventManager.OnFullConveyorSlot -= OnInvalidExecution;
        EventManager.OnWinGame -= WinGame;
        EventManager.OnEndHand -= OnEndHand;
    }

    private void Start()
    {
        AudioController.instance.PlayMusic(AudioIndex.bgm_gameplay.ToString());

        btnSetting.OnClickAsObservable()
            .Subscribe(_ =>
            {
                PopupManager.instance.OpenPopup<PopupGameplaySettings>().Forget();
            }).AddTo(this);
    }

    IEnumerator MoveUISmoothly(RectTransform uiRect, Vector2 target)
    {
        float speed = 10f;

        while (Vector2.Distance(uiRect.anchoredPosition, target) > 0.1f)
        {
            uiRect.anchoredPosition = Vector2.Lerp(uiRect.anchoredPosition, target, Time.deltaTime * speed);

            yield return null;
        }

        uiRect.anchoredPosition = target;
        isBottomUiTranslate = false;
    }

    private void StartGame()
    {
        levelText.text = "Level " + LevelController.GetMaxLevelUnlock();
        var gold = CurrencyController.GetGold();
        coinText.text = gold < 0 ? "0" : gold.ToString();
    }

    public void HandleBoosterHand()
    {
        Vector2 targetPos = rectBottomUI.anchoredPosition + new Vector2(0, -230f);
        isBottomUiTranslate = true;
        StartCoroutine(MoveUISmoothly(rectBottomUI, targetPos));
    }

    private void OnInvalidExecution()
    {
        UIManager.Instance.straightSlotText.GetComponent<Animator>().SetTrigger("IsInvalid");
    }

    private void OnEndHand()
    {
        isBottomUiTranslate = true;
        if (rectBottomUI != null)
        {
            StartCoroutine(MoveUISmoothly(rectBottomUI, new Vector2(0, 0f)));
        }
    }

    private void WinGame()
    {
        PopupManager.instance.OpenPopup<PopupWin>().Forget();
    }

    private void GameOver()
    {
        PopupManager.instance.OpenPopup<PopupGameOver>().Forget();
    }
    private Button btn;
    public void InTutorial(BoosterTutorialType key)
    {
        btn = btnUseSuper;
        switch (key)
        {
            case BoosterTutorialType.Booster_Super:
                btn = btnUseSuper;
                currentTutorial = key;
                break;

            case BoosterTutorialType.Booster_Shuffle:
                btn = btnUseShuff;
                currentTutorial = key;
                break;
            case BoosterTutorialType.Booster_Balloon:
                btn = btnHand;
                currentTutorial = key;
                break;
            case BoosterTutorialType.Booster_AddTray:
                btn = btnAddTray;
                currentTutorial = key;
                break;

            default:
                break;
        }
        var targetImage = btn.GetComponent<Image>();
        var icon = btn.transform.GetChild(1).GetComponent<Image>();
        icon.DOKill();
        targetImage.DOKill();
        btn.transform.DOKill();

        btn.transform.DOScale(1.25f, 0.1f).OnComplete(() =>
        {
            targetImage.DOFade(0.2f, 1f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

            icon.DOFade(0.2f, 1f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
        });

    }
    public void CompleteFirstStepTutorial(BoosterTutorialType key)
    {
        if (currentTutorial == BoosterTutorialType.None) return;

        // 2. Nếu cái hoàn thành không phải cái đang chạy thì cũng biến đi
        if (currentTutorial != key) return;

        TutorialController.AdvanceStep(currentTutorial.ToString());
        ResetButton();
        currentTutorial = BoosterTutorialType.None;
    }
    public void ResetButton()
    {
        var targetImage = btn.GetComponent<Image>();
        var icon = btn.transform.GetChild(1).GetComponent<Image>();
        icon.DOKill();
        targetImage.DOKill();
        btn.transform.DOKill();

        targetImage.color = new Color(targetImage.color.r, targetImage.color.g, targetImage.color.b, 1f);
        icon.color = new Color(icon.color.r, icon.color.g, icon.color.b, 1f);
        btn.transform.localScale = Vector3.one;
    }

}
