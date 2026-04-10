using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameUtility : MonoBehaviour
{
    public static async UniTask<LevelData> LoadLevelData(int levelNumber)
    {
        var filename = $"L{levelNumber:D4}.json";
        var filetext = await StaticUtils.GetStreamingFileText(filename);
        var currentLevel = JsonUtility.FromJson<LevelData>(filetext);
        return currentLevel;
    }
}
