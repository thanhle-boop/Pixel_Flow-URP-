using DG.Tweening;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OnOffSettingPresenter : MonoBehaviour
{
    [SerializeField] Button btnChangeStatus;
    [SerializeField] GameObject objBgOn;
    [SerializeField] Image imgStatus;
    [SerializeField] Image imgBg;
    [SerializeField] TextMeshProUGUI txtStatus;

    [Header("Anim")]
    [SerializeField] float animDuration = 0.2f;
    [SerializeField] float xOFF;
    [SerializeField] float xON;

    [Header("Sprite")]
    [SerializeField] Sprite sprON;
    [SerializeField] Sprite sprOFF;

    public void Init(ReactiveProperty<bool> isStatusRx, UnityAction<bool> onChangeStatus)
    {
        isStatusRx
            .Skip(1)
            .Subscribe(isOn =>
            {
                RunAnimChangeStatus(isOn);
            }).AddTo(this);

        ChangeStatus(isStatusRx.Value);
        imgStatus.transform.localPosition = new Vector3(isStatusRx.Value ? xON : xOFF, imgStatus.transform.localPosition.y, imgStatus.transform.localPosition.z);

        btnChangeStatus.OnClickAsObservable()
            .Subscribe(_ =>
            {
                var newStatus = !isStatusRx.Value;
                isStatusRx.Value = newStatus;
                onChangeStatus?.Invoke(newStatus);
            }).AddTo(this);
    }

    void RunAnimChangeStatus(bool isOn)
    {
        var targetX = isOn ? xON : xOFF;

        imgStatus.transform.DOLocalMoveX(targetX, animDuration)
            .OnComplete(() =>
            {
                ChangeStatus(isOn);
            });
    }

    void ChangeStatus(bool isOn)
    {
        imgStatus.sprite = isOn ? sprON : sprOFF;
        objBgOn.SetActive(isOn);
        txtStatus.text = isOn ? "ON" : "OFF";
    }
}
