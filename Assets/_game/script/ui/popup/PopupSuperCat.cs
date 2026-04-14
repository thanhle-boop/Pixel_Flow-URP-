using UnityEngine;
using UnityEngine.UI;
using R3;

public class PopupSuperCat : BasePopup
{
    [SerializeField] Button btnClaim;
    protected override void Start()
    {
        base.Start();
        btnClaim.OnClickAsObservable()
            .Subscribe(_ =>
            {
                TutorialController.AdvanceStep(BoosterTutorialType.Booster_Super.ToString());
                ClosePopup();
            }).AddTo(this);
    }
}
