using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public enum state { Idle, Moving, Attacking, Looting};
    public state currentState;
    public bool detained = false;
    public bool selectable = false;
    public bool selected = false;
    public bool combatant = false;
    public int tileX;
    public int tileY;
    public int previousTileX;
    public int previousTileY;
    public int directionX;
    public int directionY;
    public float moveRate; //Speed at which the unit moves
    public float lootMoveRate; //Speed at which the unit moves whilst carrying loot
    public TileMap map;
    public int hp;
    public int interactionRadius = 2;
    public float attackCooldownCap;
    public bool hasLoot; //True if the unit is carrying loot
    public Animator animator;
    SpriteRenderer spriteRenderer;
    Rigidbody2D rigidbody2D;
    private float attackCooldown; //The time inbetween the units attacks
    public HealthBar healthBar;
    public List<Node> currentPath = null;
    public GameObject bulletType;
    public float lootRate; //The rate at which the unit can bag up loot
    public bool detectedPlayerUnit = false;
    public float detectionTimer = 0;
    public float detectionTimerMax;
    public float detainTimer = 0;
    public float detainTimerMax;
    public DetectionIndicator detectionIndicator;
    public bool isDetected = false;
    AudioSource audioSource;
    public AudioClip bulletSound;
    public bool isLarge; //If the unit is large and blocks the tile it is on
    public bool isDead=false;
    public bool inDetainRange=false;
    public bool inFreeRange=false;
    //Initialization
    private void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        tileX = (int)transform.position.x;
        tileY = (int)transform.position.y;
        map = GameObject.Find("Map").GetComponent<TileMap>();
        Debug.Log(transform.gameObject.name);
        name = transform.gameObject.name;
        currentState = state.Idle;
        attackCooldown = attackCooldownCap;
        healthBar = GetComponentInChildren<HealthBar>();
        hasLoot = false;
        if (combatant&&!selectable)
        {
            detectionIndicator = GetComponentInChildren<DetectionIndicator>();
        }
        audioSource = GetComponent<AudioSource>();
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
        else if (!selectable&&!combatant)
        {
            //If the unit is not already detained and is in range of a selected unit,
            if (!detained && inDetainRange && detainTimer>detainTimerMax)
            {
                map.detainUnit(this);
                playSound(map.handCuffSound);
            }
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
        if (!map.paused && !isDead)
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
            if (currentState == state.Idle)
            {
                directionX = 0;
                directionY = 0;
                animator.SetFloat("Move X", directionX);
                animator.SetFloat("Move Y", directionY);
            }
            manageDetainTimer();
            //Detect any nearby units and perform the appropriate action
            detectNearbyUnits();
            //If health is zero (or somehow below)
            if (hp <= 0)
            {
                die();
            }
        }
    }

    //Method which gets rid of the unit from the world
    public void die()
    {
        Debug.Log(name + " has died!");
        rigidbody2D.velocity = Vector2.zero;
        map.removeUnit(gameObject);
        Destroy(GetComponent<CircleCollider2D>());
        animator.SetBool("Dead", true);
        isDead = true;
        if (detectionIndicator!=null) {
            detectionIndicator.spriteRenderer.color = Color.clear;
        }
        spriteRenderer.sortingLayerName = "UnderUnits";
        transform.position = new Vector3(transform.position.x, transform.position.y, 0.5f);
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
            if (hasLoot) {
                rigidbody2D.velocity = new Vector2(directionX * lootMoveRate, directionY * lootMoveRate);
            }
            else
            {
                rigidbody2D.velocity = new Vector2(directionX * moveRate, directionY * moveRate);
            }
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
        if (map.tiles[currentPath[0].x, currentPath[0].y].blocked || map.tiles[currentPath[0].x, currentPath[0].y].occupied)
        {
            map.GeneratePathTo(currentPath[currentPath.Count - 1].x, currentPath[currentPath.Count - 1].y, this);
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
                map.tiles[tileX, tileY].isDestination = false;
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
                    Debug.Log("Unit position is being reset due to over-moving");
                    transform.position = map.TileCoordToWorldCoord(previousTileX, previousTileY);
                    map.GeneratePathTo(currentPath[currentPath.Count-1].x, currentPath[currentPath.Count - 1].y, this);
                }
                else if (oldDirectionX != directionX || oldDirectionY != directionY)
                {
                    //setRotation();
                    animator.SetFloat("Move X", directionX);
                    animator.SetFloat("Move Y", directionY);
                    if (hasLoot)
                    {
                        rigidbody2D.velocity = new Vector2(directionX, directionY).normalized * lootMoveRate;
                    }
                    else
                    {
                        rigidbody2D.velocity = new Vector2(directionX, directionY).normalized * moveRate;
                    }
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
        //Close units are the units which fall within the range of the circle around the units
        Collider2D[] closeUnits = Physics2D.OverlapCircleAll((Vector2)transform.position, interactionRadius, LayerMask.GetMask("EnemyUnits", "PlayerUnits", "CivilianUnits"));
        //Seen units are the units which the unit has a line of sight to and is in the desired range
        List<Collider2D> seenUnits = new List<Collider2D>();
        //This foreach loop trims the close units into units that the unit can actually see
        foreach (Collider2D c in closeUnits)
        {
            //If there is a line of sight
            if (hasLOS(c.gameObject))
            {
                //If this unit is a combatant and the seen unit is not on the same team or is a civilian, add to seen units
                if (combatant && (!c.GetComponent<Unit>().combatant || (!c.GetComponent<Unit>().selectable && selectable) || (c.GetComponent<Unit>().selectable && !selectable)))
                {
                    seenUnits.Add(c);
                }
                //If this unit is a civilian, add all close units with a line of sight to seen units
                else if (!combatant)
                {
                    seenUnits.Add(c);
                }

            }
        }
        Unit u = null;
        //If this unit is a combatant and is in the attacking state
        if (combatant && currentState == state.Attacking)
        {
            //If there are no nearby units, set the current state to not be attacking.
            if (seenUnits.Count == 0)
            {
                animator.SetBool("Attacking", false);
                if (rigidbody2D.velocity == Vector2.zero)
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
                    Collider2D closestUnit = nearestUnitFromOtherTeam(seenUnits);
                    if (closestUnit == null)
                    {
                        currentState = state.Idle;
                        animator.SetBool("Attacking", false);
                    }
                    else
                    {
                        shootBullet(closestUnit);
                    }
                }
            }
        }

        //If there are no nearby units and there are no civilians who have seen player units, return
        if (!selectable && seenUnits.Count == 0)
        {
            //If this unit is an AI unit and has no nearby units, update the timer
            if (!selectable && !map.civilianController.hasDetectedaPlayerUnit && detectionTimer > 0)
            {
                detectionTimer = detectionTimer - Time.deltaTime;
                detectionIndicator.animator.SetFloat("DetectionLevel", detectionTimer / detectionTimerMax);
            }
            //If this unit is a hostage and there are no nearby enemy units, set in free range to be false
            if (!combatant && selectable)
            {
                inFreeRange = false;
            }
            //If this unit is a civilian and there are no nearby player units, set in free range to be false
            if(!combatant && !selectable)
            {
                inDetainRange = false;
            }
            return;
        }

        bool stillInFreeRange = false;
        bool stillInDetainRange = false;
        //For each unit in range of this unit
        foreach (Collider2D c in seenUnits)
        {
            u = c.gameObject.GetComponent<Unit>();
            //If the unit in range is a combatant, attempt to attack if this unit is also idle
            if (u.combatant && (selectable&&!u.selectable||!selectable&&u.selectable) && currentState!=state.Attacking)
            {
                    //CODE FOR COMBATANT PLAYER UNITS
                    if (combatant && selectable)
                    {
                        //Debug.Log(name + " can attack " + u.name);
                        Debug.DrawRay(transform.position, u.transform.position - transform.position, Color.white, interactionRadius);
                        if (currentState!=state.Moving && combatant) {
                            currentState = state.Attacking;
                            animator.SetBool("Attacking", true);
                        }
                    }
                    //CODE FOR COMBATANT AI UNITS
                    else
                    {
                        //If the alarm has been triggered or this unit has detected a player, attack the nearby unit
                        if(map.enemyController.alarm || detectedPlayerUnit)
                        {
                            //Debug.Log(name + " can attack " + u.name);
                            Debug.DrawRay(transform.position, u.transform.position - transform.position, Color.white, interactionRadius);
                            if (currentState != state.Moving)
                            {
                                currentState = state.Attacking;
                                animator.SetBool("Attacking", true);
                            }
                        }
                        //Increase the detection timer
                        else
                        {
                            //Unit takes time to react to seeing a player unit
                            if (detectionTimer<detectionTimerMax)
                            {
                                if (combatant)
                                {
                                   increaseDetectionTimer(Vector2.Distance(transform.position, c.transform.position));
                                }
                                else
                                {
                                    increaseDetectionTimer(2*Vector2.Distance(transform.position, c.transform.position));
                                }
                            }
                            //When this time has expired, the unit will be detected
                            else
                            {
                                Debug.Log(name + " has detected " + u.name);
                                detectedPlayerUnit = true;
                                detectionIndicator.animator.SetBool("HasDetectedUnit", true);
                                u.isDetected = true;
                            }
                        }
                    }
            }
            //If this unit is a civilian
            if (!combatant && u.combatant)
            {
                //If the nearby unit is an enemy unit that is in range
                if (!u.selectable && detained && Vector3.Distance(u.transform.position, transform.position) <= u.interactionRadius / 2)
                {
                    stillInFreeRange = true;
                }
                //If the nearby unit is a player unit that is in range
                if (u.selectable && Vector3.Distance(u.transform.position, transform.position) <= u.interactionRadius / 2)
                {
                    stillInDetainRange = true;
                }
            }
        }

        inDetainRange = stillInDetainRange;
        inFreeRange = stillInFreeRange;
    }

    public void increaseDetectionTimer(float rate)
    {
        if (detectionTimer<detectionTimerMax) {
            detectionTimer = detectionTimer + Time.deltaTime * 3 / rate;
            detectionIndicator.animator.SetFloat("DetectionLevel", detectionTimer / 2);
        }
        else
        {
            detectedPlayerUnit = true;
        }
    }

    void shootBullet(Collider2D closestUnit)
    {
        Vector2 bulletDirection = (closestUnit.transform.position - transform.position).normalized;
        var angle = Mathf.Atan2(bulletDirection.y, bulletDirection.x) * Mathf.Rad2Deg;

        Quaternion bulletrotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
        attackCooldown = 0;

        GameObject bulletClone = Instantiate(bulletType, (Vector2)transform.position, bulletrotation, transform);

        bulletClone.GetComponent<Rigidbody2D>().velocity = bulletDirection * 10;
        /*Debug.Log(name + " is firing a bullet in direction " + bulletDirection.x + "," + bulletDirection.y +
            " from tile " + transform.position.x + "," + transform.position.y + " to tile " +
            closestUnit.transform.position.x + "," + closestUnit.transform.position.y +
            ". Bullet has direction ");*/

        animator.SetFloat("Move X", bulletDirection.x);
        animator.SetFloat("Move Y", bulletDirection.y);
        StartCoroutine(map.viewingCamera.GetComponent<CameraMovement>().Shake(.02f,.04f));
        //Will move this playSound into each units animations as an event. Only have attack animations for the police officer atm.
        //REMEMBER TO CHANGE
        if (selectable) {
            playSound(bulletSound);
        }
        else
        {
            animator.SetBool("Shoot", true);
        }
    }

    Collider2D nearestUnitFromOtherTeam(List<Collider2D> nearbyUnits)
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
                //If there is no current nearest unit and we have a bullet line of sight to that unit, set it as the nearest enemy unit
                if (hasBulletLOS(c.gameObject)&& ((selectable && !u.selectable) || (!selectable && u.selectable)))
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
                && hasLOS(c.gameObject) && u.combatant && ((selectable && !u.selectable) || (!selectable && u.selectable)))
            {
                nearestUnit = c;
            }
        }
        return nearestUnit;
    }

    public bool hasBulletLOS(GameObject u)
    {
        RaycastHit2D sightTest = Physics2D.Raycast(transform.position, u.transform.position - transform.position, 
            Vector2.Distance(transform.position, u.transform.position), LayerMask.GetMask("Walls","Doors", "CivilianUnits"));
        if (sightTest.collider == null)
        {
            //Debug.Log(name + " LOS hasn't collided with anything");
            return true;
        }
        if(sightTest.collider.CompareTag("Wall") || sightTest.collider.CompareTag("Door") || sightTest.collider.CompareTag("Civilian"))
        {
            //Debug.Log(name + " LOS has collided with object with tag " + sightTest.collider.tag);
            return false;
        }
        return false;
    }

    private bool hasLOS(GameObject u)
    {
        RaycastHit2D sightTest = Physics2D.Raycast(transform.position, u.transform.position - transform.position,
            Vector2.Distance(transform.position, u.transform.position), LayerMask.GetMask("Walls", "Doors"));
        if (sightTest.collider == null)
        {
            //Debug.Log(name + " LOS hasn't collided with anything");
            return true;
        }
        if (sightTest.collider.CompareTag("Wall") || sightTest.collider.CompareTag("Door"))
        {
            //Debug.Log(name + " LOS has collided with object with tag " + sightTest.collider.tag);
            return false;
        }
        return true;
    }

    public void takeBulletDamage(Bullet bullet)
    {
        if (combatant) {
            hp = hp - bullet.bulletDamage;
            healthBar.UpdateHealth();
        }
    }

    public void playSound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    public void shootSound()
    {
        audioSource.PlayOneShot(bulletSound);
    }

    public void endShot()
    {
        animator.SetBool("Shoot",false);
    }

    void manageDetainTimer()
    {
        if (!combatant)
        {
            //If this unit is in detain range and is not already detained
            if (!inFreeRange && inDetainRange && !detained)
            {
                //If the detain timer has reached its max value, show the handcuff icon and dont show the other icons
                if (detainTimer > detainTimerMax)
                {
                    GetComponentInChildren<HandCuffIcon>().spriteRenderer.color = Color.white;
                    GetComponentInChildren<DetectionIndicator>().spriteRenderer.color = Color.clear;
                    GetComponentInChildren<DetainBar>().spriteRenderer.color = Color.clear;
                    if (currentPath != null)
                    {
                        Node node = currentPath[0];
                        currentPath.Clear();
                        currentPath.Add(node);
                    }
                }
                //If the detain timer is under its max value, increase it
                else
                {
                    GetComponentInChildren<HandCuffIcon>().spriteRenderer.color = Color.clear;
                    GetComponentInChildren<DetectionIndicator>().spriteRenderer.color = Color.white;
                    GetComponentInChildren<DetainBar>().spriteRenderer.color = Color.white;
                    detainTimer = detainTimer + Time.deltaTime;
                }
            }
            //If this unit can be freed and is detained
            else if (!inDetainRange && inFreeRange && detained)
            {
                if (detainTimer > 0)
                {
                    GetComponentInChildren<DetainBar>().spriteRenderer.color = Color.white;
                    detainTimer = detainTimer - Time.deltaTime;
                }
                //If the unit has been freed, add it back to the civilian controller list
                else
                {
                    GetComponentInChildren<HandCuffIcon>().spriteRenderer.color = Color.clear;
                    map.freeUnit(this);
                    map.civilianController.units.Add(this);
                }
            }
            //If there is a player unit in range of a hostage, the hostage cannot be freed
            else if (inDetainRange && detained)
            {
                detainTimer = detainTimerMax;
                GetComponentInChildren<DetainBar>().spriteRenderer.color = Color.clear;
            }
            //If this unit is not in detain or free range and is not detained, decrease the timer
            else if (!inFreeRange && !inDetainRange && !detained)
            {
                if (detainTimer > 0)
                {
                    GetComponentInChildren<HandCuffIcon>().spriteRenderer.color = Color.clear;
                    GetComponentInChildren<DetainBar>().spriteRenderer.color = Color.white;
                    detainTimer = detainTimer - Time.deltaTime;
                }
            }
        }
    }
}