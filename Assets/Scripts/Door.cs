using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    Vector2 storedVelocity;
    public void togglePause(bool paused)
    {
        if (paused) {
            storedVelocity = GetComponent<Rigidbody2D>().velocity;
            GetComponent<Rigidbody2D>().velocity = new Vector2(0,0);
            GetComponent<ConstantForce2D>().enabled = false;
        }
        else
        {
            GetComponent<Rigidbody2D>().velocity = storedVelocity;
            GetComponent<ConstantForce2D>().enabled = true;
        }
    }
}
