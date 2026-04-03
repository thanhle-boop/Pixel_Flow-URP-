using UnityEngine;

public class SceneMenuUI : MonoBehaviour
{
    private void Start()
    {
        AudioGameManger.instance.InitAudioGameManager();
        AudioController.instance.PlayMusic(AudioIndex.bgm.ToString());
    }
}