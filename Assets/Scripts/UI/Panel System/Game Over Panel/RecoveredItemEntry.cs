using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RecoveredItemEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {


    public Image itemImage;
    public ItemDefinition Item { get; private set; }


    private Item displayItem;
    private GameOverPanel gameOverPanel;

    public void Setup(ItemDefinition item, GameOverPanel gameOverPanel) {
        this.Item = item;
        this.gameOverPanel = gameOverPanel;
        itemImage.sprite = item.itemData.itemIcon;
        displayItem = item.itemData.GetDisplayItem();
    }
    
    
    
    public void OnPointerClick(PointerEventData eventData) {
        gameOverPanel.OnRecoveredItemSelected(this);
    }

    public void Select() {
        SaveLoadUtility.SaveData.AddRecoveredItem(Item);
        SaveLoadUtility.SavePlayerData();
        TooltipManager.Hide();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        TooltipManager.Show(displayItem.GetTooltip(), TextHelper.ColorizeText(displayItem.Data.itemName, ColorDataManager.Instance["Item Header"]));

    }

    public void OnPointerExit(PointerEventData eventData) {
        TooltipManager.Hide();
    }
}
