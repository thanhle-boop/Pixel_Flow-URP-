using Cysharp.Threading.Tasks;
using R3;
using System.Collections;
using TMPro;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

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

    bool isBottomUiTranslate = false;

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
        btnAddTray.OnClickAsObservable()
            .Subscribe(_ =>
            {
                if (DataManager.instance.GetItemCount(1) <= 0)
                {
                    return;
                }
                UIManager.Instance.straightSlot.SetActive(true);
                EventManager.OnUseAddTray?.Invoke();
                DataManager.instance.ConsumeItem(1);
            }).AddTo(this);

        btnHand.OnClickAsObservable()
            .Subscribe(_ =>
            {
                if (isBottomUiTranslate)
                {
                    return;
                }

                if (DataManager.instance.GetItemCount(2) <= 0)
                {
                    return;
                }
                EventManager.OnUseHand?.Invoke();
                Vector2 targetPos = rectBottomUI.anchoredPosition + new Vector2(0, -230f);
                isBottomUiTranslate = true;
                StartCoroutine(MoveUISmoothly(rectBottomUI, targetPos));
                DataManager.instance.ConsumeItem(2);
            }).AddTo(this);

        btnUseShuff.OnClickAsObservable()
            .Subscribe(_ =>
            {
                if (DataManager.instance.GetItemCount(3) <= 0)
                {
                    return;
                }
                EventManager.OnUseShuffle?.Invoke();
                DataManager.instance.ConsumeItem(3);
            }).AddTo(this);

        btnUseSuper.OnClickAsObservable()
            .Subscribe(_ =>
            {
                if (DataManager.instance.GetItemCount(4) <= 0)
                {
                    return;
                }
                EventManager.OnUseSuperCat?.Invoke();
                DataManager.instance.ConsumeItem(4);
            }).AddTo(this);

        btnSetting.OnClickAsObservable()
            .Subscribe(_ =>
            {
                PopupManager.instance.OpenPopup<PopupSettings>().Forget();
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
        levelText.text = "Level " + (DataManager.instance.CurrentLevel + 1);
        coinText.text = "" + DataManager.instance.Coins;
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
