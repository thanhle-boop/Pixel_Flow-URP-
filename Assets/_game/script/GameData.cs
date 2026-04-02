using System;
using R3;

[System.Serializable]
public class GameData 
{
    public int CurrentLevel = 0;
    public int CurrentScore = 0;
    public int CurrentLives = 5;

    // public int item1 = 5;
    // public int item2 = 5;
    // public int item3 = 5;
    // public int item4 = 5;

    public itemData item1 = new itemData { count = 5, isOpened = true, itemType = 1 };
    public itemData item2 = new itemData { count = 5, isOpened = true, itemType = 2 };
    public itemData item3 = new itemData { count = 5, isOpened = true, itemType = 3 };
    public itemData item4 = new itemData { count = 5, isOpened = false, itemType = 4 };
}

[System.Serializable]
public class itemData
{
    public int count;
    public bool isOpened;
    public int itemType;
}