using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    System.Random rnd;
    float setTimer = 5f;
    float timer=0;
    private Unit unit;
    private TileMap map;
    // Start is called before the first frame update
    void Start()
    {
        unit = gameObject.GetComponent<Unit>();
        map = unit.map;
        rnd = new System.Random();
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > setTimer) {
            timer = 0;
            Debug.Log("New AI Path created");
            map.GeneratePathTo(rnd.Next(0, map.mapSizeX),
                rnd.Next(0, map.mapSizeY),
                gameObject);
        }
        else
        {
            timer = timer + Time.deltaTime;
        }
    }
}
