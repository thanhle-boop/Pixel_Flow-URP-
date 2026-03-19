using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Configs/LevelData")]
public class LevelDataSO : ScriptableObject {
    public TextAsset sourceJson; // Kéo file JSON vào đây

    [Header("Parsed Data")]
    public int levelIndex;
    public int width;
    public int height;
    public List<string> gridData;
    public List<LaneConfig> lanes;

    [ContextMenu("Import from JSON")] // Chuột phải vào SO chọn dòng này để load
    public void Import() {
        if (sourceJson != null) {
            JsonUtility.FromJsonOverwrite(sourceJson.text, this);
            Debug.Log("Đã cập nhật dữ liệu từ JSON!");
        }
    }
    
    public string GetCell(int x, int y) {
         int index = y * width + x;
         if (index >= 0 && index < gridData.Count) return gridData[index];
         return "empty";
    }
}