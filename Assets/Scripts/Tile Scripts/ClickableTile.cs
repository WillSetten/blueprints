﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableTile : MonoBehaviour
{
    public int tileX;
    public int tileY;
    public TileMap map;

    void OnMouseUp()
    {
        Debug.Log("Tile " + tileX + "," + tileY);

        map.GeneratePathTo(tileX, tileY);
    }
}
