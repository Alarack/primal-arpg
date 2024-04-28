using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tile Data", fileName = "Tile Database")]
public class TileDatabase : ScriptableObject
{

    public Sprite baseFloorTile;
    public List<Sprite> floorTileDecals = new List<Sprite>();
    public List<Sprite> debris = new List<Sprite>();
    public Sprite GetRandomDecal()
    {
        Sprite randomTile = floorTileDecals[Random.Range(0, floorTileDecals.Count)];

        return randomTile;
    }


    public Sprite GetRandomDebris() {
        Sprite result = debris[Random.Range(0, debris.Count)];

        return result;
    }

}
