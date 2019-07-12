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
        /*if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Clicked on Tile " + tileX + "," + tileY);
        }*/
        if (Input.GetMouseButtonDown(1) && this!=map.lastSelectedTile)
        {
            if (!map.multipleUnitsSelected)
            {
                map.GeneratePathTo(tileX, tileY, map.selectedUnit.GetComponent<Unit>());
            }
            else
            {
                foreach(GameObject u in map.selectedUnits)
                {
                    map.GeneratePathTo(tileX, tileY, u.GetComponent<Unit>());
                }
            }
            map.lastSelectedTile = this;
        }
    }
}
