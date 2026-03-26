using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Helper
{
    public static readonly Dictionary<string, Color> ColorMap = new Dictionary<string, Color>()
    {
            // // Unity built-in
            // { "red",              Color.red },                          // (1, 0, 0)
            // // { "green",            Color.green },                       // (0, 1, 0)
            // { "blue",             Color.blue },                        // (0, 0, 1)
            // { "yellow",           Color.yellow },                      // (1, 0.92, 0.016)
            // { "black",            Color.black },                       // (0, 0, 0)
            // { "white",            Color.white },                       // (1, 1, 1)
            // { "cyan",             Color.cyan },                        // (0, 1, 1)
            // // { "magenta",          Color.magenta },                     // (1, 0, 1)
            // { "gray",             Color.gray },                        // (0.5, 0.5, 0.5)

            // // Reds / Pinks
            // { "dark red",         new Color(0.55f, 0f, 0f) },         // DarkRed
            // { "maroon",           new Color(0.5f, 0f, 0f) },          // Maroon
            // { "crimson",          new Color(0.86f, 0.08f, 0.24f) },   // Crimson
            // // { "indian red",       new Color(0.80f, 0.36f, 0.36f) },   // IndianRed
            // // { "salmon",           new Color(0.98f, 0.50f, 0.45f) },   // Salmon
            // // { "light salmon",     new Color(1f, 0.63f, 0.48f) },      // LightSalmon
            // // { "coral",            new Color(1f, 0.50f, 0.31f) },      // Coral
            // { "tomato",           new Color(1f, 0.39f, 0.28f) },      // Tomato
            // { "pink",             new Color(1f, 0.75f, 0.80f) },      // Pink
            // { "hot pink",         new Color(1f, 0.41f, 0.71f) },      // HotPink
            // { "deep pink",        new Color(1f, 0.08f, 0.58f) },      // DeepPink
            // { "light pink",       new Color(1f, 0.71f, 0.76f) },      // LightPink
            // // { "rose",             new Color(1f, 0f, 0.50f) },         // Rose

            // // Oranges
            // { "orange",           new Color(1f, 0.65f, 0f) },         // Orange
            // { "dark orange",      new Color(1f, 0.55f, 0f) },         // DarkOrange
            // // { "orange red",       new Color(1f, 0.27f, 0f) },         // OrangeRed
            // // { "peach",            new Color(1f, 0.85f, 0.73f) },      // PeachPuff

            // // Yellows
            // // { "gold",             new Color(1f, 0.84f, 0f) },         // Gold
            // // { "light yellow",     new Color(1f, 1f, 0.88f) },         // LightYellow
            // // { "lemon",            new Color(1f, 0.97f, 0f) },         // Lemon
            // { "khaki",            new Color(0.94f, 0.90f, 0.55f) },   // Khaki
            // // { "dark khaki",       new Color(0.74f, 0.72f, 0.42f) },   // DarkKhaki

            // // Greens
            // { "lime",             new Color(0.75f, 1f, 0f) },         // Lime/Chartreuse
            // { "lime green",       new Color(0.20f, 0.80f, 0.20f) },   // LimeGreen
            // { "light green",      new Color(0.56f, 0.93f, 0.56f) },   // LightGreen
            // { "dark green",       new Color(0f, 0.39f, 0f) },         // DarkGreen
            // // { "forest green",     new Color(0.13f, 0.55f, 0.13f) },   // ForestGreen
            // // { "sea green",        new Color(0.18f, 0.55f, 0.34f) },   // SeaGreen
            // // { "spring green",     new Color(0f, 1f, 0.50f) },         // SpringGreen
            // { "olive",            new Color(0.50f, 0.50f, 0f) },      // Olive
            // { "dark olive",       new Color(0.33f, 0.42f, 0.18f) },   // DarkOliveGreen
            // // { "mint",             new Color(0.60f, 1f, 0.60f) },      // Mint
            // { "teal",             new Color(0f, 0.50f, 0.50f) },      // Teal

            // // Blues
            // // { "light blue",       new Color(0.68f, 0.85f, 0.90f) },   // LightBlue
            // // { "sky blue",         new Color(0.53f, 0.81f, 0.92f) },   // SkyBlue
            // { "deep sky blue",    new Color(0f, 0.75f, 1f) },         // DeepSkyBlue
            // // { "dodger blue",      new Color(0.12f, 0.56f, 1f) },      // DodgerBlue
            // // { "royal blue",       new Color(0.25f, 0.41f, 0.88f) },   // RoyalBlue
            // { "dark blue",        new Color(0f, 0f, 0.55f) },         // DarkBlue
            // // { "navy",             new Color(0f, 0f, 0.50f) },         // Navy
            // // { "midnight blue",    new Color(0.10f, 0.10f, 0.44f) },   // MidnightBlue
            // // { "steel blue",       new Color(0.27f, 0.51f, 0.71f) },   // SteelBlue
            // // { "cornflower blue",  new Color(0.39f, 0.58f, 0.93f) },   // CornflowerBlue
            // { "turquoise",        new Color(0.25f, 0.88f, 0.82f) },   // Turquoise
            // // { "aquamarine",       new Color(0.50f, 1f, 0.83f) },      // Aquamarine

            // // Purples / Violets
            // // { "purple",           new Color(0.50f, 0f, 0.50f) },      // Purple
            // { "dark purple",      new Color(0.30f, 0f, 0.30f) },      // DarkPurple
            // { "violet",           new Color(0.93f, 0.51f, 0.93f) },   // Violet
            // { "dark violet",      new Color(0.58f, 0f, 0.83f) },      // DarkViolet
            // // { "indigo",           new Color(0.29f, 0f, 0.51f) },      // Indigo
            // // { "lavender",         new Color(0.90f, 0.90f, 0.98f) },   // Lavender
            // // { "orchid",           new Color(0.85f, 0.44f, 0.84f) },   // Orchid
            // // { "plum",             new Color(0.87f, 0.63f, 0.87f) },   // Plum
            // // { "medium purple",    new Color(0.58f, 0.44f, 0.86f) },   // MediumPurple
            // // { "blue violet",      new Color(0.54f, 0.17f, 0.89f) },   // BlueViolet
            // // { "slate blue",       new Color(0.42f, 0.35f, 0.80f) },   // SlateBlue

            // // Browns / Beiges
            // // { "brown",            new Color(0.65f, 0.16f, 0.16f) },   // SaddleBrown-ish
            // { "dark brown",       new Color(0.40f, 0.26f, 0.13f) },   // DarkBrown
            // { "light brown",      new Color(0.76f, 0.60f, 0.42f) },   // BurlyWood
            // // { "chocolate",        new Color(0.82f, 0.41f, 0.12f) },   // Chocolate
            // { "sienna",           new Color(0.63f, 0.32f, 0.18f) },   // Sienna
            // // { "tan",              new Color(0.82f, 0.71f, 0.55f) },   // Tan
            // // { "wheat",            new Color(0.96f, 0.87f, 0.70f) },   // Wheat
            // // { "beige",            new Color(0.96f, 0.96f, 0.86f) },   // Beige
            // // { "sandy brown",      new Color(0.96f, 0.64f, 0.38f) },   // SandyBrown
            // // { "peru",             new Color(0.80f, 0.52f, 0.25f) },   // Peru

            // // Grays / Neutrals
            // { "dark gray",        new Color(0.25f, 0.25f, 0.25f) },   // DarkGray
            // { "light gray",       new Color(0.75f, 0.75f, 0.75f) },   // LightGray
            // // { "silver",           new Color(0.75f, 0.75f, 0.75f) },   // Silver
            // { "dim gray",         new Color(0.41f, 0.41f, 0.41f) },   // DimGray
            // // { "slate gray",       new Color(0.44f, 0.50f, 0.56f) },   // SlateGray
            // // { "ivory",            new Color(1f, 1f, 0.94f) },         // Ivory
            // // { "snow",             new Color(1f, 0.98f, 0.98f) },      // Snow
            // { "cream",            new Color(1f, 0.99f, 0.82f) },      // Cream

            { "c01",            new Color(245f/255f, 166f/255f, 220f/255f) },      // Cream
            { "c02",            new Color(139f/255f, 236f/255f, 243f/255f) },      // Cream
            { "c03",            new Color(181f/255f, 145f/255f, 251f/255f) },      // Cream
            { "c04",            new Color(1f, 233f/255f, 123f/255f) },      // Cream
            { "c05",            new Color(141f/255f, 227f/255f, 126f/255f) },      // Cream
            { "c06",            new Color(1f, 174f/255f, 111f/255f) },      // Cream
            { "c07",            new Color(249f/255f, 243f/255f, 1f) },      // Cream
            { "c08",            new Color(85f/255f, 85f/255f, 97f/255f) },      // Cream
            { "c09",            new Color(161f/255f, 208f/255f, 1f) },      // Cream
            { "c10",           new Color(97f/255f, 174f/255f, 9f/255f) },      // Cream
            { "c11",           new Color(1f, 138f/255f, 145f/255f) },
            { "c12",           new Color(109f/255f, 154f/255f, 223f/255f) },
            { "c13",           new Color(87f/255f, 162f/255f, 139f/255f) },
            { "c14",           new Color(233f/255f, 183f/255f, 1f) },
            { "c15",           new Color(195f/255f, 215f/255f, 1f) },
            { "c16",           new Color(122f/255f, 91f/255f, 71f/255f) },
            { "c17",           new Color(1f, 232f/255f, 177f/255f) },
            { "c18",           new Color(1f, 181f/255f, 195f/255f) },
            { "c19",           new Color(191f/255f, 107f/255f, 137f/255f) },
            { "c20",           new Color(181f/255f, 216f/255f, 162f/255f) },
            { "c21",           new Color(124f/255f, 92f/255f, 181f/255f) },
            { "c22",           new Color(204f/255f, 101f/255f, 113f/255f) },
            { "c23",           new Color(183f/255f, 187f/255f, 223f/255f) },
            { "c24",           new Color(115f/255f, 113f/255f, 135f/255f) },
            { "c25",           new Color(206f/255f, 84f/255f, 157f/255f) },
            { "c26",           new Color(1f, 202f/255f, 145f/255f) },
            { "c27",           new Color(191f/255f, 93f/255f, 98f/255f) },
            { "c28",           new Color(134f/255f, 199f/255f, 205f/255f) },
    };

    public static string GetClosestColor(Color c)
    {
        if (c.a < 0.1f) return "empty";

        string closestColor = "empty";
        float minDist = float.MaxValue;

        foreach (var kvp in ColorMap)
        {
            float dist = (c.r - kvp.Value.r) * (c.r - kvp.Value.r)
                       + (c.g - kvp.Value.g) * (c.g - kvp.Value.g)
                       + (c.b - kvp.Value.b) * (c.b - kvp.Value.b);

            if (dist < minDist)
            {
                minDist = dist;
                closestColor = kvp.Key;
            }
        }

        if (minDist > 0.75f) return "empty";

        return closestColor;
    }

    public static Color GetColorFromName(string name)
    {
        if (string.IsNullOrEmpty(name) || name == "empty")
            return new Color(0.2f, 0.2f, 0.2f, 0.5f);

        string key = name.ToLower();
        if (ColorMap.TryGetValue(key, out Color c)) return c;
        return Color.gray;
    }

    public static string MostColoredAtEdge(Dictionary<string, int> dict)
    {
        var maxCount = 0;
        var maxColor = "";
        foreach (var pair in dict.Where(pair => pair.Value > maxCount))
        {
            maxCount = pair.Value;
            maxColor = pair.Key;
        }
        return maxColor;
    }

    public static void ShuffleList<T>(IList<T> list)
    {


        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public static Dictionary<int, List<PigComponent>> ShuffleHeoDictionary(Dictionary<int, List<PigComponent>> inputDict)
    {
        List<PigComponent> poolHeoThuong = new List<PigComponent>();
        var resultDict = new Dictionary<int, List<PigComponent>>();

        // Từ điển để lưu tạm Heo VIP và Vị trí gốc (Index) của chúng
        var vipPigsDict = new Dictionary<int, List<PigComponent>>();
        var vipIndicesDict = new Dictionary<int, List<int>>();
        var originalLengths = new Dictionary<int, int>();

        // BƯỚC 1: Phân loại heo và ghi nhớ Index gốc của heo VIP
        foreach (var kvp in inputDict)
        {
            int key = kvp.Key;
            List<PigComponent> currentList = kvp.Value;

            originalLengths[key] = currentList.Count;
            vipPigsDict[key] = new List<PigComponent>();
            vipIndicesDict[key] = new List<int>();

            for (int i = 0; i < currentList.Count; i++)
            {
                var heo = currentList[i];
                if (heo.IsLinkedPig())
                {
                    vipPigsDict[key].Add(heo);
                    vipIndicesDict[key].Add(i); // Lưu lại index hiện tại của heo nối dây
                }
                else
                {
                    poolHeoThuong.Add(heo);
                }
            }
        }

        // BƯỚC 2: Xáo trộn rổ heo thường
        ShuffleList(poolHeoThuong);

        // BƯỚC 3: Gộp lại vào các cột
        foreach (var key in inputDict.Keys)
        {
            int totalLength = originalLengths[key];

            // Tạo một mảng trống (null) có độ dài bằng cột gốc để dễ xếp chỗ
            PigComponent[] newColumnArray = new PigComponent[totalLength];

            var vipPigs = vipPigsDict[key];
            var vipIndices = vipIndicesDict[key];

            // Nếu cột này CÓ heo nối dây
            if (vipPigs.Count > 0)
            {
                // Tìm index nhỏ nhất và lớn nhất của heo VIP trong cột này
                int minIndex = vipIndices[0];
                int maxIndex = vipIndices[vipIndices.Count - 1];

                // Tính toán giới hạn n (offset) để KHÔNG bao giờ bị Out Of Range
                // min_n là số bước lùi tối đa (âm), max_n là số bước tiến tối đa (dương)
                int min_n = -minIndex;
                int max_n = totalLength - 1 - maxIndex;

                // Random giá trị n trong giới hạn an toàn (Random.Range với int thì max phải + 1)
                int n = UnityEngine.Random.Range(min_n, max_n + 1);

                // Xếp các heo nối dây vào mảng mới với index đã được cộng thêm n
                for (int k = 0; k < vipPigs.Count; k++)
                {
                    int newIndex = vipIndices[k] + n;
                    newColumnArray[newIndex] = vipPigs[k];
                }
            }

            // BƯỚC 4: Lấp đầy các khoảng trống (null) trong mảng bằng heo thường
            for (int i = 0; i < totalLength; i++)
            {
                if (newColumnArray[i] == null)
                {
                    int lastIndex = poolHeoThuong.Count - 1;
                    newColumnArray[i] = poolHeoThuong[lastIndex];
                    poolHeoThuong.RemoveAt(lastIndex); // Rút heo thường ra khỏi rổ
                }
            }

            // Chuyển mảng thành List và lưu vào kết quả
            resultDict[key] = new List<PigComponent>(newColumnArray);
        }

        return resultDict;
    }

}
