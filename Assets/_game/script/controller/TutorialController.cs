using R3;

public static class TutorialController
{
    private static TutorialModel Model => PlayerModelManager.instance.GetPlayerModel<TutorialModel>();

    // Hàm helper lấy Item công khai
    public static TutorialItem GetTutorialItem(string key)
    {
        return Model.GetTutorialItem(key);
    }

    public static bool IsCompleted(string key)
    {
        var item = GetTutorialItem(key);
        // Cần .Value vì isCompleted là ReactiveProperty<bool>
        return item != null && item.isCompleted.Value;
    }

    // Lấy ReadOnly để View không thể tự ý sửa giá trị từ bên ngoài
    public static ReadOnlyReactiveProperty<bool> IsCompletedRx(string key)
    {
        var item = GetTutorialItem(key);
        return item?.isCompleted;
    }

    public static int GetCurrentStep(string key)
    {
        var item = GetTutorialItem(key);
        return item?.currentStep.Value ?? 0;
    }

    public static void AdvanceStep(string key)
    {
        var item = GetTutorialItem(key);
        if (item == null || item.isCompleted.Value) return;

        item.currentStep.Value++;

        if (item.currentStep.Value >= item.totalSteps)
        {
            item.isCompleted.Value = true;
        }

        Model.Save();
    }

    public static void CompleteTutorial(string key)
    {
        var item = GetTutorialItem(key);
        if (item != null)
        {
            item.isCompleted.Value = true;
            item.currentStep.Value = item.totalSteps;
            Model.Save();
        }
    }

    public static void ResetTutorialStep(string key)
    {
        var item = GetTutorialItem(key);
        if (item == null || item.isCompleted.Value) return;

        // Reset step về 0
        item.currentStep.Value = 0;
        
        // Lưu lại trạng thái mới
        Model.Save();
        // Debug.Log($"[ResetTutorial] {key} has been reset to step 0");
    }
}