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
    //Random number generator 
    System.Random random;

    private void Start()
    {
        random = new System.Random();
        tiles = GetComponentsInChildren<Tile>();
        foreach(Tile t in tiles)
        {
            t.room = this;
        }
    }

    //If a tile is occupied, this method is used to find the closest available tile in the room
    public Tile findBestNextTile(Tile tile, Unit unit)
    {
        Tile currentNextBestTile = null;
        //If the unit has a path, set the currentNextBestTile as the units current destination.
        if (unit.currentPath!=null)
        {
            Node currentDestination = unit.currentPath[unit.currentPath.Count - 1];
            currentNextBestTile = map.tiles[currentDestination.x,currentDestination.y];
        }
        //For each tile in the room
        foreach (Tile t in tiles)
        {
            //If the tile is not occupied
            if (!t.occupied && !t.isDestination)
            {
                //If currentNextBestTile is set to null and the current tile is unoccupied, this tile is the next best tile.
                if (currentNextBestTile == null)
                {
                    currentNextBestTile = t;
                }
                //If the current Tile is unoccupied and is closer to the destination tile than anything else, this tile is the next best tile
                else if (Vector2.Distance(new Vector2(t.tileX, t.tileY), 
                    new Vector2(tile.tileX, tile.tileY)) <
                    Vector2.Distance(new Vector2(currentNextBestTile.tileX, currentNextBestTile.tileY),
                    new Vector2(tile.tileX, tile.tileY)))
                {
                    currentNextBestTile = t;
                }
            }
        }
        return currentNextBestTile;
    }

    //Method which returns another unoccupied tile in the same room. Created to make sure that location-based civilians (staff, customers)
    //stay in their respective areas
    public Tile randomTileInRoom()
    {
        int tileIndex = 0;
        //Will iterate the same number of times as there are tiles. This is to prevent an infinite loop occuring if all
        //tiles are occupied
        for (int i=0; i < tiles.Length - 1; i++)
        {
            tileIndex = random.Next(0, tiles.Length - 1);
            if (!tiles[tileIndex].occupied && !tiles[tileIndex].isDestination)
            {
                return tiles[tileIndex];
            }
        }
        return null;
    }
}
