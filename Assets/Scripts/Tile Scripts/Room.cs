using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Room : MonoBehaviour
{
    //The map which the room is on
    public TileMap map;
    //An array to keep track of which tiles are occupied
    Tile[] tiles;

    private void Start()
    {
        tiles = GetComponentsInChildren<Tile>();
        foreach(Tile t in tiles)
        {
            t.room = this;
        }
    }
}
