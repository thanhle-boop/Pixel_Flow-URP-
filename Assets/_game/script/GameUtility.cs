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
    public Color cyan;
    public Color darkRed;
    public Color maroon;
    public Color crimson;
    public Color tomato;
    public Color hotPink;
    public Color deepPink;
    public Color lightPink;
    public Color darkOrange;
    public Color khaki;
    public Color lime;
    public Color limeGreen;
    public Color olive;
    public Color darkOlive;
    public Color teal;
    public Color deepSkyBlue;
    public Color turquoise;
    public Color darkPurple;
    public Color violet;
    public Color darkViolet;
    public Color darkBrown;
    public Color sienna;
    public Color darkGray;
    public Color lightGray;
    public Color dimGray;

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
            case "cyan": return Instance.cyan;
            case "dark red": return Instance.darkRed;
            case "maroon": return Instance.maroon;
            case "crimson": return Instance.crimson;
            case "tomato": return Instance.tomato;
            case "hot pink": return Instance.hotPink;
            case "deep pink": return Instance.deepPink;
            case "light pink": return Instance.lightPink;
            case "dark orange": return Instance.darkOrange;
            case "khaki": return Instance.khaki;
            case "lime": return Instance.lime;
            case "lime green": return Instance.limeGreen;
            case "olive": return Instance.olive;
            case "dark olive": return Instance.darkOlive;
            case "teal": return Instance.teal;
            case "deep sky blue": return Instance.deepSkyBlue;
            case "turquoise": return Instance.turquoise;
            case "dark purple": return Instance.darkPurple;
            case "violet": return Instance.violet;
            case "dark violet": return Instance.darkViolet;
            case "dark brown": return Instance.darkBrown;
            case "sienna": return Instance.sienna;
            case "dark gray": return Instance.darkGray;
            case "light gray": return Instance.lightGray;
            case "dim gray": return Instance.dimGray;
            default: return Instance.gray;
        }
    }
}