using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ClickButton : MonoBehaviour
{
    [SerializeField] Button btn;

    void Reset()
    {
        btn = GetComponent<Button>();
    }

    void Start()
    {
        btn.OnClickAsObservable().Subscribe(_ =>
        {
            AudioController.instance.PlaySound(AudioIndex.tap_button.ToString());
            RunAnimClick();
        }).AddTo(this);
    }

    void RunAnimClick()
    {
        btn.transform.DOScale(1.1f, 0.1f).OnComplete(() =>
        {
            btn.transform.DOScale(1f, 0.1f);
        });
    }
}
