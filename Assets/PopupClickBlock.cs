using R3;
using UnityEngine;
using UnityEngine.UI;

public class PopupClickBlock : BasePopup
{
    [SerializeField] Button btnClaim;
    protected override void Start()
    {
        base.Start();
        btnClaim.OnClickAsObservable()
            .Subscribe(_ =>
            {
                EventManager.closeClickBlockPopup?.Invoke();
                ClosePopup();
            }).AddTo(this);
    }
}
