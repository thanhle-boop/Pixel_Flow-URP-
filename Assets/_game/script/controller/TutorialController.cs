using System.Collections.Generic;

public static class TutorialController
{
    // Truy cập nhanh vào Model thông qua Manager
    private static TutorialModel tutorialModel => PlayerModelManager.instance.GetPlayerModel<TutorialModel>();

    /// <summary>
    /// Kiểm tra xem một tutorial cụ thể đã hoàn thành chưa
    /// </summary>
    public static bool IsCompleted(string tutorialKey)
    {
        var item = FindItem(tutorialKey);
        return item != null && item.isCompleted;
    }

    /// <summary>
    /// Đánh dấu hoàn thành tutorial và lưu lại ngay lập tức
    /// </summary>
    public static void MarkAsCompleted(string tutorialKey)
    {
        var item = FindItem(tutorialKey);
        if (item != null && !item.isCompleted)
        {
            item.isCompleted = true;
            tutorialModel.Save(); // Lưu xuống máy và chuẩn bị sync server
            
            // Thông báo cho UI cập nhật nếu cần
            // EventManager.OnTutorialUpdated?.Invoke(tutorialKey);
        }
    }

    /// <summary>
    /// Tìm item trong cả 2 danh sách Mechanic và Booster
    /// </summary>
    private static TutorialItem FindItem(string key)
    {
        var found = tutorialModel.mechanicTutorials.Find(x => x.tutorialKey == key);
        if (found == null)
            found = tutorialModel.boosterTutorials.Find(x => x.tutorialKey == key);
        return found;
    }
}