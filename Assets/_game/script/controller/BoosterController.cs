using R3;

public static class BoosterController
{
    static BoosterModel boosterModel => PlayerModelManager.instance.GetPlayerModel<BoosterModel>();

    public static BoosterModelItem GetBooster(BoosterIndex boosterIndex)
    {
        var ret = boosterModel.boosterItems.Find(item => item.boosterIndex == boosterIndex);

        if (ret == null)
        {
            ret = new BoosterModelItem()
            {
                boosterIndex = boosterIndex,
                countRx = new ReactiveProperty<int>(0)
            };

            boosterModel.boosterItems.Add(ret);
            boosterModel.Save();
        }

        return ret;
    }

    public static ReactiveProperty<int> GetBoosterCountRx(BoosterIndex boosterIndex)
    {
        return GetBooster(boosterIndex).countRx;
    }

    public static int GetBoosterCount(BoosterIndex boosterIndex)
    {
        return GetBoosterCountRx(boosterIndex).Value;
    }

    public static void AddBooster(BoosterIndex boosterIndex, int count)
    {
        var booster = GetBooster(boosterIndex);
        booster.countRx.Value += count;

        boosterModel.Save();
    }

    public static void SubtractBooster(BoosterIndex boosterIndex, int count)
    {
        var booster = GetBooster(boosterIndex);
        booster.countRx.Value -= count;

        boosterModel.Save();
    }

    public static bool IsCanUseBooster(BoosterIndex boosterIndex)
    {
        if (!IsBoosterAvailable(boosterIndex))
        {
            return false;
        }

        return GetBoosterCount(boosterIndex) > 0;
    }

    public static bool IsBoosterAvailable(BoosterIndex boosterIndex)
    {
        switch (boosterIndex)
        {
            case BoosterIndex.tray: return HardCodeInGame.TRAY_AVAILABLE;
            case BoosterIndex.hand: return HardCodeInGame.HAND_AVAILABLE && !SceneGameplayUI.instance.isBottomUiTranslate;
            case BoosterIndex.shuffle: return HardCodeInGame.SHUFFLE_AVAILABLE;
            case BoosterIndex.super: return HardCodeInGame.SUPER_AVAILABLE;
            default: return false;
        }
    }

    public static void HandleUseBooster(BoosterIndex boosterIndex)
    {
        switch (boosterIndex)
        {
            case BoosterIndex.tray:
                UIManager.Instance.straightSlot.SetActive(true);
                EventManager.OnUseAddTray?.Invoke();
                break;
            case BoosterIndex.hand:
                EventManager.OnUseHand?.Invoke();
                SceneGameplayUI.instance.HandleBoosterHand();
                break;
            case BoosterIndex.shuffle:
                EventManager.OnUseShuffle?.Invoke();
                break;
            case BoosterIndex.super:
                EventManager.OnUseSuperCat?.Invoke();
                break;
        }

        SubtractBooster(boosterIndex, 1);
    }

    public static void CompledtedTutorialStep(BoosterIndex boosterIndex)
    {
        BoosterTutorialType boosterTutorial = BoosterTutorialType.None;
        switch (boosterIndex)
        {
            case BoosterIndex.tray:
                boosterTutorial = BoosterTutorialType.Booster_AddTray;
                break;
            case BoosterIndex.hand:
                boosterTutorial = BoosterTutorialType.Booster_Balloon;
                break;
            case BoosterIndex.shuffle:
                boosterTutorial = BoosterTutorialType.Booster_Shuffle;
                break;
            case BoosterIndex.super:
                boosterTutorial = BoosterTutorialType.Booster_Super;
                break;
        }

        if (boosterTutorial != BoosterTutorialType.None && boosterTutorial != BoosterTutorialType.Booster_Balloon && boosterTutorial != BoosterTutorialType.Booster_Super)
        {
            SceneGameplayUI.instance.CompleteFirstStepTutorial(boosterTutorial);
        }
    }
}
