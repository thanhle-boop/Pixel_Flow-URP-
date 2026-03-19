using UnityEngine;

public class GameUtility
{
    public static Color GetColorByName(string colorName)
    {
        switch (colorName.ToLower())
        {
         case "red" : return new Color(0.82f, 0.14f, 0.13f);
         case "green": return new Color(0.36f, 0.96f, 0.23f);
        case "blue" :
            return Color.blue;
        case "yellow":
            return new Color(0.98f, 0.87f, 0.24f);
        case "black" :
           return new Color(0.309f, 0.322f, 0.357f);
        case "white":
            return Color.white;
        case "pink":
            return new Color(1f, 0.6f, 0.7f);
        case "dark pink" :
            return new Color(1f, 0.2f, 0.7f);
        case "orange" :
            return new Color(1f, 0.5f, 0f);
        case "dark green": return new Color(0.12f, 0.67f, 0.09f);
        case "light green":
            return new Color(0.56f, 0.93f, 0.56f);
        case "dark blue":
            return new Color(0f, 0f, 0.5f);
        case "light blue":
            return new Color(0.68f, 0.85f, 0.9f);  // LightBlue chuẩn
        case "purple":
            return new Color(0.5f, 0f, 0.5f);         // Purple thật (tối hơn Magenta)
        case "brown":
            return new Color(0.59f, 0.29f, 0f);
        case "light brown":
            return new Color(0.76f, 0.6f, 0.42f);
        case "cream":
            return new Color(1f, 0.99f, 0.82f);
        default : return Color.gray;
        }
    }
}
