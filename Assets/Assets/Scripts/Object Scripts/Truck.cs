using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Truck : MonoBehaviour
{
    public List<GameObject> doors;
    bool open = false;

    private void OnMouseUp()
    {
        if (open)
        {
            open = false;
        }
        else
        {
            open = true;
        }
        foreach (GameObject d in doors)
        {
            d.GetComponent<TruckDoor>().toggleOpen(open);
        }
    }
}
