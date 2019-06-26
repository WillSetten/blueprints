using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyController : MonoBehaviour
{
    System.Random rnd;
    float setTimer;
    float timer=0;
    private Unit unit;
    private TileMap map;
    // Start is called before the first frame update
    void Start()
    {
        unit = gameObject.GetComponent<Unit>();
        map = GameObject.Find("Map").GetComponent<TileMap>();
        rnd = new System.Random();
        setTimer = rnd.Next(2,10);
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > setTimer) {
            timer = 0;
            setTimer = rnd.Next(2, 10);
            Debug.Log("New AI Path created");
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
