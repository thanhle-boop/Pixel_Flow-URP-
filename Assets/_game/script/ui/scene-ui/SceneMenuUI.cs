using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;

public class SceneMenuUI : MonoBehaviour
{
    [SerializeField] Button btnSettings;

    void Start()
    {
        btnSettings.OnClickAsObservable()
            .Subscribe(_ =>
            {
                PopupManager.instance.OpenPopup<PopupSettings>().Forget();
            }).AddTo(this);
    }
}