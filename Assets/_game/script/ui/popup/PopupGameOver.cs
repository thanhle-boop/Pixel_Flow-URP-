using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PopupGameOver : BasePopup
{
    [SerializeField] VideoPlayer loseGameVideo;
    [SerializeField] Button btnContinue;

    protected override void Start()
    {
        base.Start();
        loseGameVideo.Play();

        btnContinue.OnClickAsObservable()
            .Subscribe(_ =>
            {
                GameManager.Instance.ContinueGame();
                CurrencyController.SubtractGold(HardCodeInGame.COST_GOLD_CONTINUE);

                ClosePopup();
            }).AddTo(this);
    }

    public override void OnClosePopup(bool isRunAnim = true)
    {
        base.OnClosePopup(isRunAnim);

        PopupManager.instance.OpenPopup<PopupRetry>().Forget();
    }
}
