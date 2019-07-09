using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public bool occupied;
    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1) && !occupied)
        {
            map.GeneratePathTo(tileX, tileY, map.selectedUnit.GetComponent<Unit>());

            Debug.Log("Goal Tile :" + tileX + "," + tileY);
            occupied = true;
        }
    }
}
