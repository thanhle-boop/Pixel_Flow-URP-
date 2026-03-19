using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Configs/LevelData")]
public class LevelDataSO : ScriptableObject {
    public TextAsset sourceJson;

    [Header("Parsed Data")]
    public int levelIndex;
    public int width;
    public int height;
    public List<string> gridData;
    public List<LaneConfig> lanes;

    [ContextMenu("Import from JSON")]
    public void Import() {
        if (sourceJson != null) {
            JsonUtility.FromJsonOverwrite(sourceJson.text, this);
        }
    }
    
    public string GetCell(int x, int y) {
         int index = y * width + x;
         if (index >= 0 && index < gridData.Count) return gridData[index];
         return "empty";
    }
}