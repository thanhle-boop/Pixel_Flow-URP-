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
    
public int GetItemCount(int itemType)
    {
        switch (itemType)
        {
            case 1: return _playerData.item1.count;
            case 2: return _playerData.item2.count;
            case 3: return _playerData.item3.count;
            case 4: return _playerData.item4.count;
        }
        return 0;
    }

    public void ConsumeItem(int itemType)
    {
        int newCount = 0; 

        switch (itemType)
        {
            case 1:
                if (_playerData.item1.count > 0) _playerData.item1.count--;
                newCount = _playerData.item1.count;
                break;
            case 2:
                if (_playerData.item2.count > 0) _playerData.item2.count--;
                newCount = _playerData.item2.count;
                break;
            case 3:
                if (_playerData.item3.count > 0) _playerData.item3.count--;
                newCount = _playerData.item3.count;
                break;
            case 4:
                if (_playerData.item4.count > 0) _playerData.item4.count--;
                newCount = _playerData.item4.count;
                break;
        }

        SaveData();
        EventManager.OnItemCountChanged?.Invoke(itemType, newCount);
    }
    public bool GetStatus(int itemType)
    {
        switch (itemType)
        {
            case 1:
                return _playerData.item1.isOpened;
            case 2:
                return _playerData.item2.isOpened;
            case 3:
                return _playerData.item3.isOpened;
            case 4:
                return _playerData.item4.isOpened;
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
