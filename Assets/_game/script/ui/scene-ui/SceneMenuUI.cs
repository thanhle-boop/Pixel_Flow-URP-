using UnityEngine;

public class SceneMenuUI : MonoBehaviour
{
    private void Start()
    {
        AudioGameManger.instance.InitAudioGameManager();
        AudioController.instance.PlaySound(AudioIndex.bgm.ToString());
    }
}