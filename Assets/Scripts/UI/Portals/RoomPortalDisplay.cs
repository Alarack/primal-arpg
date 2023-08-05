using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RoomPortalDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{

    public Image roomRewardIcon;

    private Room room;

    private RoomPortal portal;

    public void Setup(Room room, RoomPortal portal) {
        this.room = room;
        this.portal = portal;

        SetupDisplay();
    }

    private void SetupDisplay() {

    }

    #region UI CALLBACKS

    public void OnPointerEnter(PointerEventData eventData) {


        Debug.Log("Room Portal mouse over");

        //switch (displayItem.Data.Type) {
        //    case ItemType.None:
        //        break;
        //    case ItemType.Equipment:
        //        TooltipManager.Show(displayItem.GetTooltip(), TextHelper.ColorizeText(displayItem.Data.itemName, ColorDataManager.Instance["Burnt Orange"]));
        //        break;
        //    case ItemType.Rune:
        //        break;
        //    case ItemType.Currency:
        //        break;
        //    case ItemType.Skill:
        //        TooltipManager.Show(displayAbility.GetTooltip(), displayAbility.Data.abilityName);
        //        break;
        //    default:
        //        break;
        //}

    }

    public void OnPointerExit(PointerEventData eventData) {
        TooltipManager.Hide();
    }

    public void OnPointerClick(PointerEventData eventData) {
        RoomManager.EnterRoom(room);
        TooltipManager.Hide();
    }

    #endregion


}
