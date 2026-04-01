using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public TextMeshProUGUI currentLevelText;
    public TextMeshProUGUI nextLevelText;
    public TextMeshProUGUI nextNextLevelText;
    public TextMeshProUGUI currentCoinText;

    public GameplayButton playGameButton;

    void OnEnable()
    {
        playGameButton.AddListener(OnPlayGameButtonClicked);
    }
    void Start()
    {

        int currentLevel = DataManager.instance.CurrentLevel + 1;
        currentLevelText.text = "" + currentLevel;
        nextLevelText.text = "" + (currentLevel + 1);
        nextNextLevelText.text = "" + (currentLevel + 2);
        currentCoinText.text = "" + DataManager.instance.Coins;
    }

    // Update is called once per frame
    public void OnPlayGameButtonClicked()
    {
        SceneManager.LoadScene("4.gameplay");
    }
}
