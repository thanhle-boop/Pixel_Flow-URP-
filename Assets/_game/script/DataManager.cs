using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    private GameData _playerData;

    protected override void Awake()
    {
        base.Awake();
        // _playerData = GetData();
        
        if (_playerData == null)
        {
            _playerData = new GameData();
        }

        //For Testing
        string json = JsonUtility.ToJson(_playerData);
        PlayerPrefs.SetString("GameData", json);
        PlayerPrefs.Save();
    }

    public int CurrentLevel => _playerData.CurrentLevel;
    public int Score        => _playerData.CurrentScore;
    public int Lives        => _playerData.CurrentLives;
    public int Coins        => _playerData.CurrentCoins;
    
    public int Item2 => _playerData.item2;
    public int Item3 => _playerData.item3;
    public int Item4 => _playerData.item4;
    
    public bool GetItem1()
    {
        if (_playerData.item1 > 0)
        {
            _playerData.item1--;
            return true;
        }
        return false;
    }

    public bool GetItem2()
    {
        if (_playerData.item2 > 0)
        {
            _playerData.item2--;
            return true;
        }
        return false;
    }
    public void IncreaseLevel()
    {
        _playerData.CurrentLevel++;
    }
    
    public void AddScore(int amount)
    {
        _playerData.CurrentScore += amount;
        UIManager.Instance.UpdateScore(_playerData.CurrentScore);
    }
    
    public void AddCoins(int amount)
    {
        _playerData.CurrentCoins += amount;
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
