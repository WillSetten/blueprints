using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    Vector2 storedVelocity;
    //Method triggered from TileMap when space is pressed
    public void togglePause(bool paused)
    {
        //If the game is being paused, disable the force acting on the door, store the doors velocity and set it to 0
        if (paused) {
            storedVelocity = GetComponent<Rigidbody2D>().velocity;
            GetComponent<Rigidbody2D>().velocity = new Vector2(0,0);
            GetComponent<ConstantForce2D>().enabled = false;
        }
        //If the game is being unpaused, re-enable the force acting on the door and set the doors' velocity to the value we stored
        else
        {
            GetComponent<Rigidbody2D>().velocity = storedVelocity;
            GetComponent<ConstantForce2D>().enabled = true;
        }
    }
}
