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

    public Color color1 = new Color(245f / 255f, 166f / 255f, 220f / 255f);     // Cream
    public Color color2 = new Color(139f / 255f, 236f / 255f, 243f / 255f);      // Cream
    public Color color3 = new Color(181f / 255f, 145f / 255f, 251f / 255f);      // Cream
    public Color color4 = new Color(1f, 233f / 255f, 123f / 255f);      // Cream
    public Color color5 = new Color(141f / 255f, 227f / 255f, 126f / 255f);      // Cream
    public Color color6 = new Color(1f, 174f / 255f, 111f / 255f);      // Cream
    public Color color7 = new Color(249f / 255f, 243f / 255f, 1f);      // Cream
    public Color color8 = new Color(85f / 255f, 8f / 255f, 97f / 255f);      // Cream
    public Color color9 = new Color(161f / 255f, 208f / 255f, 1f);      // Cream
    public Color color10 = new Color(97f / 255f, 174f / 255f, 9f / 255f);      // Cream

    public static Color GetColorByName(string colorName)
    {
        if (Instance == null)
        {
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

            case "c1": return Instance.color1;
            case "c2": return Instance.color2;
            case "c3": return Instance.color3;
            case "c4": return Instance.color4;
            case "c5": return Instance.color5;
            case "c6": return Instance.color6;
            case "c7": return Instance.color7;
            case "c8": return Instance.color8;
            case "c9": return Instance.color9;
            case "c10": return Instance.color10;

            default: return Instance.gray;
        }
    }
}