using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public enum state { Idle, Moving, Attacking, Looting };
    public state currentState;
    public string name;
    public bool selectable = false;
    public bool selected = false;
    public bool combatant = false;
    public int tileX;
    public int tileY;
    public int previousTileX;
    public int previousTileY;
    public int directionX;
    public int directionY;
    public float moveRate;
    public float lootMoveRate;
    public TileMap map;
    public int hp;
    public int interactionRadius = 2;
    public float attackCooldownCap;
    Animator animator;
    SpriteRenderer spriteRenderer;
    Rigidbody2D rigidbody2D;
    private float attackCooldown;
    public HealthBar healthBar;
    public List<Node> currentPath = null;
    public GameObject bulletType;
    public float lootRate;
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
        map.tiles[tileX,tileY].occupied = true;
        if (combatant)
        {
            hp = 5;
        }
        currentState = state.Idle;
        attackCooldown = attackCooldownCap;
        healthBar = GetComponentInChildren<HealthBar>();
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
        if (!map.paused)
        {
            if (currentPath != null)
            {
                int currNode = 0;
                while (currNode < currentPath.Count - 1)
                {
                    Vector3 start = map.TileCoordToWorldCoord(currentPath[currNode].x, currentPath[currNode].y);
                    Vector3 end = map.TileCoordToWorldCoord(currentPath[currNode + 1].x, currentPath[currNode + 1].y);
                    Debug.DrawLine(start, end);
                    currNode = currNode + 1;
                }
                MoveNextTile();
            }
            //Detect any nearby units and perform the appropriate action
            detectNearbyUnits();
            //If health is zero (or somehow below)
            if (hp <= 0)
            {
                die();
            }
            if (currentState == state.Looting)
            {
            }
        }
    }

    //Method which gets rid of the unit from the world
    void die()
    {
        Debug.Log(name + " has died!");
        if (currentPath == null)
        {
            map.tiles[tileX, tileY].occupied = false;
        }
        else
        {
            map.tiles[currentPath[currentPath.Count-1].x, currentPath[currentPath.Count - 1].y].occupied = false;
        }
        if (selectable&&selected)
        {
            map.units.Remove(gameObject);
            map.deselectUnit(gameObject);
        }
        Destroy(gameObject);
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
        {
            return;
        }
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
                currentState = state.Idle;
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
                if (directionX<0 && directionX<-1 || directionX>0 && directionX > 1 || directionY < 0 && directionY < -1 || directionY > 0 && directionY > 1)
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

    //True if the unit has moved past the desired point
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

    //Detects any nearby units
    void detectNearbyUnits()
    {
        Collider2D[] nearbyUnits = Physics2D.OverlapCircleAll((Vector2)transform.position, interactionRadius, LayerMask.GetMask("EnemyUnits", "PlayerUnits"));
        Unit u = null;
        //If there are no nearby units, return
        if (nearbyUnits.Length == 0)
        {
            return;
        }
        foreach (Collider2D c in nearbyUnits)
        {
            u = c.gameObject.GetComponent<Unit>();
            //If the unit has detected itself, skip
            if (Object.Equals(u, this))
            {
                continue;
            }
            //If the unit in range is a combatant and is on the other side, attempt to attack if this unit is also idle
            if (u.combatant && ((selectable && !u.selectable) || (!selectable && u.selectable)))
            {
                if (hasLOS(u))
                {
                    //Debug.Log(name + " can attack " + u.name);
                    Debug.DrawRay(transform.position, u.transform.position - transform.position, Color.white, interactionRadius);
                    currentState = state.Attacking;
                }
                //if (selectable)
                //Debug.Log("Unit " + name + " has enemy combatant " + u.name + " in range");
            }
            //If unit is a civilian, attempt to pacify if this unit is also idle
            else if (!u.combatant && currentState == state.Idle)
            {
                //Debug.Log("Unit " + name + "has civilian " + u.name + " in range");
            }
        }
        if (currentState == state.Attacking)
        {
            //If there are no nearby units, set the current state to not be attacking.
            if (nearbyUnits.Length == 0)
            {
                if (rigidbody2D.velocity==Vector2.zero)
                {
                    currentState = state.Idle;
                }
                else
                {
                    currentState = state.Moving;
                }
                return;
            }
            //If there are nearby units, check if this unit can attack
            else
            {
                //If attackcooldown is still below the units cap, increase it
                if (attackCooldown < attackCooldownCap)
                {
                    attackCooldown += Time.deltaTime;
                }
                //Attack if the cooldown is above the cap, shoot a bullet at the enemy with the least health and reset the cooldown
                else
                {
                    Collider2D closestUnit = nearestUnitFromOtherTeam(nearbyUnits);
                    if (closestUnit == null)
                    {
                        currentState = state.Idle;
                    }
                    else
                    {
                        shootBullet(closestUnit);
                    }
                }
            }
        }
        else
        {
            return;
        }
    }

    void shootBullet(Collider2D closestUnit)
    {
        Vector2 bulletDirection = (closestUnit.transform.position - transform.position).normalized;
        var angle = Mathf.Atan2(bulletDirection.y, bulletDirection.x) * Mathf.Rad2Deg;
        Quaternion bulletrotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
        attackCooldown = 0;
        GameObject bulletClone = Instantiate(bulletType, (Vector2)transform.position, bulletrotation, transform);
        bulletClone.GetComponent<Rigidbody2D>().velocity = bulletDirection * 5;
        Debug.Log(name + " is firing a bullet in direction " + bulletDirection.x + "," + bulletDirection.y +
            " from tile " + transform.position.x + "," + transform.position.y + " to tile " +
            closestUnit.transform.position.x + "," + closestUnit.transform.position.y +
            ". Bullet has direction ");
    }

    Collider2D nearestUnitFromOtherTeam(Collider2D[] nearbyUnits)
    {
        Collider2D nearestUnit=null;
        Unit u;
        foreach (Collider2D c in nearbyUnits)
        {
            //If this unit is the first unit in the array, continue
            if(c.gameObject == gameObject)
            {
                continue;
            }
            u = c.GetComponent<Unit>();
            if (nearestUnit == null)
            {
                //If there is no current nearest unit and we have a line of sight to that unit, set it as the nearest enemy unit
                if (hasLOS(c.GetComponent<Unit>())&& ((selectable && !u.selectable) || (!selectable && u.selectable)))
                {
                    nearestUnit = c;
                }
                //If there is no assigned unit and we do not have a line of sight to that unit or the unit is not an enemy unit, skip
                else
                {
                    continue;
                }
            }
            else if (Vector2.Distance(transform.position, c.transform.position) < Vector2.Distance(transform.position, nearestUnit.transform.position)
                && hasLOS(c.GetComponent<Unit>()) && ((selectable && !u.selectable) || (!selectable && u.selectable)))
            {
                nearestUnit = c;
            }
        }
        return nearestUnit;
    }

    private bool hasLOS(Unit u)
    {
        RaycastHit2D sightTest = Physics2D.Raycast(transform.position, u.transform.position - transform.position, interactionRadius, LayerMask.GetMask("Walls","Doors"));
        if (sightTest.collider == null)
        {
            //Debug.Log(name + " LOS hasn't collided with anything");
            return true;
        }
        if(sightTest.collider.CompareTag("Wall") || sightTest.collider.CompareTag("Door"))
        {
            //Debug.Log(name + " LOS has collided with object with tag " + sightTest.collider.tag);
            return false;
        }
        return true;
    }

    public void takeBulletDamage(Bullet bullet)
    {
        hp = hp - bullet.bulletDamage;
        healthBar.UpdateHealth();
    }
}