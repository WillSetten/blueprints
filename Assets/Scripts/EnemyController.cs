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
    // Start is called before the first frame update
    void Start()
    {
        units.AddRange(gameObject.GetComponentsInChildren<Unit>());
        map = GameObject.Find("Map").GetComponent<TileMap>();
        rnd = new System.Random();
        setTimer = rnd.Next(2,10);
        active = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!map.paused && active)
        {
            foreach (Unit unit in units)
            {
                if (timer > setTimer)
                {
                    timer = 0;
                    setTimer = rnd.Next(2, 10);
                    //Debug.Log("New AI Path created");
                    map.GeneratePathTo(rnd.Next(0, map.mapSizeX),
                        rnd.Next(0, map.mapSizeY),
                        unit);
                }
                else
                {
                    timer = timer + Time.deltaTime;
                }
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
                u.GetComponent<Animator>().enabled = true;
            }
        }
        else
        {
            //If game is getting paused, stop all unit animations
            foreach (Unit u in units)
            {
                u.GetComponent<Animator>().enabled = false;
            }
        }
    }
}
