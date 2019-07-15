using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public string name;
    public bool selectable = true;
    public bool selected = false;
    public bool combatant;
    public int tileX;
    public int tileY;
    public int previousTileX;
    public int previousTileY;
    public int directionX;
    public int directionY;
    public float moveRate;
    public TileMap map;
    Animator animator;
    SpriteRenderer spriteRenderer;
    Rigidbody2D rigidbody2D;

    public List<Node> currentPath = null;

    //Initialization
    private void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        tileX = (int)transform.position.x;
        tileY = (int)transform.position.y;
        map = GameObject.Find("Map").GetComponent<TileMap>();
        transform.position = map.TileCoordToWorldCoord(tileX, tileY);
        Debug.Log(transform.gameObject.name);
        name = transform.gameObject.name;
        moveRate = 2.5f;
        map.tiles[tileX,tileY].occupied = true;
    }
   
    //Highlight the unit in green when the mouse hovers over it
    private void OnMouseOver()
    {
        if (selectable&&!selected)
        {
            turnOnPreSelectionHighlight();
        }
    }

    //Remove the highlight on the unit when the mouse stops hovering over it
    private void OnMouseExit()
    {
        if (selectable&&!selected)
        {
            turnOffPreSelectionHighlight();
        }
    }

    public void turnOnPreSelectionHighlight()
    {
        GetComponent<Renderer>().material.SetColor("_SpriteColor", Color.green);
    }

    public void turnOffPreSelectionHighlight()
    {
        GetComponent<Renderer>().material.SetColor("_SpriteColor", Color.white);
    }

    void OnMouseUp()
    {
        Debug.Log("Clicked on "+name);
        if (selectable)
        {
            map.setSelectedUnit(transform.gameObject);
        }
    }

    public void changeHighlight()
    {
        if (selected)
        {
            GetComponent<Renderer>().material.SetFloat("_OutlineThickness", 1);
            GetComponent<Renderer>().material.SetColor("_SpriteColor", Color.white);
        }
        else
        {
            GetComponent<Renderer>().material.SetFloat("_OutlineThickness", 0);
        }
    }

    void Update()
    {
        if (selectable)
        {
            //Debug.Log(name + ": Tile co-ordinates: "+ tileX + "," + tileY + " Actual Co-ordinates: " + transform.position.x +","+ transform.position.y);
        }
        if (currentPath !=null && !map.paused)
        {
            int currNode = 0;
            while(currNode < currentPath.Count-1)
            {
                Vector3 start = map.TileCoordToWorldCoord(currentPath[currNode].x, currentPath[currNode].y);
                Vector3 end = map.TileCoordToWorldCoord(currentPath[currNode + 1].x, currentPath[currNode + 1].y);
                Debug.DrawLine(start, end);
                currNode = currNode + 1;
            }
        MoveNextTile();
        }
    }

    public void togglePause()
    {
        if(map.paused)
        {
            animator.enabled = false;
            rigidbody2D.velocity = new Vector2(0, 0);
        }
        else
        {
            animator.enabled = true;
            rigidbody2D.velocity = new Vector2(directionX * moveRate, directionY * moveRate);
        }
    }

    public void setPath(List<Node> newPath) 
    {
        currentPath = newPath;
    }

    public void MoveNextTile()
    {
        //If there is no path to follow, return.
        if (currentPath == null)
            return;
        //If the unit is close enough to its next destination
        if (Vector2.Distance(transform.position, map.TileCoordToWorldCoord(currentPath[0].x, currentPath[0].y)) < 0.1f || checkIfOverMoved())
        {
            //If the unit has hit a node but has reached the end of its path
            if (currentPath.Count == 1)
            {
                transform.position = map.TileCoordToWorldCoord(currentPath[0].x, currentPath[0].y);
                rigidbody2D.velocity = new Vector2(0, 0);
                previousTileX = tileX;
                previousTileY = tileY;
                tileX = currentPath[0].x;
                tileY = currentPath[0].y;
                currentPath = null;
                directionX = 0;
                directionY = 0;
                animator.SetFloat("Move X", directionX);
                animator.SetFloat("Move Y", directionY);
            }
            //If the unit has hit a node but has more nodes to visit
            else
            {
                previousTileX = tileX;
                previousTileY = tileY;
                tileX = currentPath[0].x;
                tileY = currentPath[0].y;
                currentPath.RemoveAt(0);
                int oldDirectionX = directionX;
                int oldDirectionY = directionY;
                directionX = currentPath[0].x - tileX;
                directionY = currentPath[0].y - tileY;

                //If the direction that the unit is moving in is greater in magnitude than 1 in any direction, re-place the unit and reset it's path
                //This if statement is intended to solve a bug where unity randomly moves the unit great distances for a reason I've had trouble determining
                if(directionX<0 && directionX<-1 || directionX>0 && directionX > 1 || directionY < 0 && directionY < -1 || directionY > 0 && directionY > 1)
                {
                    transform.position = map.TileCoordToWorldCoord(previousTileX, previousTileY);
                    map.GeneratePathTo(currentPath[currentPath.Count-1].x, currentPath[currentPath.Count - 1].y, this);
                }
                else if (oldDirectionX != directionX || oldDirectionY != directionY)
                {
                    //setRotation();
                    animator.SetFloat("Move X", directionX);
                    animator.SetFloat("Move Y", directionY);
                    rigidbody2D.velocity = new Vector2(directionX*moveRate, directionY*moveRate);
                }
            }
        }
    }

    //Redundant method
    public void setRotation()
    {
        if (directionX == 1 && directionY == -1)
        {
            transform.rotation = Quaternion.Euler(0, 0, 45);
        }
        else if (directionX == 1 && directionY == 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 90);
        }
        else if (directionX == 1 && directionY == 1)
        {
            transform.rotation = Quaternion.Euler(0, 0, 135);
        }
        else if (directionX == 0 && directionY == 1)
        {
            transform.rotation = Quaternion.Euler(0, 0, 180);
        }
        else if (directionX == -1 && directionY == 1)
        {
            transform.rotation = Quaternion.Euler(0, 0, -135);
        }
        else if (directionX == -1 && directionY == 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, -90);
        }
        else if (directionX == -1 && directionY == -1)
        {
            transform.rotation = Quaternion.Euler(0, 0, -45);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    //True if 
    bool checkIfOverMoved()
    {
        Vector3 position = transform.position;
        //If moving right and has an X co-ordinate greater than that of the destination
        if (directionX > 0 && position.x>map.TileCoordToWorldCoord(currentPath[0].x, currentPath[0].y).x)
        {
            return true;
        }
        //If moving left and has an X co-ordinate less than that of the destination
        if (directionX < 0 && position.x < map.TileCoordToWorldCoord(currentPath[0].x, currentPath[0].y).x)
        {
            return true;
        }
        //If moving up and has an Y co-ordinate greater than that of the destination
        if (directionY > 0 && position.y > map.TileCoordToWorldCoord(currentPath[0].x, currentPath[0].y).y)
        {
            return true;
        }
        //If moving down and has an Y co-ordinate less than that of the destination
        if (directionY < 0 && position.y < map.TileCoordToWorldCoord(currentPath[0].x, currentPath[0].y).y)
        {
            return true;
        }
        return false;
    }
}
