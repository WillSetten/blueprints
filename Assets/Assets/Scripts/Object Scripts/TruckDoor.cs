using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TruckDoor : MonoBehaviour
{
    Vector2 openForce;
    Vector2 closeForce;
    public Truck truck;
    // Start is called before the first frame update
    void Start()
    {
        openForce = GetComponent<ConstantForce2D>().force;
        closeForce = openForce * (Vector2.left + Vector2.down);
        toggleOpen(false);
    }

    //Toggle whether the door is open or closed
    public void toggleOpen(bool open)
    {
        if (open)
        {
            GetComponent<ConstantForce2D>().force = openForce;
        }
        else
        {
            GetComponent<ConstantForce2D>().force = closeForce;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        truck.playSound(truck.doorClose);
    }
}
