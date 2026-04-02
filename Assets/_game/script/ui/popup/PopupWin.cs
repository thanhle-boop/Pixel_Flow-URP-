using R3;
using UnityEngine;
using UnityEngine.UI;

public class PopupWin : BasePopup
{
    [SerializeField] Button btnContinue;

    protected override void Start()
    {
        base.Start();

        btnContinue.OnClickAsObservable()
            .Subscribe(_ =>
            {
                CurrencyController.AddGold(HardCodeInGame.REWARD_GOLD_WIN);
                GameManager.Instance.StartGame();

                ClosePopup();
            }).AddTo(this);
    }
}
