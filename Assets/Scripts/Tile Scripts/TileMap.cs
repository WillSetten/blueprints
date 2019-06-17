using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMap : MonoBehaviour
{
    //Each unit should have a click handler which, when selected, should make this variable equal to that unit.
    public GameObject selectedUnit;
    public TileType[] tileTypes;

    int[,] tiles; //2D Integer array for showing which tiles are passable and which aren't

    int mapSizeX = 10;
    int mapSizeY = 10;

    private void Start()
    {
        GenerateMapData();
        GenerateMapVisuals();
    }

    //Decides how large the map is, which tiles (floor, walls) go where in the map
    void GenerateMapData()
    {
        //Allocate Map tiles
        tiles = new int[mapSizeX, mapSizeY];

        //Initialize map
        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeY; j++)
            {
                tiles[i, j] = 0;
            }
        }
        tiles[3, 4] = 1;
        tiles[4, 4] = 1;
        tiles[5, 4] = 1;
        tiles[6, 4] = 1;
        tiles[7, 4] = 1;
        tiles[8, 4] = 1;
        tiles[8, 5] = 1;
        tiles[8, 6] = 1;
    }

    //Generates the actual tiles from the given map
    void GenerateMapVisuals()
    {
        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeY; j++)
            {
                TileType tt = tileTypes[tiles[i, j]];
                GameObject go = (GameObject)Instantiate(tt.tileVisualPrefab, new Vector3(i, j, 0), Quaternion.identity);

                ClickableTile ct = go.GetComponent<ClickableTile>();
                ct.tileX = i;
                ct.tileY = j;
                ct.map = this;
            }
        }
    }

    //Converts Tile Co-ordinates with Co-ordinates in the actual world. For now, this is just a matter of converting two ints into
    //a Vector3. Any scaling issues will probably have to be fixed in this method.
    public Vector3 TileCoordToWorldCoord(int x, int y)
    {
        return new Vector3(x, y, 0);
    }

    //Takes in an x and y to move the selected unit to.
    public void MoveSelectedUnitTo(int x, int y)
    {
        //Update the units information so that it knows where it is.
        selectedUnit.GetComponent<Unit>().tileX = x;
        selectedUnit.GetComponent<Unit>().tileY = y;
        //Move the position of the unit.
        selectedUnit.transform.position = TileCoordToWorldCoord(x, y);
    }

}
