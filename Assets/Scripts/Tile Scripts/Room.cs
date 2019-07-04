using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Room : MonoBehaviour
{
    //The map which the room is on
    TileMap map;
    //An integer array to keep track of which tiles are occupied
    int[,] tiles;
    //The position of the room in the tile map
    int sizeX;
    int sizeY;
}
