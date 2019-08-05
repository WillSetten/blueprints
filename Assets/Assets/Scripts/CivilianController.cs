using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CivilianController : MonoBehaviour
{
    private TileMap map;
    public List<Unit> units;
    System.Random rnd;
    //How frequently the unit is given a new path
    float setTimer;
    float timer = 0;
    public bool active;

    // Start is called before the first frame update
    void Start()
    {
        map = GetComponentInParent<TileMap>();
        units.AddRange(GetComponentsInChildren<Unit>());
        rnd = new System.Random();
        setTimer = 4;
    }

    // Update is called once per frame
    void Update()
    {
        if (!map.paused && active)
        {
            bool hasDetectedaPlayerUnit = false;
            foreach (Unit u in units)
            {
                //If the alarm could be raised in this update, make this variable true
                if (u.detectedPlayerUnit)
                {
                    hasDetectedaPlayerUnit = true;
                }
                if (setTimer < timer && u.currentState==Unit.state.Idle)
                {
                    timer = 0;
                    setTimer = rnd.Next(4, 8);
                    Tile newTile = map.tiles[u.tileX, u.tileY].room.randomTileInRoom();
                    map.GeneratePathTo(newTile.tileX, newTile.tileY, u);
                }
                else
                {
                    timer = timer + Time.deltaTime;
                }
            }
        }
    }
}
