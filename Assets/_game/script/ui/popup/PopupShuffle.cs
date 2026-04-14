using R3;
using UnityEngine;
using UnityEngine.UI;

public class PopupShuffle : BasePopup
{
    [SerializeField] Button btnClaim;
    protected override void Start()
    {
        base.Start();
        btnClaim.OnClickAsObservable()
            .Subscribe(_ =>
            {
                TutorialController.AdvanceStep(BoosterTutorialType.Booster_Shuffle.ToString());
                ClosePopup();
            }).AddTo(this);
    }
}
