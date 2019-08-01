using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyController : MonoBehaviour
{
    System.Random rnd;
    //How frequently the unit is given a new path
    float setTimer;
    float timer=0;
    public List<Unit> units;
    private TileMap map;
    //Enemy Controller can be toggled
    public bool active;
    //If the alarm variable is true, all units should attempt to neutralize the players units
    public bool alarm = false;
    //If an enemy has detected a player unit, the enemy will take 4 seconds to raise the alarm
    public float alarmTimer = 5;
    // Start is called before the first frame update
    void Start()
    {
        units.AddRange(gameObject.GetComponentsInChildren<Unit>());
        map = GetComponentInParent<TileMap>();
        rnd = new System.Random();
        setTimer = rnd.Next(2,10);
    }

    // Update is called once per frame
    void Update()
    {
        if (!map.paused && active)
        {
            bool enemyHasDetectedaPlayerUnit=false;
            foreach (Unit unit in units)
            {
                if (unit.detectedPlayerUnit)
                {
                    enemyHasDetectedaPlayerUnit = true;
                }
                if (timer > setTimer && (alarm||unit.detectedPlayerUnit))
                {
                    timer = 0;
                    setTimer = rnd.Next(2, 10);
                    foreach (GameObject g in map.units)
                    {
                        Unit playerUnit = g.GetComponent<Unit>();
                        //If the unit has detected a player unit or the alarm has been raised and this unit is idle, move towards this unit and attack
                        Debug.Log("Enemy unit is moving towards player unit");
                        if (Vector2.Distance(new Vector2(unit.tileX,unit.tileY), new Vector2(playerUnit.tileX,playerUnit.tileY))>1) {
                            map.GeneratePathTo(playerUnit.tileX, playerUnit.tileY, unit);
                        }
                        break;
                    }
                }
                else if(timer<setTimer)
                {
                    timer = timer + Time.deltaTime;
                }
            }
            //ALARM HANDLING
            //if a unit has detected a player unit and the alarm has been given enough time to go off, set the alarm off
            if (enemyHasDetectedaPlayerUnit && alarmTimer <= 0)
            {
                foreach (Unit unit in units)
                {
                    unit.detectionTimer = 2;
                    unit.detectedPlayerUnit = true;
                }
                    alarm = true;
            }
            //if a unit has detected a player unit but the alarm has not been given enough time to go off, decrease the amount of time left
            else if (enemyHasDetectedaPlayerUnit)
            {
                alarmTimer = alarmTimer - Time.deltaTime;
            }
            //if the unit has not detected a player unit and the alarm
            else if (!enemyHasDetectedaPlayerUnit && alarmTimer < 5)
            {
                alarmTimer = alarmTimer + Time.deltaTime;
            }
        }
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
