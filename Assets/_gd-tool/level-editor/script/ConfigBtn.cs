using TMPro;
using UnityEngine;

public class ConfigBtn : MonoBehaviour
{
    public int index = -1;

    public TextMeshProUGUI text;

    public void OnClick()
    {
        EventManager.onClickButton?.Invoke(index);
    }

    public void SetText(string newText)
    {
        text.text = newText;
    }
}
