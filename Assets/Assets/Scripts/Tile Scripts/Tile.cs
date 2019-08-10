using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tile : MonoBehaviour
{
    //Location of the tile
    public int tileX;
    public int tileY;
    //Map which the tile is on
    public TileMap map;
    //Room which the tile is part of
    public Room room;
    //Whether the tile is occupied or not.
    public bool occupied=false;
    //Is passable
    public bool impassable;
    //If a unit is currently travelling to this tile
    public bool isDestination = false;
    void OnMouseOver()
    {
        /*if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Clicked on Tile " + tileX + "," + tileY);
        }*/
        if (Input.GetMouseButtonDown(1) && this!=map.lastSelectedTile)
        {
            if (!map.multipleUnitsSelected)
            {
                map.GeneratePathTo(tileX, tileY, map.selectedUnit.GetComponent<Unit>());
            }
            else
            {
                foreach(GameObject u in map.selectedUnits)
                {
                    map.GeneratePathTo(tileX, tileY, u.GetComponent<Unit>());
                }
            }
            map.lastSelectedTile = this;
        }
    }

    private void Update()
    {
        //if there is not a unit on this tile
        if (Physics2D.OverlapCircleAll((Vector2)transform.position, 0.1f, LayerMask.GetMask("EnemyUnits", "PlayerUnits", "CivilianUnits", "Loot")).Length==0)
        {
            occupied = false;
        }
        //If there is a unit over this tile
        else
        {
            Collider2D collider = Physics2D.OverlapCircleAll((Vector2)transform.position, 0.1f, LayerMask.GetMask("EnemyUnits", "PlayerUnits", "CivilianUnits", "Loot"))[0];
            if (collider.GetComponent<Loot>())
            {
                occupied = true;
            }
            else if (collider.GetComponent<Rigidbody2D>().velocity == Vector2.zero) {
                occupied = true;
                isDestination = false;
                GetComponent<SpriteRenderer>().color = Color.white;
            }
        }
    }
    
}
