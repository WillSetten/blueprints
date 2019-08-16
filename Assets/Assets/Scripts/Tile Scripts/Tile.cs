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
    //Tile is currently blocked
    public bool blocked;
    //Is a passable tile 
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
                foreach(GameObject u in map.selectedUnits)
                {
                    map.GeneratePathTo(tileX, tileY, u.GetComponent<Unit>());
                }
            map.lastSelectedTile = this;
        }
    }

    private void Update()
    {
        //if there is not a unit on this tile
        if (Physics2D.OverlapCircleAll((Vector2)transform.position, 0.1f, LayerMask.GetMask("EnemyUnits", "PlayerUnits", "CivilianUnits", "Loot", "Vehicles", "Vault")).Length==0)
        {
            occupied = false;
            blocked = false;
        }
        //If there is a unit over this tile
        else
        {
            Collider2D collider = Physics2D.OverlapCircleAll((Vector2)transform.position, 0.5f, LayerMask.GetMask("EnemyUnits", "PlayerUnits", "CivilianUnits", "Loot", "Vehicles", "Vault"))[0];
            if (collider.GetComponent<Loot>()|| collider.GetComponent<Truck>()|| collider.GetComponent<VaultDoor>())
            {
                occupied = true;
                blocked = true;
            }
            else if (collider.GetComponent<Unit>()&&collider.GetComponent<Rigidbody2D>().velocity == Vector2.zero) {
                if (collider.GetComponent<Unit>().isLarge) {
                    blocked = true;
                }
                occupied = true;
                isDestination = false;
                GetComponent<SpriteRenderer>().color = Color.white;
            }
            else
            {
                occupied = false;
                blocked = false;
            }
        }
    }
    
}
