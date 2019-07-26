using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loot : MonoBehaviour
{
    public int tileX;
    public int tileY;
    public TileMap map;
    // Start is called before the first frame update
    void Start()
    {
        tileX = Mathf.RoundToInt(transform.position.x);
        tileY = Mathf.RoundToInt(transform.position.y);
    }

    private void OnMouseUp()
    {
        if (map.multipleUnitsSelected)
        {
            foreach (GameObject g in map.selectedUnits)
            {
                if (isUnitAdjacent(g.GetComponent<Unit>()))
                {
                    Debug.Log("Unit " + g.name + " is adjacent to " + gameObject.name);
                    //Set state of unit to looting
                    return;
                }
            }
        }
        else
        {
            if (isUnitAdjacent(map.selectedUnit.GetComponent<Unit>()))
            {
                Debug.Log("Unit " + map.selectedUnit.name + " is adjacent to " + gameObject.name);
                //Set state of unit to looting
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
