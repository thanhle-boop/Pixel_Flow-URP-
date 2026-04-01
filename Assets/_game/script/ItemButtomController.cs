
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemButtomController : MonoBehaviour
{
    public Sprite lockIcon;
    public Sprite lockPanel;
    public Sprite unlockIcon;
    public Sprite unlockPanel;

    public Image panelSprite;
    public Image iconSprite;
    
    public Sprite greenSprite;
    public Sprite blueSprite;
    public TextMeshProUGUI countText;
    public GameObject plusIcon;
    public GameObject countObject;

    public int itemType;

    void OnEnable()
    {
        EventManager.OnStartGame += UpdateItemStatus;

        EventManager.OnItemCountChanged += OnItemCountChanged;
    }
    void OnDisable()
    {
        EventManager.OnStartGame -= UpdateItemStatus;

        EventManager.OnItemCountChanged -= OnItemCountChanged;

    }

    private void OnItemCountChanged(int itemType, int newValue)
    {
        if (this.itemType == itemType)
        {
            if (newValue > 0)
            {
                countText.text = newValue.ToString();
                countObject.GetComponent<Image>().sprite = blueSprite;
                plusIcon.SetActive(false);
            }
            else
            {
                countText.text = "";
                countObject.GetComponent<Image>().sprite = greenSprite;
                plusIcon.SetActive(true);
            }
        }
    }

    private void UpdateItemStatus()
    {

        var isOpen = DataManager.instance.GetStatus(itemType);


        if (isOpen)
        {
            iconSprite.sprite = unlockIcon;
            panelSprite.sprite = unlockPanel;

            OnItemCountChanged(itemType, DataManager.instance.GetItemCount(itemType));
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

