using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tile : MonoBehaviour
{
    //Location of the tile
    public int tileX;
    public int tileY;
    //Map which the tile is on
    public TileMap map;
    //Room which the tile is part of
    public Room room;
    //Whether the tile is occupied or not.
    public bool occupied=false;
    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
                map.GeneratePathTo(tileX, tileY, map.selectedUnit.GetComponent<Unit>());
                Debug.Log("Goal Tile :" + tileX + "," + tileY);
        }
    }
}
