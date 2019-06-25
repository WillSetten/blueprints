using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableTile : MonoBehaviour
{
    public int tileX;
    public int tileY;
    public TileMap map;

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            map.GeneratePathTo(tileX, tileY, map.selectedUnit);

            Debug.Log("Goal Tile :" + tileX + "," + tileY);
        }
    }
}
