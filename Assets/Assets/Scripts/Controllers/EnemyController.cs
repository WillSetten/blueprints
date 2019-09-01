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
    public GameObject enemyPrefab;

    public int waveStage = 0;
    // Start is called before the first frame update
    void Start()
    {
        units.AddRange(gameObject.GetComponentsInChildren<Unit>());
        map = GetComponentInParent<TileMap>();
        setTimer = 2;
    }

    // Update is called once per frame
    void Update()
    {
        if (!map.paused && active)
        {
            bool hasDetectedaPlayerUnit=false;
            foreach (Unit unit in units)
            {
                //Move units if the alarm is on or if this specific unit has detected a player unit
                if (alarm||unit.detectedPlayerUnit)
                {
                    timer = 0;
                    foreach (GameObject g in map.units)
                    {
                        Unit playerUnit = g.GetComponent<Unit>();
                        //If the unit has detected a player unit, get in line of sight of the player unit and attack
                        if (Vector2.Distance(new Vector2(unit.tileX,unit.tileY), new Vector2(playerUnit.tileX,playerUnit.tileY))>unit.interactionRadius && !unit.hasBulletLOS(g) && 
                            playerUnit.isDetected && unit.currentState==Unit.state.Idle) {

                            map.GeneratePathTo(playerUnit.tileX, playerUnit.tileY, unit);
                            //Debug.Log("Enemy unit " + gameObject.name + " is moving towards player unit " + g.name);
                            break;
                        }
                    }
                }
                //If the alarm could be raised in this update, make this variable true
                if (unit.detectedPlayerUnit)
                {
                    hasDetectedaPlayerUnit = true;
                }
            }
            //If the alarm has gone off, the police will begin to close in on the bank. Slowly increment the alarm timer once again
            if (waveStage>0)
            {
                //If the timer is still below the desired level, increase it
                if (alarmTimer<40) {
                    //If the player has taken multiple hostages, slow down the timer based on how many hostages they have
                    if (map.UIhandler.hostageCount>1 && map.UIhandler.hostageCount<=5) {
                        alarmTimer = alarmTimer + Time.deltaTime / map.UIhandler.hostageCount;
                    }
                    else if (map.UIhandler.hostageCount>5)
                    {
                        alarmTimer = alarmTimer + Time.deltaTime / 5;
                    }
                    else
                    {
                        alarmTimer = alarmTimer + Time.deltaTime;
                    }
                    alarmBar.updateFill(alarmTimer/40);
                }
                else
                {
                    //Spawn enemies as per the stage at which the wave is
                    nextStage();
                }
            }
            //ALARM HANDLING
            //if a unit has detected a player unit and the alarm has been given enough time to go off, set the alarm off
            else if (hasDetectedaPlayerUnit && alarmTimer > 5)
            {
                foreach (Unit unit in units)
                {
                    unit.detectionTimer = 2;
                    unit.detectedPlayerUnit = true;
                    unit.detectionIndicator.animator.SetBool("HasDetectedUnit", true);
                }
                foreach (GameObject g in map.units) {
                    g.GetComponent<Unit>().isDetected = true;
                }
                triggerAlarm();
            }
            //if a unit has detected a player unit but the alarm has not been given enough time to go off, increase the alarm timer
            else if (hasDetectedaPlayerUnit)
            {
                alarmTimer = alarmTimer + Time.deltaTime;
                alarmBar.updateFill(alarmTimer / 5);
            }
            //if the unit has not detected a player unit and the alarm is above zero, decrease the alarm timer
            else if (!hasDetectedaPlayerUnit && alarmTimer > 0)
            {
                alarmTimer = alarmTimer - Time.deltaTime;
                alarmBar.updateFill(alarmTimer / 5);
            }
        }
    }

    public void triggerAlarm()
    {
        alarm = true;
        DetectionIndicator[] detectionIndicators = GetComponentsInChildren<DetectionIndicator>();
        foreach (DetectionIndicator d in detectionIndicators)
        {
            d.spriteRenderer.color = Color.clear;
        }
        map.civilianController.triggerAlarm();
        nextStage();
    }

    public void nextStage()
    {
        alarmBar.nextStage(waveStage);
        for (int i = 0; i < waveStage * 5; i++)
        {
            GameObject newEnemy = Instantiate(enemyPrefab, new Vector3(Random.Range(0,map.mapSizeX),0,0), new Quaternion(0,0,0,0));
            units.Add(newEnemy.GetComponent<Unit>());
            newEnemy.GetComponent<Unit>().hp = newEnemy.GetComponent<Unit>().hp + waveStage;
        }
        waveStage = waveStage + 1;
        alarmTimer = 0;
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
