using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Camera viewingCamera;
    public Vector2 direction;
    public Resolution res;
    TileMap map;
    float moveSpeed = 5f;
    bool up = false;
    bool down = false;
    bool left = false;
    bool right = false;

    void Start()
    {
        viewingCamera = GetComponent<Camera>();
        viewingCamera.orthographicSize = (float)Screen.height / 64;
        res = Screen.currentResolution;
        Debug.Log(viewingCamera.orthographicSize);
        direction = new Vector2(0, 0);
        map = GameObject.Find("Map").GetComponent<TileMap>();
    }

    void Update()
    {
        //If the resolution of the screen has changed, change the camera size appropriately
        if (!Screen.currentResolution.Equals(res))
        {
            viewingCamera.orthographicSize = (float)Screen.height / 64;
            res = Screen.currentResolution;
        }
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
        //Zoom in
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && viewingCamera.orthographicSize > Screen.height / 128)
        {
            viewingCamera.orthographicSize -= 0.4f;
        }
        //Zoom out
        if (Input.GetAxis("Mouse ScrollWheel") < 0 && viewingCamera.orthographicSize < Screen.height / 64)
        {
            viewingCamera.orthographicSize += 0.4f;
        }
    }

    void manageDirection()
    {
        if (left && transform.position.x>0)
        {
            direction = new Vector2(-1, direction.y);
        }
        else if (right && transform.position.x < map.mapSizeY)
        {
            direction = new Vector2(1, direction.y);
        }
        else
        {
            direction = new Vector2(0, direction.y);
        }
        if (up && transform.position.y < map.mapSizeY)
        {
            direction = new Vector2(direction.x, 1);
        }
        else if (down && transform.position.y > 0)
        {
            direction = new Vector2(direction.x, -1);
        }
        else
        {
            direction = new Vector2(direction.x, 0);
        }
    }
}
