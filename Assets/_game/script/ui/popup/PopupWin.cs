using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupWin : BasePopup
{
    [SerializeField] TextMeshProUGUI txtRewardAmount;
    [SerializeField] Button btnContinue;
    [SerializeField] TextMeshProUGUI txtRewardBonusAmount;
    [SerializeField] Button btnWatchAd;

    protected override void Start()
    {
        base.Start();

        txtRewardAmount.text = $"{HardCodeInGame.REWARD_GOLD_WIN}";
        txtRewardBonusAmount.text = $"{HardCodeInGame.REWARD_GOLD_WIN * HardCodeInGame.BOUNE_REWARD_GOLD_MULTI}";

        btnContinue.OnClickAsObservable()
            .Subscribe(_ =>
            {
                CurrencyController.AddGold(HardCodeInGame.REWARD_GOLD_WIN);
                GameManager.Instance.StartGame();

                ClosePopup();
            }).AddTo(this);

        btnWatchAd.OnClickAsObservable()
            .Subscribe(_ =>
            {
                Debug.Log("Add Logic: Watch ad to double reward");
            }).AddTo(this);
    }
}
