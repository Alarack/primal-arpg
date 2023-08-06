using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RoomPortalDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{

    public Image roomRewardIcon;

    private Room room;

    public void Setup(Room room) {
        this.room = room;
        SetupDisplay();
    }

    private void SetupDisplay() {

    }

    #region UI CALLBACKS

    public void OnPointerEnter(PointerEventData eventData) {

        StringBuilder builder = new StringBuilder();

        string roomName = room.Type.ToString().SplitCamelCase();

        builder.Append(TextHelper.ColorizeText(roomName, Color.cyan));

        builder.AppendLine();

        builder.AppendLine(room.rewards[0].rewardDescription/*items[0].itemData.GetItemInfo()*/);

        TooltipManager.Show(builder.ToString());

        //string roomType = room.Type switch {
        //    Room.RoomType.StartRoom => throw new System.NotImplementedException(),
        //    Room.RoomType.EliminationCombat => throw new System.NotImplementedException(),
        //    Room.RoomType.ItemShop => throw new System.NotImplementedException(),
        //    Room.RoomType.SkillShop => throw new System.NotImplementedException(),
        //    Room.RoomType.RecoveryRoom => throw new System.NotImplementedException(),
        //    Room.RoomType.SurvivalCombat => throw new System.NotImplementedException(),
        //    Room.RoomType.BossRoom => throw new System.NotImplementedException(),
        //    Room.RoomType.SecretRoom => throw new System.NotImplementedException(),
        //    Room.RoomType.TreasureRoom => throw new System.NotImplementedException(),
        //    Room.RoomType.MiniBossRoom => throw new System.NotImplementedException(),
        //    Room.RoomType.EventRoom => throw new System.NotImplementedException(),
        //};




    }

    public void OnPointerExit(PointerEventData eventData) {
        TooltipManager.Hide();
    }

    public void OnPointerClick(PointerEventData eventData) {
        RoomManager.OnRoomSelected(room);
        TooltipManager.Hide();
    }

    #endregion


}
