using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class TileMap : MonoBehaviour
{
    //Each unit should have a click handler which, when selected, should make this variable equal to that unit.
    public GameObject selectedUnit; //Currently selected unit
    public Camera viewingCamera; //The camera used to observe the map
    public List<GameObject> selectedUnits; //When multiple units are selected, they are put into this list
    public List<GameObject> units; //List of selectable units
    public EnemyController enemyController;
    public CivilianController civilianController;
    public TileType[] tileTypes;
    public bool paused = false; //True when game is paused, false when it is not
    public bool multipleUnitsSelected = true; //True when multiple units are selected.
    public Tile[,] tiles; //2D Array of tiles
    public int mapSizeX;
    public int mapSizeY;
    public Tile lastSelectedTile; //Tile stored to make sure the user spamming tiles doesn't make the unit skip tiles
    public Shader blueprintShader;
    public List<Loot> loot; //List of Loot

    GameObject[] doors; //List of Doors
    Room[] rooms; //List of Rooms
    int[,] tileMatrix; //2D Integer array for showing which tiles are passable and which aren't
    Node[,] graph; //2D Array of Nodes for pathfinding
    Dictionary<string,string> pathCache; //Dictionary of paths. Since the pathfinding algorithm is quite processor intensive,
    Rect rect;
    //storing any calculated paths for later use makes the game run smoother
    private Vector2 srtBoxPos = Vector2.zero; //Where the unit selection box begins
    private Vector2 endBoxPos = Vector2.zero; //Where the unit selection box ends
    private Collider2D[] containedColliders; //The colliders inside the selection square
    private Unit[] containedUnits; //The units inside the selection square

    //Initialisation
    private void Start()
    {
        mapSizeX = 26;
        mapSizeY = 26;
        tiles = new Tile[mapSizeX, mapSizeY];
        lastSelectedTile = null;
        enemyController = GameObject.Find("Enemy Controller").GetComponent<EnemyController>();
        GenerateMapData();
        GeneratePathfindingGraph();
        pathCache = new Dictionary<string, string>();
        viewingCamera = GameObject.Find("Main Camera").GetComponent<Camera>();

        rooms = GetComponentsInChildren<Room>();
        foreach(Room r in rooms)
        {
            r.map = this;
        }

        loot = new List<Loot>(GetComponentsInChildren<Loot>());
        foreach (Loot l in loot)
        {
            l.map = this;
        }

        doors = GameObject.FindGameObjectsWithTag("Door");
        selectedUnits = new List<GameObject>();
        //setSelectedUnit(selectedUnit);
        setMultipleSelectedUnits(units);
        blueprintShader = Shader.Find("Custom/Edge Highlight");
        rect = new Rect();
        viewingCamera.transform.parent.transform.position = new Vector3(units[0].transform.position.x, units[0].transform.position.y, -10);
    }

    private void Update()
    {
        // Called while the user is holding the mouse down.
        if (Input.GetKey(KeyCode.Mouse0))
        {
            // Called on the first update where the user has pressed the mouse button.
            if (Input.GetKeyDown(KeyCode.Mouse0))
                srtBoxPos = Input.mousePosition;
            else  // Else we must be in "drag" mode.
                endBoxPos = Input.mousePosition;
        }
        else
        {
            // Handle the case where the player had been drawing a box but has now released.
            if (endBoxPos != Vector2.zero && srtBoxPos != Vector2.zero)
            {
                handleUnitSelection();
                //Debug.Log("Start: " + srtBoxPos + ", End: " + endBoxPos);
            }
            // Reset box positions.
            endBoxPos = srtBoxPos = Vector2.zero;
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            Debug.Log("Space Pressed");
            if(paused)
            {
                //If game is getting unpaused, start up all unit animations again
                paused = false;
                viewingCamera.SetReplacementShader(blueprintShader, "blueprint");
                foreach (GameObject u in units)
                {
                    u.GetComponent<Unit>().togglePause();
                }
                foreach(GameObject door in doors)
                {
                    door.GetComponent<Door>().togglePause(paused);
                }
                enemyController.togglePause();
                civilianController.togglePause();
            }
            else
            {
                //If game is getting paused, stop all unit animations
                paused = true;
                viewingCamera.ResetReplacementShader();
                foreach (GameObject u in units)
                {
                    u.GetComponent<Unit>().togglePause();
                }
                foreach (GameObject door in doors)
                {
                    door.GetComponent<Door>().togglePause(paused);
                }
                enemyController.togglePause();
                civilianController.togglePause();
            }
        }
        if (Input.GetKeyUp(KeyCode.Alpha1)|| Input.GetKeyUp(KeyCode.Keypad1))
        {
            Debug.Log("1 pressed");
            setSelectedUnit(units[0]);
        }
        if (Input.GetKeyUp(KeyCode.Alpha2) || Input.GetKeyUp(KeyCode.Keypad2) && (units.Count>1))
        {
            Debug.Log("2 pressed");
            setSelectedUnit(units[1]);
        }
        if (Input.GetKeyUp(KeyCode.Alpha3) || Input.GetKeyUp(KeyCode.Keypad3) && (units.Count > 2))
        {
            Debug.Log("3 pressed");
            setSelectedUnit(units[2]);
        }
        if (Input.GetKeyUp(KeyCode.Alpha4) || Input.GetKeyUp(KeyCode.Keypad4) && (units.Count > 3))
        {
            Debug.Log("4 pressed");
            setSelectedUnit(units[3]);
        }
    }

    void OnGUI()
    {
        //**All box-drawing code will need to be rewritten for optimization. Research usage of a box collider rather than
        //iterating through all units**
        // If we are in the middle of a selection draw the texture.
        if (srtBoxPos != Vector2.zero && endBoxPos != Vector2.zero && Vector2.Distance(srtBoxPos,endBoxPos)>10)
        {
            // Create a rectangle object out of the start and end position while transforming it
            // to the screen's cordinates.
            if (srtBoxPos.x < endBoxPos.x)
            {
                rect.xMin = srtBoxPos.x;
                rect.xMax = endBoxPos.x;
            }
            else
            {
                rect.xMin = endBoxPos.x;
                rect.xMax = srtBoxPos.x;
            }
            if (srtBoxPos.y < endBoxPos.y)
            {
                rect.yMin = srtBoxPos.y;
                rect.yMax = endBoxPos.y;
            }
            else
            {
                rect.yMin = endBoxPos.y;
                rect.yMax = srtBoxPos.y;
            }
            foreach (GameObject u in units)
            {
                if (u == null)
                {
                    continue;
                }
                Vector2 unitScreenPosition = viewingCamera.WorldToScreenPoint(new Vector2(u.transform.position.x, u.transform.position.y));
                Debug.Log(u.name + " Screen position is " + unitScreenPosition);
                if (rect.Contains(unitScreenPosition))
                {
                    u.GetComponent<Unit>().turnOnPreSelectionHighlight();
                }
                else
                {
                    u.GetComponent<Unit>().turnOffPreSelectionHighlight();
                }
            }
            rect = new Rect(srtBoxPos.x, Screen.height - srtBoxPos.y,
                               endBoxPos.x - srtBoxPos.x,
                               -1 * (endBoxPos.y - srtBoxPos.y));
            // Draw the texture.
            GUI.Box(rect,"");
        }
    }

    //Decides how large the map is, which tiles (floor, walls) go where in the map
    void GenerateMapData()
    {
        //Allocate Map tiles
        tileMatrix = new int[mapSizeX, mapSizeY];
        tiles = new Tile[mapSizeX,mapSizeY];
        //Initialize map
        /*for (int x= 0; x< mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                tileMatrix[x, y] = 0;
            }
        }
        harvestAndTrustee();*/
        foreach (Tile t in GetComponentsInChildren<Tile>())
        {
            t.tileX = Mathf.RoundToInt(t.transform.position.x);
            t.tileY = Mathf.RoundToInt(t.transform.position.y);
            t.map = this;
            tiles[t.tileX, t.tileY] = t;
            if (t.impassable) {
                tileMatrix[t.tileX, t.tileY] = 1;
            }
            else
            {
                tileMatrix[t.tileX, t.tileY] = 0;
            }
        }
    }

    //Generates a series of nodes from the graph which define which tiles are connected to which tiles.
    public void GeneratePathfindingGraph()
    {
        // Initialize the array
        graph = new Node[mapSizeX, mapSizeY];

        // Initialize a Node for each spot in the array
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeX; y++)
            {
                graph[x, y] = new Node();
                graph[x, y].x = x;
                graph[x, y].y = y;
            }
        }

        // Calculate neighbours
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeX; y++)
            {
                if (UnitCanEnterTile(x, y))
                {
                    // Try left
                    if (x > 0)
                    {
                        if (y > 0 && tileMatrix[x - 1, y] != 1 && tileMatrix[x, y - 1] != 1)
                            graph[x, y].neighbours.Add(graph[x - 1, y - 1]);
                        if (y < mapSizeY - 1 && tileMatrix[x - 1, y] != 1 && tileMatrix[x, y + 1] != 1)
                            graph[x, y].neighbours.Add(graph[x - 1, y + 1]);
                        graph[x, y].neighbours.Add(graph[x - 1, y]);
                    }

                    // Try Right
                    if (x < mapSizeX - 1)
                    {
                        if (y > 0 && tileMatrix[x + 1, y] != 1 && tileMatrix[x, y - 1] != 1)
                            graph[x, y].neighbours.Add(graph[x + 1, y - 1]);
                        if (y < mapSizeY - 1 && tileMatrix[x + 1, y] != 1 && tileMatrix[x, y + 1] != 1)
                            graph[x, y].neighbours.Add(graph[x + 1, y + 1]);

                        graph[x, y].neighbours.Add(graph[x + 1, y]);
                    }

                    // Try straight up and down
                    if (y > 0)
                        graph[x, y].neighbours.Add(graph[x, y - 1]);
                    if (y < mapSizeY - 1)
                        graph[x, y].neighbours.Add(graph[x, y + 1]);
                }
            }
        }
    }

    //Converts Tile Co-ordinates with Co-ordinates in the actual world. For now, this is yust a matter of converting two ints into
    //a Vector3. Any scaling issues will probably have to be fixed in this method.
    public Vector3 TileCoordToWorldCoord(float x, float y)
    {
        return new Vector3((int)x, (int)y, 0);
    }

    //Takes in an x and y to move the selected unit to. This method currently uses basic Dyikstra
    public void GeneratePathTo(int x, int y, Unit unit)
    {
        //Debug.Log("Origin Tile: " + unit.tileX + "," + unit.tileY + " Goal Tile: " + x + "," + y);
        //If the units current destination is the same as the one thats been clicked on, return
        if (unit.currentPath != null) {
            Node currentDestination = unit.currentPath[unit.currentPath.Count - 1];
            if (currentDestination.x == x && currentDestination.y == y)
            {
                //Debug.Log(unit.name + " tried travelling to the same tile more than once!");
                return;
            }
            //If the path of the unit is changing, make sure that the previous destination tile is no longer listed as a destination
            else
            {
                tiles[currentDestination.x, currentDestination.y].isDestination = false;
                tiles[currentDestination.x, currentDestination.y].GetComponent<SpriteRenderer>().color = Color.white;
            }
        }

        //If the unit is attempting to travel to a tile that is impassable, return
        if (UnitCanEnterTile(x, y) == false)
        {
            //Debug.Log("Unit cannot enter that tile");
            return;
        }

        //If the unit is attempting to travel to the same tile again
        if (x == unit.tileX && y == unit.tileY)
        {
            //Debug.Log(unit.name + " tried travelling to the tile it was already on!");
            return;
        }

        //If the units destination tile is occupied or another unit is moving to it, recall this method with the closest tile to the target in the same room
        if (tiles[x, y].occupied || tiles[x,y].isDestination)
        {
            //Debug.Log("The tile that " + unit.name + " was trying to get to is occupied, attempting to find another path");
            Tile newDestinationTile = tiles[x, y].room.findBestNextTile(tiles[x, y], unit);
            if (newDestinationTile!=null) {
                GeneratePathTo(newDestinationTile.tileX, newDestinationTile.tileY, unit.GetComponent<Unit>());
            }
            return;
        }
        if (unit.selectable) {
            tiles[x, y].GetComponent<SpriteRenderer>().color = Color.blue;
        }
        else if (unit.combatant)
        {
            tiles[x, y].GetComponent<SpriteRenderer>().color = Color.red;
        }
        List<Node> currentPath = new List<Node>();
        Dictionary<string, Node> open = new Dictionary<string,Node>();
        Dictionary<string, Node> close = new Dictionary<string, Node>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();
        Node source = graph[unit.tileX, unit.tileY];
        string pathname = string.Concat(source.x, ",", source.y, ",", x, ",", y);
        //Check the pathCache for this path, we may have already calculated it!
        if (pathCache.ContainsKey(pathname))
        {
            currentPath = pathFromString(pathCache[pathname]);
            //Debug.Log("Loaded path " + pathname + " from path cache!");
        }
        //If we haven't previously calculated the path, proceed as normal
        else
        {
            Node goal = graph[x, y];
            source.f = source.DistanceTo(goal);
            source.g = 0;
            open[string.Concat(source.x +","+ source.y)] = source;
            prev[source] = null;
            Node currentNode = source;

            while (open.Count > 0)
            {
                //Find the node with the lowest f and assign it to currentNode
                string indexOfMin = "";
                foreach(string i in open.Keys)
                {
                    if (indexOfMin.Equals(""))
                    {
                        indexOfMin = i;
                    }

                    else if (open[indexOfMin].f > open[i].f)
                    {
                        indexOfMin = i;
                    }
                }
                currentNode = open[indexOfMin];

                if (currentNode == goal)
                {
                    break;  // We got it! Exit the while loop
                }

                currentNode.f = currentNode.g + currentNode.DistanceTo(goal);
                open.Remove(string.Concat(currentNode.x +","+ currentNode.y));
                close.Add(string.Concat(currentNode.x + "," + currentNode.y), currentNode);
                //A* algorithm
                foreach (Node n in currentNode.neighbours)
                {
                    if (!containsNode(close,n)&& !tiles[n.x,n.y].occupied)
                    {
                        n.g = currentNode.g + CostToEnterTile(n.x, n.y, currentNode.x, currentNode.y);
                        n.f = n.g + n.DistanceTo(goal);
                        if (!containsNode(open, n))
                        {
                            open.Add(string.Concat(n.x+","+n.y), n);
                            prev[n] = currentNode;
                        }
                        else
                        {
                            //We already have this node in the open list, so we need to see if this path to this node is faster
                            string openNeighbourIndex = string.Concat(n.x + "," + n.y);
                            Node openNeighbour = open[openNeighbourIndex];
                            //If it faster to get to this node via the currentNode, replace prev data for this node with the currentNode
                            if (n.g < openNeighbour.g)
                            {
                                open[openNeighbourIndex].g = n.g;
                                prev[n] = currentNode;
                            }
                        }
                    }
                }
            }

            //If the goal was not found, return
            if (currentNode != goal)
            {
                //Debug.Log("No path was found");
                return;
            }

            // Step through the "prev" chain and add it to our path
            while (currentNode != null)
            {
                //Debug.Log("-> Tile " + currentNode.x + "," + currentNode.y);
                currentPath.Add(currentNode);
                currentNode = prev[currentNode];
            }
            //Add both the path and it's inverse to the path cache to make future traversals faster
            //This uses slightly more memory but should massively cut down on CPU load later on
            if (!pathCache.ContainsKey(pathname))
            {
                pathCache.Add(string.Concat(x, ",", y, ",", currentPath[currentPath.Count - 1].x,
                    ",", currentPath[currentPath.Count - 1].y), 
                    pathToString(currentPath));
                currentPath.Reverse();
                pathCache.Add(pathname, pathToString(currentPath));
            }
            else
            {
                currentPath.Reverse();
            }
            tiles[x, y].isDestination = true;
            //Debug.Log("Close list size: " + close.Count);
            //Debug.Log("Open list size: " + open.Count);
            //Debug.Log("Prev list size: " + prev.Count);
            //Debug.Log("Path length: " + currentPath.Count);
        }

        // If the unit had a path, set its old destination tile to unoccupied, clear the path and move the unit to the tile it is meant to be on.
        if (unit.currentPath != null)
        {
            Node last = unit.currentPath[unit.currentPath.Count - 1];
            tiles[last.x, last.y].occupied = false;
            //If the unit is currently not moving in the correct direction, reset its position then replace its path
            if (unit.currentPath[0] != currentPath[1])
            {
                //Debug.Log(unit.name + " is not moving in the correct direction, teleporting unit to tile " + TileCoordToWorldCoord(unit.tileX, unit.tileY));
                unit.transform.position = TileCoordToWorldCoord(
                    unit.tileX,
                    unit.tileY);
            }
            //If the unit is currently moving in the correct direction, remove the first node of the path as it will have already gone past that tile
            else
            {
                currentPath.RemoveAt(0);
            }
        }
        unit.setPath(currentPath);
        unit.currentState = Unit.state.Moving;
    }
    //Determines the cost to enter a tile in position x,y
    public float CostToEnterTile(int sourceX, int sourceY, int targetX, int targetY)
    {

        TileType tt = tileTypes[tileMatrix[targetX, targetY]];

        float cost = tt.movementCost;

        if (sourceX != targetX && sourceY != targetY)
        {
            // We are moving diagonally!  Fudge the cost for tie-breaking
            // Purely a cosmetic thing!
            cost += 0.5f;
        }

        return cost;

    }

    public bool UnitCanEnterTile(int x, int y)
    {

        // We could test the unit's walk/hover/fly type against various
        // terrain flags here to see if they are allowed to enter the tile.

        return tileTypes[tileMatrix[x, y]].isWalkable;
    }

    public void setSelectedUnit(GameObject newUnit)
    {
        multipleUnitsSelected = false;
        
        foreach (GameObject u in selectedUnits)
        {
            u.GetComponent<Unit>().selected = false;
            u.GetComponent<Unit>().changeHighlight();
        }
        selectedUnits.Clear();

        //If there is another single unit selected
        if (selectedUnit != null)
        {
            selectedUnit.GetComponent<Unit>().selected = false;
            selectedUnit.GetComponent<Unit>().changeHighlight();
        }
        newUnit.GetComponent<Unit>().selected = true;
        newUnit.GetComponent<Unit>().changeHighlight();
        selectedUnit = newUnit;
    }

    public void setMultipleSelectedUnits(List<GameObject> newUnits)
    {
        multipleUnitsSelected = true;
        selectedUnit = null;
        foreach (GameObject u in selectedUnits)
        {
            u.GetComponent<Unit>().selected = false;
            u.GetComponent<Unit>().changeHighlight();
        }
        selectedUnits = new List<GameObject>();
        foreach(GameObject u in newUnits)
        {
            u.GetComponent<Unit>().selected = true;
            u.GetComponent<Unit>().changeHighlight();
            selectedUnits.Add(u);
        }
    }

    public void deselectUnit(GameObject unit)
    {
        if (multipleUnitsSelected)
        {
            if (selectedUnits.Contains(unit)) {
                unit.GetComponent<Unit>().selected = false;
                unit.GetComponent<Unit>().changeHighlight();
                selectedUnits.Remove(unit);
            }
            if (selectedUnits.Count==1)
            {
                multipleUnitsSelected = false;
                setSelectedUnit(selectedUnits[0]);
                selectedUnits = null;
            }
        }
        else
        {
            if (unit.Equals(selectedUnit)) {
                unit.GetComponent<Unit>().selected = false;
                unit.GetComponent<Unit>().changeHighlight();
                selectedUnit = null;
            }
        }
    }

    //Converts a path to string for easy storage
    public string pathToString(List<Node> path)
    {
        string pathString="";
        foreach (Node n in path)
        {
            pathString = string.Concat(pathString, n.x, "," , n.y, ",");
        }
        return pathString;
    }

    //Converts a path from string
    public List<Node> pathFromString(string path)
    {
        List<Node> pathList = new List<Node>();
        int x = 0;
        int y = 0;
        string tempX="";
        string tempY="";
        for (int i=0; i<path.Length; i++)
        {
            while (!path[i].Equals(','))
            {
                tempX = string.Concat(path[i]);
                i++;
            }
            if(!tempX.Equals(""))
                x = int.Parse(tempX);
            i++;

            while (!path[i].Equals(','))
            {
                tempY = string.Concat(path[i]);
                i++;
            }
            if (!tempX.Equals(""))
                y = int.Parse(tempY);
            pathList.Add(graph[x, y]);
        }
        return pathList;
    }

    public bool containsNode(Dictionary<string,Node> list, Node node)
    {
        foreach(KeyValuePair<string,Node> current in list)
        {
            if(current.Value.x==node.x && current.Value.y == node.y)
            {
                return true;
            }
        }
        return false;
    }

    //Called when a selection box has been released
    public void handleUnitSelection()
    {
        //**All box-drawing code will need to be rewritten for optimization. Research usage of a box collider rather than
        //iterating through all units**
        List<GameObject> newUnits = new List<GameObject>();
        Rect rect = new Rect();
        if(srtBoxPos.x < endBoxPos.x)
        {
            rect.xMin = srtBoxPos.x;
            rect.xMax = endBoxPos.x;
        }
        else
        {
            rect.xMin = endBoxPos.x;
            rect.xMax = srtBoxPos.x;
        }
        if (srtBoxPos.y < endBoxPos.y)
        {
            rect.yMin = srtBoxPos.y;
            rect.yMax = endBoxPos.y;
        }
        else
        {
            rect.yMin = endBoxPos.y;
            rect.yMax = srtBoxPos.y;
        }
        foreach (GameObject u  in units)
        {
            Vector2 unitScreenPosition = viewingCamera.WorldToScreenPoint(new Vector2(u.transform.position.x, u.transform.position.y));
            Debug.Log(u.name + " Screen position is " + unitScreenPosition);
            if (rect.Contains(unitScreenPosition))
            {
                Debug.Log(u.name + " was selected");
                newUnits.Add(u);
                u.GetComponent<Unit>().turnOffPreSelectionHighlight();
            }
        }
        //If no units were found in the drawn area, do not change what units are selected
        if(newUnits.Count == 0)
        {
            return;
        }
        //If only one unit was selected, set that unit as the sole selected unit
        else if(newUnits.Count == 1)
        {
            setSelectedUnit(newUnits.ElementAt(0));
        }
        else
        {
            setMultipleSelectedUnits(newUnits);
        }
    }
}