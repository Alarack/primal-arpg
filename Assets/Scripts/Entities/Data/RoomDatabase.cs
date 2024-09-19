using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName ="Data/Room Database", fileName = "Room Database")]
public class RoomDatabase : ScriptableObject
{

    public List<RoomDatEntry> entries = new List<RoomDatEntry>();


    public Sprite GetRoomSpriteByType(Room.RoomType roomType) {
        return entries.Where(t => t.type == roomType).FirstOrDefault().icon;

       
    }


    [System.Serializable]
    public class RoomDatEntry {
        public Room.RoomType type;
        public Sprite icon;
        public float weight;
    }

}
