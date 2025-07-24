using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RoomPortalDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{



    [Header("Portals")]
    public GameObject defaultPortal;
    public GameObject unstablePortal;
    public SerializableDictionary<Room.RoomType, GameObject> portals = new SerializableDictionary<Room.RoomType, GameObject>();

    public Image roomRewardIcon;

    [Header("VFX")]
    public ParticleSystem unstableVFX;

    private Room room;

    public void Setup(Room room) {
        this.room = room;
        SetupDisplay();
    }

    private void SetupDisplay() {

        if (room.DisplayReward != null) {
            Sprite rewardSprite = ItemSpawner.Instance.lootDatabase.GetItemIconByType(room.DisplayReward.itemCategory);
            if (rewardSprite != null) {
                roomRewardIcon.sprite = rewardSprite;
            }
        }


        if(room.Unstable == true) {
            defaultPortal.SetActive(false);
            unstablePortal.SetActive(true);
            unstableVFX.Play();
            return;
        }
        else {
            unstablePortal.SetActive(false);
            unstableVFX.Stop();
        }


        if(portals.ContainsKey(room.Type) == false) {
            defaultPortal.SetActive(true);
        }
        else {
            portals[room.Type].SetActive(true);
            defaultPortal.SetActive(false);
        }



    }

    #region UI CALLBACKS

    public void OnPointerEnter(PointerEventData eventData) {

        StringBuilder builder = new StringBuilder();

        string roomName = room.Type.ToString().SplitCamelCase();

        builder.Append(TextHelper.ColorizeText(roomName, Color.cyan));

        builder.AppendLine();

        builder.AppendLine(room.DisplayReward.rewardDescription/*items[0].itemData.GetItemInfo()*/);

        if(room.Unstable == true) {
            builder.AppendLine();
            builder.AppendLine(TextHelper.ColorizeText("Unstable", ColorDataManager.GetColorByName("Unstable")));
        }


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
        PanelManager.OpenPanel<TransitionPanel>().Setup(FinishRoomTransition);

       
        TooltipManager.Hide();
    }

    private void FinishRoomTransition() {
        RoomManager.OnRoomSelected(room);
    }

    #endregion


}
