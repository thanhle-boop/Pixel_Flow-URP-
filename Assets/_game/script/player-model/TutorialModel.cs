using System;
using System.Collections.Generic;
using R3;

public class TutorialItem : IFileStreamObject
{
    public string tutorialKey;
    public int currentStep;
    public int totalSteps;
    public ReactiveProperty<bool> isCompleted;

    public int ModelVersion => 1;

    public void ReadOrWrite(IFileStream stream, int version)
    {
        stream.ReadOrWriteString(ref tutorialKey, nameof(tutorialKey));
        stream.ReadOrWriteInt(ref currentStep, nameof(currentStep));
        stream.ReadOrWriteInt(ref totalSteps, nameof(totalSteps));
        stream.ReadOrWriteRxBool(ref isCompleted, nameof(isCompleted));
    }

}
public class TutorialModel : BasePlayerModel
{
    public List<TutorialItem> mechanicTutorials = new();
    public List<TutorialItem> boosterTutorials = new();
    public List<TutorialItem> guide = new();

    public override int ModelVersion => 1;
    public override void ReadOrWrite(IFileStream stream, int version)
    {
        stream.ReadOrWriteListObj(ref mechanicTutorials, nameof(mechanicTutorials));
        stream.ReadOrWriteListObj(ref boosterTutorials, nameof(boosterTutorials));
        stream.ReadOrWriteListObj(ref guide, nameof(guide));
    }

    public override void OnModelInitializing()
    {
        base.OnModelInitializing();

        AddBoosterTutorial(BoosterTutorialType.Booster_AddTray, 2);
        AddBoosterTutorial(BoosterTutorialType.Booster_Balloon, 3);
        AddBoosterTutorial(BoosterTutorialType.Booster_Shuffle, 2);
        AddBoosterTutorial(BoosterTutorialType.Booster_Super, 3);

        AddGuideTutorial(GuideTutorialType.Level_1, 3);
        AddGuideTutorial(GuideTutorialType.Level_2, 1);
        AddGuideTutorial(GuideTutorialType.Full_slot, 1);

        foreach (MechanicTutorialType type in Enum.GetValues(typeof(MechanicTutorialType)))
        {
            mechanicTutorials.Add(new TutorialItem
            {
                tutorialKey = type.ToString(),
                currentStep = 0,
                totalSteps = 1,
                isCompleted = new ReactiveProperty<bool>(false)
            });
        }
    }

    private void AddBoosterTutorial(BoosterTutorialType type, int steps)
    {
        boosterTutorials.Add(new TutorialItem
        {
            tutorialKey = type.ToString(),
            currentStep = 0,
            totalSteps = steps,
            isCompleted = new ReactiveProperty<bool>(false)
        });
    }

        private void AddGuideTutorial(GuideTutorialType type, int steps)
    {
        guide.Add(new TutorialItem
        {
            tutorialKey = type.ToString(),
            currentStep = 0,
            totalSteps = steps,
            isCompleted = new ReactiveProperty<bool>(false)
        });
    }

    public TutorialItem GetTutorialItem(string key)
    {
        return mechanicTutorials.Find(x => x.tutorialKey == key)
               ?? boosterTutorials.Find(x => x.tutorialKey == key)
               ?? guide.Find(x => x.tutorialKey == key);
    }

    public bool IsTutorialDone(string key)
    {
        var item = GetTutorialItem(key);
        return item?.isCompleted.Value ?? false;
    }

    public int GetCurrentStep(string key)
    {
        var item = GetTutorialItem(key);
        return item?.currentStep ?? 0;
    }

}