using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCoin : MonoBehaviour
{
    [SerializeField] Button btnAddCoin;
    [SerializeField] Image imgCoin;
    [SerializeField] TextMeshProUGUI txtCoin;

    void Start()
    {
        DataManager.instance.CoinsRx
            .Subscribe(coins =>
            {
                txtCoin.text = coins.ToString();
            }).AddTo(this);
    }
}
