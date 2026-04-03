using Cysharp.Threading.Tasks;
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
        coinText.text = "" + CurrencyController.GetGold();
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
}
