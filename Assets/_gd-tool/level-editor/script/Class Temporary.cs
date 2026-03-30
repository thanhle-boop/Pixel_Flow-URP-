[System.Serializable]
public class PigDataPool
{
    public string color;
    public int bullets;
    public bool isUsed;
}

[System.Serializable]
public class PigLayoutData
{
    public string colorName;
    public int bullets;
    public bool isHidden;
    public int linkId = -1;
    // Tối đa 2 kết nối; hướng suy ra tại runtime bằng cách so sánh col/row
    public PigMarker pigLeft = null;
    public PigMarker pigRight = null;
}
