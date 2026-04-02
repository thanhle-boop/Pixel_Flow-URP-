using UnityEngine;
using R3;

public class DataManager : SingletonMonoBehaviour<DataManager>
{
    private GameData _playerData;

    protected override void Awake()
    {
        base.Awake();
        _playerData = GetData();

        // if (_playerData == null)
        // {
        //     _playerData = new GameData();
        // }

        //For Testing
        // string json = JsonUtility.ToJson(_playerData);
        // PlayerPrefs.SetString("GameData", json);
        // PlayerPrefs.Save();
    }

    public int CurrentLevel => _playerData.CurrentLevel;
    public int Score => _playerData.CurrentScore;
    public int Lives => _playerData.CurrentLives;


    public void IncreaseLevel()
    {
        _playerData.CurrentLevel++;
    }

    public void AddScore(int amount)
    {
        _playerData.CurrentScore += amount;
        UIManager.Instance.UpdateScore(_playerData.CurrentScore);
    }

    public void LoseLife()
    {
        if (_playerData.CurrentLives > 0)
        {
            _playerData.CurrentLives--;
        }
    }

    private GameData GetData()
    {
        GameData data = null;
        if (PlayerPrefs.HasKey("GameData"))
        {
            string json = PlayerPrefs.GetString("GameData");
            data = JsonUtility.FromJson<GameData>(json);
        }
        else
        {
            data = new GameData();
        }
        return data;
    }

    public void SaveData()
    {
        string json = JsonUtility.ToJson(_playerData);
        PlayerPrefs.SetString("GameData", json);
        PlayerPrefs.Save();
    }

}
