using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TileMap : MonoBehaviour
{
    //Each unit should have a click handler which, when selected, should make this variable equal to that unit.
    public GameObject selectedUnit;
    public List<GameObject> units;
    public EnemyController enemyController;
    public TileType[] tileTypes;
    public bool paused = false;

    List<Room> rooms; //List of Rooms
    int[,] tileMatrix; //2D Integer array for showing which tiles are passable and which aren't
    Node[,] graph; //2D Array of Nodes for pathfinding
    public Tile[,] tiles; //2D Array of tiles

    public int mapSizeX;
    public int mapSizeY;

    Dictionary<string,string> pathCache;

    //Initialisation
    private void Start()
    {
        enemyController = GameObject.Find("Enemy Controller").GetComponent<EnemyController>();
        GenerateMapData();
        GeneratePathfindingGraph();
        GenerateMapVisuals();
        pathCache = new Dictionary<string, string>();
        rooms = new List<Room>();
        mapSizeX = 26;
        mapSizeY = 26;
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            Debug.Log("Space Pressed");
            if(paused)
            {
                //If game is getting unpaused, start up all unit animations again
                foreach (GameObject u in units)
                {
                    u.GetComponent<Animator>().enabled = true;
                }
                enemyController.togglePause();
                paused = false;
            }
            else
            {
                //If game is getting paused, stop all unit animations
                foreach(GameObject u in units)
                {
                    u.GetComponent<Animator>().enabled = false;
                }
                enemyController.togglePause();
                paused = true;
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

    //Decides how large the map is, which tiles (floor, walls) go where in the map
    void GenerateMapData()
    {
        //Allocate Map tiles
        tileMatrix = new int[mapSizeX, mapSizeY];
        tiles = new Tile[mapSizeX,mapSizeY];
        //Initialize map
        for (int x= 0; x< mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                tileMatrix[x, y] = 0;
            }
        }
        harvestAndTrustee();
    }

    //Generates the actual tiles from the given map
    void GenerateMapVisuals()
    {
        for (int x= 0; x< mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                TileType tt = tileTypes[tileMatrix[x , y]];
                GameObject go = (GameObject)Instantiate(tt.tileVisualPrefab, new Vector3(x, y, 1), Quaternion.identity);

                Tile ct = go.GetComponent<Tile>();
                ct.tileX = x ;
                ct.tileY = y;
                ct.map = this;
                tiles[x, y] = ct;
            }
        }
    }

    //Converts Tile Co-ordinates with Co-ordinates in the actual world. For now, this is yust a matter of converting two ints into
    //a Vector3. Any scaling issues will probably have to be fixed in this method.
    public Vector3 TileCoordToWorldCoord(float x, float y)
    {
        return new Vector3(x, y, 0);
    }

    //Takes in an x and y to move the selected unit to. This method currently uses basic Dyikstra
    public void GeneratePathTo(int x, int y, Unit unit)
    {
        //If the units current destination is the same as the one thats been clicked on, return
        if (unit.currentPath != null) {
            Node currentDestination = unit.currentPath[unit.currentPath.Count - 1];
            if (currentDestination.x == x && currentDestination.y == y)
            {
                return;
            }
        }
        //If the unit is attempting to travel to the same tile
        if (x == unit.tileX && y == unit.tileY)
        {
            return;
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
        }
        //If we haven't previously calculated the path, proceed as normal
        else
        {
            Node goal = graph[x, y];

            if (UnitCanEnterTile(x, y) == false)
            {
                // We probably clicked on a mountain or something, so just quit out.
                return;
            }

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
                    if (!containsNode(close,n))
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

            //Debug.Log("Close list size: " + close.Count);
            //Debug.Log("Open list size: " + open.Count);
            //Debug.Log("Prev list size: " + prev.Count);
            //Debug.Log("Path length: " + currentPath.Count);
        }

        // If the unit had a path, clear it and move the unit to the tile it is meant to be on.
        if (unit.currentPath != null)
        {
            //If the unit is currently not moving in the correct direction, reset its position then replace its path
            if (unit.currentPath[0] != currentPath[1])
            {
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
                if (UnitCanEnterTile(x,y)) {
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
        selectedUnit = newUnit;
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

    public void harvestAndTrustee()
    {
        tileMatrix[5, 2] = 1;
        tileMatrix[6, 2] = 1;
        tileMatrix[7, 2] = 1;
        tileMatrix[8, 2] = 1;
        tileMatrix[9, 2] = 1;
        tileMatrix[10, 2] = 1;
        tileMatrix[11, 2] = 1;
        tileMatrix[14, 2] = 1;
        tileMatrix[15, 2] = 1;

        tileMatrix[5, 3] = 1;
        tileMatrix[15, 3] = 1;

        tileMatrix[5, 4] = 1;
        tileMatrix[15, 4] = 1;

        tileMatrix[5, 5] = 1;
        tileMatrix[15, 5] = 1;

        tileMatrix[5, 6] = 1;
        tileMatrix[8, 6] = 1;
        tileMatrix[10, 6] = 1;
        tileMatrix[15, 6] = 1;

        tileMatrix[5, 7] = 1;
        tileMatrix[8, 7] = 1;
        tileMatrix[10, 7] = 1;
        tileMatrix[15, 7] = 1;
        
        tileMatrix[8, 8] = 1;
        tileMatrix[10, 8] = 1;
        tileMatrix[15, 8] = 1;
        tileMatrix[16, 8] = 1;
        tileMatrix[17, 8] = 1;
        tileMatrix[18, 8] = 1;
        tileMatrix[19, 8] = 1;
        tileMatrix[20, 8] = 1;
        tileMatrix[21, 8] = 1;

        tileMatrix[5, 9] = 1;
        tileMatrix[8, 9] = 1;
        tileMatrix[10, 9] = 1;
        tileMatrix[21, 9] = 1;

        tileMatrix[2, 10] = 1;
        tileMatrix[3, 10] = 1;
        tileMatrix[4, 10] = 1;
        tileMatrix[5, 10] = 1;
        tileMatrix[21, 10] = 1;

        tileMatrix[2, 11] = 1;
        tileMatrix[21, 11] = 1;

        tileMatrix[2, 12] = 1;
        tileMatrix[5, 12] = 1;
        tileMatrix[12, 12] = 1;
        tileMatrix[14, 12] = 1;
        tileMatrix[15, 12] = 1;
        tileMatrix[21, 12] = 1;


        tileMatrix[2, 13] = 1;
        tileMatrix[3, 13] = 1;
        tileMatrix[4, 13] = 1;
        tileMatrix[5, 13] = 1;
        tileMatrix[6, 13] = 1;
        tileMatrix[7, 13] = 1;
        tileMatrix[8, 13] = 1;
        tileMatrix[9, 13] = 1;
        tileMatrix[10, 13] = 1;
        tileMatrix[11, 13] = 1;
        tileMatrix[12, 13] = 1;
        tileMatrix[15, 13] = 1;
        tileMatrix[21, 13] = 1;

        tileMatrix[4, 14] = 1;
        tileMatrix[5, 14] = 1;
        tileMatrix[6, 14] = 1;
        tileMatrix[7, 14] = 1;
        tileMatrix[8, 14] = 1;
        tileMatrix[9, 14] = 1;
        tileMatrix[12, 14] = 1;
        tileMatrix[21, 14] = 1;

        tileMatrix[4, 15] = 1;
        tileMatrix[5, 15] = 1;
        tileMatrix[9, 15] = 1;
        tileMatrix[12, 15] = 1;
        tileMatrix[13, 15] = 1;
        tileMatrix[15, 15] = 1;
        tileMatrix[16, 15] = 1;
        tileMatrix[21, 15] = 1;

        tileMatrix[4, 16] = 1;
        tileMatrix[5, 16] = 1;
        tileMatrix[9, 16] = 1;
        tileMatrix[12, 16] = 1;
        tileMatrix[21, 16] = 1;

        tileMatrix[4, 17] = 1;
        tileMatrix[5, 17] = 1;
        tileMatrix[21, 17] = 1;

        tileMatrix[4, 18] = 1;
        tileMatrix[5, 18] = 1;
        tileMatrix[9, 18] = 1;
        tileMatrix[12, 18] = 1;
        tileMatrix[13, 18] = 1;
        tileMatrix[14, 18] = 1;
        tileMatrix[15, 18] = 1;
        tileMatrix[16, 18] = 1;
        tileMatrix[18, 18] = 1;
        tileMatrix[19, 18] = 1;
        tileMatrix[21, 18] = 1;
        tileMatrix[22, 18] = 1;
        tileMatrix[23, 18] = 1;

        tileMatrix[4, 19] = 1;
        tileMatrix[5, 19] = 1;
        tileMatrix[9, 19] = 1;
        tileMatrix[12, 19] = 1;
        tileMatrix[18, 19] = 1;
        tileMatrix[23, 19] = 1;

        tileMatrix[4, 20] = 1;
        tileMatrix[5, 20] = 1;
        tileMatrix[6, 20] = 1;
        tileMatrix[7, 20] = 1;
        tileMatrix[8, 20] = 1;
        tileMatrix[9, 20] = 1;
        tileMatrix[12, 20] = 1;
        tileMatrix[15, 20] = 1;
        tileMatrix[18, 20] = 1;
        tileMatrix[23, 20] = 1;

        tileMatrix[4, 21] = 1;
        tileMatrix[5, 21] = 1;
        tileMatrix[6, 21] = 1;
        tileMatrix[7, 21] = 1;
        tileMatrix[8, 21] = 1;
        tileMatrix[9, 21] = 1;
        tileMatrix[10, 21] = 1;
        tileMatrix[11, 21] = 1;
        tileMatrix[12, 21] = 1;
        tileMatrix[13, 21] = 1;
        tileMatrix[14, 21] = 1;
        tileMatrix[15, 21] = 1;
        tileMatrix[18, 21] = 1;
        tileMatrix[23, 21] = 1;

        tileMatrix[15, 22] = 1;
        tileMatrix[18, 22] = 1;
        tileMatrix[19, 22] = 1;
        tileMatrix[20, 22] = 1;
        tileMatrix[21, 22] = 1;
        tileMatrix[22, 22] = 1;
        tileMatrix[23, 22] = 1;

        tileMatrix[15, 23] = 1;

        tileMatrix[15, 24] = 1;
        tileMatrix[16, 24] = 1;
        tileMatrix[17, 24] = 1;
        tileMatrix[18, 24] = 1;
        tileMatrix[19, 24] = 1;

        /*rooms.Add(new Room(this, 2, 2, 3, 11));
        rooms.Add(new Room(this, 2, 2, 13, 13));
        rooms.Add(new Room(this, 4, 3, 19, 19));
        rooms.Add(new Room(this, 2, 2, 13, 19));
        rooms.Add(new Room(this, 2, 5, 16, 11));
        rooms.Add(new Room(this, 3, 5, 6, 15));
        rooms.Add(new Room(this, 2, 7, 10, 14));*/
    }
}
