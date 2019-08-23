using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drill : MonoBehaviour
{
    public GameObject drill;
    public GameObject drillArea;
    public bool drillWorking;
    TileMap map;
    public Vault vault;

    // Start is called before the first frame update
    void Start()
    {
        drill.GetComponent<SpriteRenderer>().color = Color.clear;
        map = GetComponentInParent<TileMap>();
    }

    //Returns true if there are units present in the drill area
   List<Unit> unitsInDrillArea()
    {
        List<Unit> units = new List<Unit>();
        if (Physics2D.OverlapCircleAll(drillArea.transform.position, 0.1f, LayerMask.GetMask("PlayerUnits")).Length != 0)
        {
            units.Add(Physics2D.OverlapCircleAll(drillArea.transform.position, 0.1f, LayerMask.GetMask("PlayerUnits"))[0].GetComponent<Unit>());
        }
        return units;
    }

    //Begins the drill timer
    public void beginDrilling()
    {
        drill.GetComponent<SpriteRenderer>().color = Color.white;
        drill.GetComponent<Rigidbody2D>().simulated = true;
        drillArea.GetComponent<SpriteRenderer>().color = Color.clear;
        
    }

    //Pauses the drill timer due to an interruption
    public void pauseDrilling()
    {

    }

    //Ends the drilling and opens the vault
    void endDrilling()
    {
        drill.GetComponent<Rigidbody2D>().simulated = false;
        drill.GetComponent<SpriteRenderer>().color = Color.clear;
    }
}
