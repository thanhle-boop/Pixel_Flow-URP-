using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupRetry : BasePopup
{
    [SerializeField] TextMeshProUGUI txtLevel;
    [SerializeField] Button btnRetry;

    protected override void Start()
    {
        base.Start();

        txtLevel.text = $"Level {LevelController.GetMaxLevelUnlock()}";

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
