using R3;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TabHomePresenter : MonoBehaviour
{
    [SerializeField] Button btnPlay;

    [SerializeField] TextMeshProUGUI currentLevelText;
    [SerializeField] TextMeshProUGUI nextLevelText;
    [SerializeField] TextMeshProUGUI nextNextLevelText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int currentLevel = LevelController.GetMaxLevelUnlock();
        currentLevelText.text = "" + currentLevel;
        nextLevelText.text = "" + (currentLevel + 1);
        nextNextLevelText.text = "" + (currentLevel + 2);

        btnPlay.OnClickAsObservable()
            .Subscribe(_ =>
            {
                SceneManager.LoadScene("4.gameplay");
            }).AddTo(this);
    }
}
