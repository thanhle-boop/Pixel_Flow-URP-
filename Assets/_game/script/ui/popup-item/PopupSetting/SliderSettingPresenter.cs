using R3;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SliderSettingPresenter : MonoBehaviour
{
    [SerializeField] Image img;
    [SerializeField] Slider slider;

    [Header("Sprite")]
    [SerializeField] Sprite sprON;
    [SerializeField] Sprite sprOFF;

    public void Init(float initValue, UnityAction<float> onChangeValue)
    {
        slider.value = initValue;

        slider.OnValueChangedAsObservable()
            .Subscribe(value =>
            {
                img.sprite = value > 0 ? sprON : sprOFF;
                onChangeValue?.Invoke(value);
            }).AddTo(this);
    }
}
