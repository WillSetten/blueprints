using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int bulletDamage;
    //Checks if the collider is attached to a unit script, if so, reduce unit health by 1 and create a small explosion effect
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Bullet hit " + collision.gameObject.name);
        if (collision.gameObject.GetComponent<Unit>())
        {
            collision.gameObject.GetComponent<Unit>().takeBulletDamage(this);
        }
        Destroy(gameObject);
    }
}