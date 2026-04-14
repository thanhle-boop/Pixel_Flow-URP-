using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemButtomController : MonoBehaviour
{
    public Image panelSprite;
    public Image iconSprite;

    public TextMeshProUGUI countText;
    public GameObject plusIcon;
    public GameObject countObject;

    [SerializeField] Button btnUseBooster;

    [Header("sprite")]
    public Sprite lockIcon;
    public Sprite lockPanel;
    public Sprite unlockIcon;
    public Sprite unlockPanel;

    public Sprite greenSprite;
    public Sprite blueSprite;

    public BoosterIndex itemType;

    void OnEnable()
    {
        EventManager.OnStartGame += UpdateItemStatus;
    }

    void OnDisable()
    {
        EventManager.OnStartGame -= UpdateItemStatus;
    }

    private void Start()
    {
        BoosterController.GetBoosterCountRx(itemType)
           .Subscribe(val =>
           {
               bool isHaveValue = val > 0;

               countText.text = isHaveValue ? val.ToString() : "";
               countObject.GetComponent<Image>().sprite = isHaveValue ? blueSprite : greenSprite;
               plusIcon.SetActive(!isHaveValue);
           }).AddTo(this);

        btnUseBooster.OnClickAsObservable()
            .Subscribe(_ =>
            {
                if (BoosterController.IsCanUseBooster(itemType))
                {
                    BoosterController.HandleUseBooster(itemType);
                    BoosterController.CompledtedTutorialStep(itemType);
                }
            }).AddTo(this);
    }

    private void UpdateItemStatus()
    {
        var isOpen = BoosterController.IsBoosterAvailable(itemType);

        if (isOpen)
        {
            iconSprite.sprite = unlockIcon;
            panelSprite.sprite = unlockPanel;

            countObject.SetActive(true);
        }
        else
        {
            iconSprite.sprite = lockIcon;
            panelSprite.sprite = lockPanel;
            countObject.SetActive(false);
        }
    }
}

