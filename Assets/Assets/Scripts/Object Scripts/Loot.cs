using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loot : MonoBehaviour
{
    public int tileX;
    public int tileY;
    public TileMap map;
    private Unit lootingUnit;
    public float totalTime;
    public float remainingTime;
    // Start is called before the first frame update
    void Start()
    {
        tileX = Mathf.RoundToInt(transform.position.x);
        tileY = Mathf.RoundToInt(transform.position.y);
        remainingTime = totalTime;
    }

    void Update()
    {
        //If there is a unit attached to this loot
        if (!map.paused&&lootingUnit!=null)
        {
            //And if that unit is actually in a looting state and is still next to the loot
            if (lootingUnit.currentState == Unit.state.Looting && isUnitAdjacent(lootingUnit))
            {
                //Show the loot progress bar once the unit has started looting it
                if (remainingTime==totalTime) {
                    GetComponentInChildren<LootBar>().showLootBar();
                }
                //If the loot progress timer has finished, the unit bags up the loot
                if (remainingTime < 0)
                {
                    Debug.Log(name + " has been bagged up! Lets do another round");
                    lootingUnit.hasLoot = true;
                    map.loot.Remove(this);
                    Destroy(gameObject);
                }
                //Reduce the remaining time before the loot is bagged up
                else
                {
                    remainingTime = remainingTime - Time.deltaTime*lootingUnit.lootRate;
                }
            }
        }
    }

    private void OnMouseUp()
    {
            foreach (GameObject g in map.selectedUnits)
            {
                if (isUnitAdjacent(g.GetComponent<Unit>()) && g.GetComponent<Unit>().combatant)
                {
                    //Debug.Log("Unit " + g.name + " is adjacent to " + gameObject.name);
                    //Set state of unit to looting
                    g.GetComponent<Unit>().currentState = Unit.state.Looting;
                    lootingUnit = g.GetComponent<Unit>();
                    return;
                }
            }
    }

    //Returns true if the given unit is
    bool isUnitAdjacent(Unit u)
    {
        if ((u.tileX == tileX && ((u.tileY - tileY==1)||(u.tileY - tileY == -1))) ||
            (u.tileY == tileY && ((u.tileX - tileX == 1) || (u.tileX - tileX == -1))))
        {
            return true;
        }
        return false;
    }
}
