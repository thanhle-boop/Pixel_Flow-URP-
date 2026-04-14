using R3;
using UnityEngine;
using UnityEngine.UI;

public class PopupHiddenMechanic : BasePopup
{
    [SerializeField] Button btnContinue;
    protected override void Start()
    {
        base.Start();
        btnContinue.OnClickAsObservable()
            .Subscribe(_ =>
            {
                TutorialController.AdvanceStep(MechanicTutorialType.Mechanic_Hidden.ToString());
                ClosePopup();
            }).AddTo(this);
    }
}
