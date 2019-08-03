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
    //If an enemy has detected a player unit, the enemy will take several seconds to raise the alarm
    public float alarmTimer = 0;

    public AlarmBar alarmBar;
    // Start is called before the first frame update
    void Start()
    {
        units.AddRange(gameObject.GetComponentsInChildren<Unit>());
        map = GetComponentInParent<TileMap>();
        setTimer = 2;
        alarmBar = map.viewingCamera.GetComponentInChildren<AlarmBar>();
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
            if (enemyHasDetectedaPlayerUnit && alarmTimer > 5)
            {
                foreach (Unit unit in units)
                {
                    unit.detectionTimer = 2;
                    unit.detectedPlayerUnit = true;
                    unit.detectionIndicator.animator.SetBool("HasDetectedUnit", true);
                }
                    alarm = true;
            }
            //if a unit has detected a player unit but the alarm has not been given enough time to go off, increase the alarm timer
            else if (enemyHasDetectedaPlayerUnit)
            {
                alarmTimer = alarmTimer + Time.deltaTime;
                alarmBar.updateFill(new Vector3(alarmTimer / 5, 1, 1));
            }
            //if the unit has not detected a player unit and the alarm is above zero, decrease the alarm timer
            else if (!enemyHasDetectedaPlayerUnit && alarmTimer > 0)
            {
                alarmTimer = alarmTimer - Time.deltaTime;
                alarmBar.updateFill(new Vector3(alarmTimer / 5, 1, 1));
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
