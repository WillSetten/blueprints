using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableTile : MonoBehaviour
{
    public int tileX;
    public int tileY;
    public TileMap map;

    void OnMouseUp()
    {
        Debug.Log("Click on tile!");

        map.GeneratePathTo(tileX, tileY);
    }
}
