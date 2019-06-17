using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMap : MonoBehaviour
{
    public TileType[] tileTypes;

    int[,] tiles; //2D Integer array for showing which tiles are passable and which aren't

    int mapSizeX = 10;
    int mapSizeY = 10;

    private void Start()
    {
        //Allocate Map tiles
        tiles = new int[mapSizeX, mapSizeY];

        //Initialize map
        for(int i=0; i<mapSizeX; i++)
        {
            for(int j=0; j<mapSizeY; j++)
            {
                tiles[i, j] = 0;
            }
        }
        tiles[4, 4] = 1;
        tiles[5, 4] = 1;
        tiles[6, 4] = 1;
        tiles[7, 4] = 1;
        tiles[8, 4] = 1;
        tiles[8, 5] = 1;
        tiles[8, 6] = 1;

        //Spawn the actual tiles
        GenerateMapVisuals();
    }
    //Generates the actual tiles from the given map
    void GenerateMapVisuals()
    {
        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeY; j++)
            {
                TileType tt = tileTypes[tiles[i, j]];
                Instantiate(tt.tileVisualPrefab,new Vector3(i,j,0), Quaternion.identity);
            }
        }
    }
}
