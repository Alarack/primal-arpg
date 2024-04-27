using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilesetHelper : MonoBehaviour
{
    public TileDatabase tileDatabase;

    public List<GameObject> tileList = new List<GameObject>();

    public Transform floorRoot;
    public int roomHeight;
    public int roomWidth;
    public float xSpacing;
    public float ySpacing;
    public int spriteOrderInLayer;

    public GameObject CreateRandomFloorTile()
    {
        GameObject floorTile = new GameObject();
        SpriteRenderer renderer = floorTile.AddComponent<SpriteRenderer>();
        renderer.sprite = tileDatabase.baseFloorTile;
        floorTile.layer = spriteOrderInLayer;

        GameObject decal = new GameObject();
        decal.transform.SetParent(floorTile.transform);
        decal.transform.localPosition = Vector2.zero;
        decal.layer = spriteOrderInLayer;

        SpriteRenderer floorDecal = decal.AddComponent<SpriteRenderer>();
        floorDecal.sprite = tileDatabase.GetRandomDecal();

        return floorTile;
    }

    public void CreateFloorGrid()
    {
        ClearFloorGrid();

        for (int x = 0; x < roomWidth; x++)
        {
            for (int y = 0; y < roomHeight; y++)
            {
                GameObject floorTile = CreateRandomFloorTile();
                floorTile.transform.SetParent(floorRoot, false);

                floorTile.transform.localPosition = new Vector3(x * (xSpacing + 1), y * (ySpacing + 1), 0);

                tileList.Add(floorTile);
            }
        }
    }

    public void ClearFloorGrid()
    {
        for (int i = 0; i < tileList.Count; i++)
        {
            DestroyImmediate(tileList[i]);
        }
    }

}
