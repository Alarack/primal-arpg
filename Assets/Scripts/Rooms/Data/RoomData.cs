using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Room;

[System.Serializable]
public class RoomData
{
    public RoomType type;

    public List<EntityManager.Wave> waves = new List<EntityManager.Wave>();
    public List<RoomReward> rewards = new List<RoomReward>();
}
