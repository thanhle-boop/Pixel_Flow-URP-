using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class UIManager : Singleton<UIManager>
{
    protected override bool PersistAcrossScenes => false;

    public List<string> tagValueTypes;
    public TextMeshProUGUI straightSlotText;
    public VideoPlayer loseGameVideo;

    [Header("Gameplay UI")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI levelText;
    public GameObject gameplayUI;


    [Header("Game Over UI")]
    public GameObject gameOverUI;
    public GameObject reTryUI;
    public TextMeshProUGUI gameOverCoinText;


    [Header("Win UI")]
    public GameObject winUI;
    public TextMeshProUGUI winCoinText;

    public GameObject straightSlot;
    public GameObject BottomUI;
    public UnityEngine.EventSystems.EventSystem eventSystem;
    public bool isBottomUiTranslate = false;

    public GameplayButton item1Button;
    public GameplayButton item2Button;
    public GameplayButton item3Button;
    public GameplayButton item4Button;
    public GameplayButton settingButton;
    private void OnEnable()
    {
        EventManager.OnLoseGame += GameOver;
        EventManager.OnStartGame += StartGame;
        EventManager.OnFullConveyorSlot += OnInvalidExecution;
        EventManager.OnWinGame += WinGame;
        EventManager.OnEndHand += OnEndHand;

        item1Button.AddListener(OnAddTrayButtonClicked);
        item2Button.AddListener(OnHandButtonClicked);
        item3Button.AddListener(OnUseShuffleButtonClicked);
        item4Button.AddListener(OnUseSuperCatClicked);
        settingButton.AddListener(OnSettingButtonClicked);
    }

    private void OnDisable()
    {
        EventManager.OnLoseGame -= GameOver;
        EventManager.OnStartGame -= StartGame;
        EventManager.OnFullConveyorSlot -= OnInvalidExecution;
        EventManager.OnWinGame -= WinGame;
        EventManager.OnEndHand -= OnEndHand;

        item1Button.RemoveListener(OnAddTrayButtonClicked);
        item2Button.RemoveListener(OnHandButtonClicked);
        item3Button.RemoveListener(OnUseShuffleButtonClicked);
        item4Button.RemoveListener(OnUseSuperCatClicked);
        settingButton.RemoveListener(OnSettingButtonClicked);

    }

    private void OnSettingButtonClicked()
    {
        SceneManager.LoadScene("3.menu");
    }

    private void OnEndHand()
    {
        isBottomUiTranslate = true;
        RectTransform rect = BottomUI.GetComponent<RectTransform>();
        StartCoroutine(MoveUISmoothly(rect, new Vector2(0, 0f)));
    }

    private void WinGame()
    {
        ShowWinUI(DataManager.Instance.Coins);
    }

    private void OnInvalidExecution()
    {
        straightSlotText.GetComponent<Animator>().SetTrigger("IsInvalid");
    }

    private void StartGame()
    {
        if (winUI == null || gameOverUI == null || reTryUI == null || gameplayUI == null)
        {
            return;
        }
        levelText.text = "Level " + (DataManager.Instance.CurrentLevel + 1);
        coinText.text = "" + DataManager.Instance.Coins;
        gameOverUI.SetActive(false);
        reTryUI.SetActive(false);
        gameplayUI.SetActive(true);
    }

    private void GameOver()
    {
        ShowGameOverUI();
        loseGameVideo.Play();
    }
    public void OnDownloadButtonClicked()
    {
        //Luna.Unity.Playable.InstallFullGame();
    }

    public void UpdateStraightSlot(float count, float maxSlot)
    {
        if (straightSlotText == null)
        {
            return;
        }
        straightSlotText.text = count + "/" + maxSlot;
    }

    public void UpdateScore(int score)
    {
        // scoreText.text = score.ToString();
    }

    public void RestartGame()
    {
        GameManager.Instance.StartGame();
    }

    public void CollectCoin()
    {
        DataManager.Instance.AddCoins(40);
        winCoinText.text = "" + DataManager.Instance.Coins;
        winUI.SetActive(false);
        GameManager.Instance.StartGame();
    }

    public void OnContinueButtonClicked()
    {
        if (gameOverUI == null || gameplayUI == null)
        {
            Debug.Log("congthanh");
            return;
        }
        GameManager.Instance.ContinueGame();
        DataManager.Instance.AddCoins(-900);
        coinText.text = "" + DataManager.Instance.Coins;
        gameOverCoinText.text = "" + DataManager.Instance.Coins;
        gameOverUI.SetActive(false);
        gameplayUI.SetActive(true);
    }

    public void ShowWinUI(int coinsEarned)
    {
        if (winUI == null || gameplayUI == null)
        {
            return;
        }

        winUI.SetActive(true);
        gameplayUI.SetActive(false);
        winCoinText.text = "" + coinsEarned;
    }

    public void ShowGameOverUI()
    {
        if (gameOverUI == null || gameplayUI == null)
        {
            return;
        }

        gameOverUI.SetActive(true);
        gameOverCoinText.text = "" + DataManager.Instance.Coins;
        gameplayUI.SetActive(false);
    }

    public void ShowGameplayUI()
    {
        if (gameplayUI == null)
        {
            return;
        }
        gameplayUI.SetActive(true);
        levelText.text = "Level " + (DataManager.Instance.CurrentLevel + 1);
        coinText.text = "" + DataManager.Instance.Coins;
    }
    public void CloseGameOverUI()
    {
        if (gameOverUI == null || reTryUI == null)
        {
            return;
        }
        gameOverUI.SetActive(false);
        reTryUI.SetActive(true);
    }

    public void OnRetryButtonClicked()
    {
        if (reTryUI == null)
        {
            return;
        }

        DataManager.Instance.LoseLife();
        GameManager.Instance.StartGame();
        reTryUI.SetActive(false);
    }

    public void CloseRetryUI()
    {
        if (reTryUI == null)
        {
            return;
        }
        reTryUI.SetActive(false);
    }
    public void OnBackToEditorClicked()
    {
        SceneManager.LoadScene("5.level_editor");
    }

    public void OnAddTrayButtonClicked()
    {
        if (DataManager.Instance.GetItemCount(1) <= 0)
        {
            return;
        }
        straightSlot.SetActive(true);
        EventManager.OnUseAddTray?.Invoke();
        DataManager.Instance.ConsumeItem(1);

    }

    public void OnHandButtonClicked()
    {
        if (isBottomUiTranslate)
        {
            return;
        }

        if (DataManager.Instance.GetItemCount(2) <= 0)
        {
            return;
        }
        EventManager.OnUseHand?.Invoke();
        RectTransform rect = BottomUI.GetComponent<RectTransform>();
        Vector2 targetPos = rect.anchoredPosition + new Vector2(0, -230f);
        isBottomUiTranslate = true;
        StartCoroutine(MoveUISmoothly(rect, targetPos));
        DataManager.Instance.ConsumeItem(2);

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

    public void OnUseShuffleButtonClicked()
    {
        if (DataManager.Instance.GetItemCount(3) <= 0)
        {
            return;
        }
        EventManager.OnUseShuffle?.Invoke();
        DataManager.Instance.ConsumeItem(3);
    }

    public void OnUseSuperCatClicked()
    {
        if (DataManager.Instance.GetItemCount(4) <= 0)
        {
            return;
        }
        EventManager.OnUseSuperCat?.Invoke();
        DataManager.Instance.ConsumeItem(4);
    }


}
