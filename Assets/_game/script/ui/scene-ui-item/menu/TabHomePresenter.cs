using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TabHomePresenter : MonoBehaviour
{
    [SerializeField] Button btnPlay;
    [SerializeField] Button btnSettings;

    [SerializeField] TextMeshProUGUI currentLevelText;
    [SerializeField] TextMeshProUGUI nextLevelText;
    [SerializeField] TextMeshProUGUI nextNextLevelText;
    [SerializeField] TextMeshProUGUI nextNextLevelText1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int currentLevel = LevelController.GetMaxLevelUnlock();
        currentLevelText.text = "" + currentLevel;
        nextLevelText.text = "" + (currentLevel + 1);
        nextNextLevelText.text = "" + (currentLevel + 2);
        nextNextLevelText1.text = "" + (currentLevel + 3);

        btnSettings.OnClickAsObservable()
            .Subscribe(_ =>
            {
                PopupManager.instance.OpenPopup<PopupSettings>().Forget();
            }).AddTo(this);

        btnPlay.OnClickAsObservable()
            .Subscribe(_ =>
            {
                SceneManager.LoadScene("4.gameplay");
            }).AddTo(this);
    }
}
