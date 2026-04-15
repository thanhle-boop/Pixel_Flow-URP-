using TMPro;
using UnityEngine;
using R3;

public class PopupLevel1 : BasePopup
{
    public TextMeshProUGUI des;
        private RectTransform _rect;


    protected override void Start()
    {
        base.Start();
        _rect = GetComponent<RectTransform>();
        var key = GuideTutorialType.Level_1.ToString();
        var item = TutorialController.GetTutorialItem(key);

        if (item != null)
        {
            item.currentStep
                .Subscribe(step =>
                {
                    switch (step)
                    {
                        case 0:

                            des.text = "Pick cat and start collecting yarn!"; // Ví dụ đổi text
                            _rect.sizeDelta = new Vector2(1080, 1000);
                            _rect.anchoredPosition = new Vector2(0f, 250f);
                            break;
                        case 1:
                            des.text = "Wait for the cat to travel!";
                            break;
                        case 2:

                            des.text = "The cat isn't full yet. Send it out again!";
                            break;
                        default:

                            break;
                    }
                })
                .AddTo(this);
        }
    }
}