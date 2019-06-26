using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Camera viewingCamera;
    public Vector2 direction;
    float moveSpeed = 5f;
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
            direction = new Vector2(direction.x,1);
        }
        if (Input.GetKeyDown("a"))
        {
            direction = new Vector2(-1, direction.y);
        }
        if (Input.GetKeyDown("d"))
        {
            direction = new Vector2(1, direction.y);
        }
        if (Input.GetKeyDown("s"))
        {
            direction = new Vector2(direction.x, -1);
        }
        if (Input.GetKeyUp("w")|| Input.GetKeyUp("s"))
        {
            direction = new Vector2(direction.x,0);
        }
        if (Input.GetKeyUp("a") || Input.GetKeyUp("d"))
        {
            direction = new Vector2(0, direction.y);
        }
    }
}
