using UnityEngine;

public class GameUtility : Singleton<GameUtility>
{   
    public Color red;
    public Color green;
    public Color blue;
    public Color yellow;
    public Color black;
    public Color white;
    public Color pink;
    public Color darkPink;
    public Color orange;
    public Color darkGreen;
    public Color lightGreen;
    public Color darkBlue;
    public Color lightBlue;
    public Color purple;
    public Color brown;
    public Color lightBrown;
    public Color cream;
    public Color gray;

    public static Color GetColorByName(string colorName)
    {
        if (Instance == null)
        {
            Debug.LogWarning("Chưa có GameUtility trong Scene! Trả về màu xám mặc định.");
            return Color.gray;
        }

        switch (colorName.ToLower())
        {
            case "red": return Instance.red;
            case "green": return Instance.green;
            case "blue": return Instance.blue;
            case "yellow": return Instance.yellow;
            case "black": return Instance.black;
            case "white": return Instance.white;
            case "pink": return Instance.pink;
            case "dark pink": return Instance.darkPink;
            case "orange": return Instance.orange;
            case "dark green": return Instance.darkGreen;
            case "light green": return Instance.lightGreen;
            case "dark blue": return Instance.darkBlue;
            case "light blue": return Instance.lightBlue;
            case "purple": return Instance.purple;
            case "brown": return Instance.brown;
            case "light brown": return Instance.lightBrown;
            case "cream": return Instance.cream;
            default: return Instance.gray;
        }
    }
}