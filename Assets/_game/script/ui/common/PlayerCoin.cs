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
        CurrencyController.GetGoldRx()
            .Subscribe(coins =>
            {
                var gold = CurrencyController.GetGold();
                txtCoin.text = gold < 0 ? "0" : gold.ToString();
            }).AddTo(this);
    }
}
