using UnityEngine;


public class ReplacePanel : MonoBehaviour
{
    public LevelEditor editor;

    public void OnClickYes()
    {
        editor.ReplaceData();
    }
    public void OnClickNo()
    {
        editor.NoChange();
    }
}
