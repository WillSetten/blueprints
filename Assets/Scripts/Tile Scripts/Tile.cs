using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int tileX;
    public int tileY;
    public TileMap map;

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            map.GeneratePathTo(tileX, tileY, map.selectedUnit.GetComponent<Unit>());

            Debug.Log("Goal Tile :" + tileX + "," + tileY);
        }
    }
}
