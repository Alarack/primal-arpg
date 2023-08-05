using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Data/Room Database", fileName = "Room Database")]
public class RoomDatabase : ScriptableObject
{

    public List<RoomDatEntry> entries = new List<RoomDatEntry>();


    [System.Serializable]
    public class RoomDatEntry {
        public Room.RoomType type;
        public float weight;
    }

}
