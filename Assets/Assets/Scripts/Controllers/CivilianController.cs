using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CivilianController : MonoBehaviour
{
    private TileMap map;
    public List<Unit> units;
    System.Random rnd;
    //How frequently the unit is given a new path
    float setTimer;
    float timer = 0;
    public bool active;
    public bool hasDetectedaPlayerUnit;
    public bool alarm = false;

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
            hasDetectedaPlayerUnit = false;
            foreach (Unit u in units.ToList())
            {
                if (!u.detained) {
                    //If the civilian is in a civilian escape zone
                    if ((u.tileX < 2 && u.tileY < 2) || (u.tileX > map.mapSizeX - 3 && u.tileY > map.mapSizeY - 3))
                    {
                        map.tiles[u.tileX, u.tileY].occupied = false;
                        map.tiles[u.tileX, u.tileY].blocked = false;
                        units.Remove(u);
                        Destroy(u.gameObject);
                    }
                    if (alarm && !u.inDetainRange)
                    {
                        if (u.currentState != Unit.state.Moving)
                        {
                            if (u.detectedPlayerUnit)
                            {
                                map.GeneratePathTo(0, 0, u);
                            }
                            else
                            {
                                map.GeneratePathTo(map.mapSizeX - 1, map.mapSizeY - 1, u);
                            }
                        }
                    }
                    else if (!u.inDetainRange)
                    {
                        //If the alarm could be raised in this update, make this variable true
                        if (u.detectedPlayerUnit && !alarm)
                        {
                            //If the unit has detected a player unit
                            hasDetectedaPlayerUnit = true;

                            //If the unit has detected a player unit
                            Unit nearestGuard = FindNearestGuard(u);
                            if (nearestGuard != null && u.currentState == Unit.state.Idle)
                            {
                                //and this civilian is close enough to raise the detection of the guard, raise it
                                if (Vector3.Distance(u.transform.position, nearestGuard.transform.position) < 2)
                                {
                                    nearestGuard.increaseDetectionTimer(8);
                                }
                                //and this civilian is too far away to warn the guard, move towards it
                                else
                                {
                                    map.GeneratePathToNextBestTile(nearestGuard.tileX, nearestGuard.tileY, u);
                                }
                            }
                        }
                        //If no enemies have been detected and the alarm is not on, tell the units to just mill around the room a bit
                        else if (u.currentState == Unit.state.Idle)
                        {
                            manageTimer(u);
                        }
                    }
                }
            }
        }
    }

    //Manages the random timing of units movement.
    void manageTimer(Unit u)
    {
        if(setTimer < timer)
        {
            timer = 0;
            //If the alarm is not on, make the civilians move in a more sedentary manner.
            if (!alarm) {
                setTimer = rnd.Next(4, 8);
            }
            else
            {
                setTimer = rnd.Next(1, 2);
            }
            moveToRandomTileInRoom(u);
        }
        else
        {
            timer = timer + Time.deltaTime;
        }
    }

    //Tells the unit to move to a random tile in the room they are in
    void moveToRandomTileInRoom(Unit u)
    {
        Tile newTile = map.tiles[u.tileX, u.tileY].room.randomTileInRoom();
        map.GeneratePathTo(newTile.tileX, newTile.tileY, u);
    }

    //Sets all civilian handcuff icons to seeable and all detection icons to invisible
    public void triggerAlarm()
    {
        alarm = true;
        DetectionIndicator[] detectionIndicators = GetComponentsInChildren<DetectionIndicator>();
        foreach (DetectionIndicator d in detectionIndicators)
        {
            d.spriteRenderer.color = Color.clear;
        }
    }

    //Returns the nearest guard to a unit.
    Unit FindNearestGuard(Unit unit)
    {
        Unit nearestUnit=null;
        foreach (Unit guard in map.enemyController.units.ToList())
        {
            if (nearestUnit == null)
            {
                nearestUnit = guard;
            }
            else if (!guard.detectedPlayerUnit || Vector3.Distance(unit.transform.position, guard.transform.position) < Vector3.Distance(unit.transform.position, nearestUnit.transform.position) && !guard.detectedPlayerUnit)
            {
                nearestUnit = guard;
            }
        }
        return nearestUnit;
    }

    public void togglePause()
    {
        if (map.paused)
        {
            //If game is getting unpaused, start up all unit animations again
            foreach (Unit u in units)
            {
                u.togglePause();
            }
        }
        else
        {
            //If game is getting paused, stop all unit animations
            foreach (Unit u in units)
            {
                u.togglePause();
            }
        }
    }
}
