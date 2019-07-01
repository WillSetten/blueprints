using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Camera viewingCamera;
    public Vector2 direction;
    float moveSpeed = 5f;
    bool up = false;
    bool down = false;
    bool left = false;
    bool right = false;
    // Start is called before the first frame update
    void Start()
    {
        viewingCamera = GetComponent<Camera>();
        direction = new Vector2(0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        checkInput();
        manageDirection();
        moveCam();
    }

    void moveCam()
    {
        transform.Translate(direction * Time.deltaTime * moveSpeed);
    }

    void checkInput()
    {
        if (Input.GetKeyDown("w"))
        {
            up = true;
        }
        if (Input.GetKeyDown("a"))
        {
            left = true;
        }
        if (Input.GetKeyDown("d"))
        {
            right = true;
        }
        if (Input.GetKeyDown("s"))
        {
            down = true;
        }
        if (Input.GetKeyUp("w"))
        {
            up = false;
        }
        if (Input.GetKeyUp("s"))
        {
            down = false;
        }
        if (Input.GetKeyUp("a"))
        {
            left = false;
        }
        if (Input.GetKeyUp("d"))
        {
            right = false;
        }
    }

    void manageDirection()
    {
        if (left)
        {
            direction = new Vector2(-1, direction.y);
        }
        else if (right)
        {
            direction = new Vector2(1, direction.y);
        }
        else
        {
            direction = new Vector2(0, direction.y);
        }
        if (up)
        {
            direction = new Vector2(direction.x, 1);
        }
        else if (down)
        {
            direction = new Vector2(direction.x, -1);
        }
        else
        {
            direction = new Vector2(direction.x, 0);
        }
    }
}
