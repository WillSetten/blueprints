using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    //The map which the room is on
    TileMap map;
    //An integer array to keep track of which tiles are occupied
    int[,] tiles;
    //The position of the room in the tile map

    public Room(TileMap newMap, int sizeX, int sizeY, int bottomLeftX, int bottomLeftY)
    {
        map = newMap;
        tiles = new int[sizeX, sizeY];
        for(int x=0; x<sizeX; x++)
        {
            for(int y=0; y<sizeY; y++)
            {
                map.tiles[bottomLeftX+x, bottomLeftY+y].room = this;
                tiles[x, y] = 0;
            }
        }
    }


}
