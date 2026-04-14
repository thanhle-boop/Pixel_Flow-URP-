using System;
using System.Collections.Generic;

public class TutorialItem : IFileStreamObject
{
    public string tutorialKey;
    public bool isCompleted;

    public int ModelVersion => 1;

    public void ReadOrWrite(IFileStream stream, int version)
    {
        stream.ReadOrWriteString(ref tutorialKey, nameof(tutorialKey));
        stream.ReadOrWriteBool(ref isCompleted, nameof(isCompleted));
    }
}
public class TutorialModel : BasePlayerModel
{
    public List<TutorialItem> mechanicTutorials = new();
    public List<TutorialItem> boosterTutorials = new();

    public override int ModelVersion => 1;

    public override void ReadOrWrite(IFileStream stream, int version)
    {
        stream.ReadOrWriteListObj(ref mechanicTutorials, nameof(mechanicTutorials));
        stream.ReadOrWriteListObj(ref boosterTutorials, nameof(boosterTutorials));
    }

    public override void OnModelInitializing()
    {
        base.OnModelInitializing();

        // Khởi tạo danh sách cho Mechanic (3 loại)
        foreach (MechanicTutorialType type in Enum.GetValues(typeof(MechanicTutorialType)))
        {
            mechanicTutorials.Add(new TutorialItem { 
                tutorialKey = type.ToString(), 
                isCompleted = false 
            });
        }

        // Khởi tạo danh sách cho Booster (4 loại)
        foreach (BoosterTutorialType type in Enum.GetValues(typeof(BoosterTutorialType)))
        {
            boosterTutorials.Add(new TutorialItem { 
                tutorialKey = type.ToString(), 
                isCompleted = false 
            });
        }
    }

    // Helper method để kiểm tra trạng thái nhanh
    public bool IsTutorialDone(string key)
    {
        var item = mechanicTutorials.Find(x => x.tutorialKey == key) 
                   ?? boosterTutorials.Find(x => x.tutorialKey == key);
        return item?.isCompleted ?? false;
    }
}