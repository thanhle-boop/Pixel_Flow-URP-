/// <summary>
/// logic core game: only join max level unlock. can't play the old levels
/// </summary>
public static class LevelController
{
    static LevelModel levelModel => PlayerModelManager.instance.GetPlayerModel<LevelModel>();

    public static int GetMaxLevelClear()
    {
        return levelModel.lLevel.Count == 1 ? 0 : levelModel.lLevel[levelModel.lLevel.Count - 2].level;
    }

    public static int GetMaxLevelUnlock()
    {
        return levelModel.lLevel[levelModel.lLevel.Count - 1].level;
    }

    public static LevelModelItem GetLevelModelItem(int level)
    {
        return levelModel.lLevel.Find(e => e.level == level);
    }

    public static void ClearLevel(int level)
    {
        //Here, we can process other values ​​for the level we just cleared. Ex: stars, score, etc.

        //Add new level unlocked
        var nextLevel = level + 1;
        //nex level is not exist, add it to the list
        if (levelModel.lLevel.Find(e => e.level == nextLevel) == null)
        {
            levelModel.lLevel.Add(new LevelModelItem { level = nextLevel });
        }

        levelModel.Save();
    }

    public static bool IsLevelUnlocked(int level)
    {
        return levelModel.lLevel.Find(e => e.level == level) != null;
    }
}
