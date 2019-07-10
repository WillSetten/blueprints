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

    //If a tile is occupied, this method is used to find the closest available tile in the room
    public Tile findBestNextTile(Tile tile)
    {
        Tile currentNextBestTile = null;
        //For each tile in the room
        foreach (Tile t in tiles)
        {
            //If the tile is not occupied
            if (!t.occupied)
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
}
