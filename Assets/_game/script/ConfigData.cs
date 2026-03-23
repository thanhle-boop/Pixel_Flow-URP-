using System;
using System.Collections.Generic;

// [Serializable]
// public class PigConfig {
//     public string colorName;
//     public int bullets;
//     public bool isHidden = false;
//     public PigMarker pigLeft = null;
//     public PigMarker pigRight = null;
// }

[Serializable]
public class LaneConfig {
    public List<PigLayoutData> pigs;
}

[Serializable]
public class PigMarker {
    public int LaneIndex = -1;
    public int index = -1;
    public bool IsValid()
    {
        return LaneIndex != -1 && index != -1;
    }
}



