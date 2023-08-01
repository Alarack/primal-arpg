using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : Singleton<RoomManager>
{


    public Room CurrentRoom { get; private set; }

    private int currentRoomIndex;
    private List<Room> roomList = new List<Room>();






}
