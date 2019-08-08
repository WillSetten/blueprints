﻿using System.Collections;
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
    public bool hasDetectedaPlayerUnit;

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
            foreach (Unit u in units)
            {
                if (map.enemyController.alarm)
                {
                    //Not sure what to put here yet, ask Jay and Kyle
                }
                //If the alarm could be raised in this update, make this variable true
                else if (u.detectedPlayerUnit)
                {
                    //If the unit has detected a player unit
                    hasDetectedaPlayerUnit = true;

                    //If the unit has detected a player unit
                    Unit nearestGuard = FindNearestGuard(u);
                    if (nearestGuard!=null) {
                        //and this civilian is close enough to raise the detection of the guard, raise it
                        if (Vector3.Distance(u.transform.position, nearestGuard.transform.position) < 2)
                        {
                            nearestGuard.increaseDetectionTimer(8);
                        }
                        //and this civilian is too far away to warn the guard, move towards it
                        else
                        {
                            map.GeneratePathTo(nearestGuard.tileX, nearestGuard.tileY, u);
                        }
                    }
                }
                else if (setTimer < timer && u.currentState==Unit.state.Idle)
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

    Unit FindNearestGuard(Unit unit)
    {
        Unit nearestUnit=null;
        foreach (Unit guard in map.enemyController.units)
        {
            if (nearestUnit == null)
            {
                nearestUnit = guard;
            }
            else if (Vector3.Distance(unit.transform.position, guard.transform.position) < Vector3.Distance(unit.transform.position, nearestUnit.transform.position))
            {
                nearestUnit = guard;
            }
        }
        return nearestUnit;
    }
}
