using UnityEngine;

public class GameUtility
{
    // public Color red;
    // public Color green;
    // public Color blue;
    // public Color yellow;
    // public Color black;
    // public Color white;
    // public Color pink;
    // public Color darkPink;
    // public Color orange;
    // public Color darkGreen;
    // public Color lightGreen;
    // public Color darkBlue;
    // public Color lightBlue;
    // public Color purple;
    // public Color brown;
    // public Color lightBrown;
    // public Color cream;
    // public Color gray;
    // public Color cyan;
    // public Color darkRed;
    // public Color maroon;
    // public Color crimson;
    // public Color tomato;
    // public Color hotPink;
    // public Color deepPink;
    // public Color lightPink;
    // public Color darkOrange;
    // public Color khaki;
    // public Color lime;
    // public Color limeGreen;
    // public Color olive;
    // public Color darkOlive;
    // public Color teal;
    // public Color deepSkyBlue;
    // public Color turquoise;
    // public Color darkPurple;
    // public Color violet;
    // public Color darkViolet;
    // public Color darkBrown;
    // public Color sienna;
    // public Color darkGray;
    // public Color lightGray;
    // public Color dimGray;

    public static Color color1 = new Color(245f / 255f, 166f / 255f, 220f / 255f);     // Cream
    public static Color color2 = new Color(139f / 255f, 236f / 255f, 243f / 255f);      // Cream
    public static Color color3 = new Color(181f / 255f, 145f / 255f, 251f / 255f);      // Cream
    public static Color color4 = new Color(1f, 233f / 255f, 123f / 255f);      // Cream
    public static Color color5 = new Color(141f / 255f, 227f / 255f, 126f / 255f);      // Cream
    public static Color color6 = new Color(1f, 174f / 255f, 111f / 255f);      // Cream
    public static Color color7 = new Color(249f / 255f, 243f / 255f, 1f);      // Cream
    public static Color color8 = new Color(85f / 255f, 8f / 255f, 97f / 255f);      // Cream
    public static Color color9 = new Color(161f / 255f, 208f / 255f, 1f);      // Cream
    public static Color color10 = new Color(97f / 255f, 174f / 255f, 9f / 255f);      // Cream

      // Cream
    public static Color color11 = new Color(1f, 138f/255f, 145f/255f);
    public static Color color12 = new Color(109f/255f, 154f/255f, 223f/255f);
    public static Color color13 = new Color(87f/255f, 162f/255f, 139f/255f);
    public static Color color14 = new Color(233f/255f, 183f/255f, 1f);
    public static Color color15 = new Color(195f/255f, 215f/255f, 1f);
    public static Color color16 = new Color(122f/255f, 91f/255f, 71f/255f);
    public static Color color17 = new Color(1f, 232f/255f, 177f/255f);
    public static Color color18 = new Color(1f, 181f/255f, 195f/255f);
    public static Color color19 = new Color(191f/255f, 107f/255f, 137f/255f);
    public static Color color20 = new Color(181f/255f, 216f/255f, 162f/255f);
    public static Color color21 = new Color(124f/255f, 92f/255f, 181f/255f);
    public static Color color22 = new Color(204f/255f, 101f/255f, 113f/255f);
    public static Color color23 = new Color(183f/255f, 187f/255f, 223f/255f);
    public static Color color24 = new Color(115f/255f, 113f/255f, 135f/255f);
    public static Color color25 = new Color(206f/255f, 84f/255f, 157f/255f);
    public static Color color26 = new Color(1f, 202f/255f, 145f/255f);
    public static Color color27 = new Color(191f/255f, 93f/255f, 98f/255f);
    public static Color color28 = new Color(134f/255f, 199f/255f, 205f/255f);



    public static Color GetColorByName(string colorName)
    {
        // if (Instance == null)
        // {
        //     return Color.gray;
        // }

        switch (colorName.ToLower())
        {
            // case "red": return Instance.red;
            // case "green": return Instance.green;
            // case "blue": return Instance.blue;
            // case "yellow": return Instance.yellow;
            // case "black": return Instance.black;
            // case "white": return Instance.white;
            // case "pink": return Instance.pink;
            // case "dark pink": return Instance.darkPink;
            // case "orange": return Instance.orange;
            // case "dark green": return Instance.darkGreen;
            // case "light green": return Instance.lightGreen;
            // case "dark blue": return Instance.darkBlue;
            // case "light blue": return Instance.lightBlue;
            // case "purple": return Instance.purple;
            // case "brown": return Instance.brown;
            // case "light brown": return Instance.lightBrown;
            // case "cream": return Instance.cream;
            // case "cyan": return Instance.cyan;
            // case "dark red": return Instance.darkRed;
            // case "maroon": return Instance.maroon;
            // case "crimson": return Instance.crimson;
            // case "tomato": return Instance.tomato;
            // case "hot pink": return Instance.hotPink;
            // case "deep pink": return Instance.deepPink;
            // case "light pink": return Instance.lightPink;
            // case "dark orange": return Instance.darkOrange;
            // case "khaki": return Instance.khaki;
            // case "lime": return Instance.lime;
            // case "lime green": return Instance.limeGreen;
            // case "olive": return Instance.olive;
            // case "dark olive": return Instance.darkOlive;
            // case "teal": return Instance.teal;
            // case "deep sky blue": return Instance.deepSkyBlue;
            // case "turquoise": return Instance.turquoise;
            // case "dark purple": return Instance.darkPurple;
            // case "violet": return Instance.violet;
            // case "dark violet": return Instance.darkViolet;
            // case "dark brown": return Instance.darkBrown;
            // case "sienna": return Instance.sienna;
            // case "dark gray": return Instance.darkGray;
            // case "light gray": return Instance.lightGray;
            // case "dim gray": return Instance.dimGray;

            case "c01": return color1;
            case "c02": return color2;
            case "c03": return color3;
            case "c04": return color4;
            case "c05": return color5;
            case "c06": return color6;
            case "c07": return color7;
            case "c08": return color8;
            case "c09": return color9;
            case "c10": return color10;
            case "c11": return color11;
            case "c12": return color12;
            case "c13": return color13;
            case "c14": return color14;
            case "c15": return color15;
            case "c16": return color16;
            case "c17": return color17;
            case "c18": return color18;
            case "c19": return color19;
            case "c20": return color20;
            case "c21": return color21;
            case "c22": return color22;
            case "c23": return color23;
            case "c24": return color24;
            case "c25": return color25;
            case "c26": return color26;
            case "c27": return color27;
            case "c28": return color28;

            default: return Color.gray;
        }
    }
}