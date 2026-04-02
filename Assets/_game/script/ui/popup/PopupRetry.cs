using R3;
using UnityEngine;
using UnityEngine.UI;

public class PopupRetry : BasePopup
{
    [SerializeField] Button btnRetry;

    protected override void Start()
    {
        base.Start();
        btnRetry.OnClickAsObservable()
            .Subscribe(_ =>
            {
                GameManager.Instance.StartGame();

                ClosePopup();
            }).AddTo(this);
    }

    protected override void AfterRunAnimClose()
    {
        GameManager.Instance.StartGame();

        base.AfterRunAnimClose();
    }
}
