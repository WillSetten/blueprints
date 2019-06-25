using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TileMap : MonoBehaviour
{
    //Each unit should have a click handler which, when selected, should make this variable equal to that unit.
    public GameObject selectedUnit;
    public List<GameObject> units;
    public TileType[] tileTypes;
    public bool paused = false;

    int[,] tiles; //2D Integer array for showing which tiles are passable and which aren't
    Node[,] graph; //2D Array of Nodes for pathfinding

    public int mapSizeX = 10;
    public int mapSizeY = 10;

    Dictionary<string,string> pathCache;

    //Initialisation
    private void Start()
    {
        //Setup all units variables
        foreach (GameObject u in units)
        {
            u.GetComponent<Unit>().map = this;
        }
        GenerateMapData();
        GeneratePathfindingGraph();
        GenerateMapVisuals();
        pathCache = new Dictionary<string, string>();
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
                paused = false;
            }
            else
            {
                //If game is getting paused, stop all unit animations
                foreach(GameObject u in units)
                {
                    u.GetComponent<Animator>().enabled = false;
                }
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
        tiles = new int[mapSizeX, mapSizeY];

        //Initialize map
        for (int x= 0; x< mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                tiles[x, y] = 0;
            }
        }
        tiles[2, 6] = 1;
        tiles[2, 5] = 1;
        tiles[3, 4] = 1;
        tiles[4, 4] = 1;
        tiles[5, 4] = 1;
        tiles[6, 4] = 1;
        tiles[7, 4] = 1;
        tiles[8, 4] = 1;
        tiles[8, 5] = 1;
        tiles[8, 6] = 1;
    }

    //Generates the actual tiles from the given map
    void GenerateMapVisuals()
    {
        for (int x= 0; x< mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                TileType tt = tileTypes[tiles[x , y]];
                GameObject go = (GameObject)Instantiate(tt.tileVisualPrefab, new Vector3(x, y, 0), Quaternion.identity);

                ClickableTile ct = go.GetComponent<ClickableTile>();
                ct.tileX = x ;
                ct.tileY = y;
                ct.map = this;
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
    public void GeneratePathTo(int x, int y, GameObject unit)
    {
        List<Node> currentPath = new List<Node>();
        Dictionary<int, Node> open = new Dictionary<int,Node>();
        Dictionary<int, Node> close = new Dictionary<int, Node>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();
        Node source = graph[selectedUnit.GetComponent<Unit>().tileX, selectedUnit.GetComponent<Unit>().tileY];
        string pathname = string.Concat(source.x, source.y, x, y);
        //Check the pathCache for this path, we may have already calculated it!
        if (pathCache.ContainsKey(pathname))
        {
            Debug.Log("Found path " + pathname + " in the pathCache!");
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
            open[int.Parse(string.Concat(source.x +""+ source.y))] = source;
            prev[source] = null;
            Node currentNode = source;

            while (open.Count > 0)
            {
                //Find the node with the lowest f and assign it to currentNode
                int indexOfMin = 0;
                foreach(int i in open.Keys)
                {
                    if (indexOfMin == 0)
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
                open.Remove(int.Parse(string.Concat(currentNode.x +""+ currentNode.y)));
                close.Add(close.Count, currentNode);
                foreach (Node n in currentNode.neighbours)
                {
                    if (!containsNode(close,n))
                    {
                        prev[n] = currentNode;
                        n.g = currentNode.g + CostToEnterTile(n.x, n.y, currentNode.x, currentNode.y);
                        n.f = n.g + n.DistanceTo(goal);
                        if (!containsNode(open, n))
                        {
                            open.Add(int.Parse(string.Concat(n.x+""+n.y)), n);
                        }
                        else
                        {
                            int openNeighbourIndex = int.Parse(string.Concat(n.x + "" + n.y));
                            Node openNeighbour = open[openNeighbourIndex];
                            if (n.g < openNeighbour.g)
                            {
                                open.Remove(openNeighbourIndex);
                                open.Add(openNeighbourIndex, n);
                                prev[open[openNeighbourIndex]] = currentNode;
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
                Debug.Log("-> Tile " + currentNode.x + "," + currentNode.y);
                currentPath.Add(currentNode);
                currentNode = prev[currentNode];
            }
            //Add both the path and it's inverse to the path cache to make future traversals faster
            //This uses slightly more memory but should massively cut down on CPU load later on
            if (!pathCache.ContainsKey(pathname))
            {
                pathCache.Add(string.Concat(x, y, currentPath[currentPath.Count - 1].x, 
                    currentPath[currentPath.Count - 1].y), 
                    pathToString(currentPath));
                currentPath.Reverse();
                pathCache.Add(pathname, pathToString(currentPath));
            }
            else
            {
                currentPath.Reverse();
            }

            Debug.Log("Close list size: " + close.Count);
            Debug.Log("Open list size: " + open.Count);
            Debug.Log("Prev list size: " + prev.Count);
            Debug.Log("Path length: " + currentPath.Count);
        }

        // If the unit had a path, clear it and move the unit to the tile it is meant to be on.
        if (unit.GetComponent<Unit>().currentPath != null)
        {
            //If the unit is currently not moving in the correct direction, reset its position then replace its path
            if (unit.GetComponent<Unit>().currentPath[0] != currentPath[1])
            {
                unit.transform.position = TileCoordToWorldCoord(
                    unit.GetComponent<Unit>().tileX,
                    unit.GetComponent<Unit>().tileY);
            }
            //If the unit is currently moving in the correct direction, remove the first node of the path as it will have already gone past that tile
            else
            {
                currentPath.RemoveAt(0);
            }
        }
        unit.GetComponent<Unit>().setPath(currentPath);
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
                        graph[x, y].neighbours.Add(graph[x - 1, y]);
                        if (y > 0 && tiles[x - 1, y] != 1 && tiles[x, y - 1] != 1)
                            graph[x, y].neighbours.Add(graph[x - 1, y - 1]);
                        if (y < mapSizeY - 1 && tiles[x - 1, y] != 1 && tiles[x, y + 1] != 1)
                            graph[x, y].neighbours.Add(graph[x - 1, y + 1]);
                    }

                    // Try Right
                    if (x < mapSizeX - 1)
                    {
                        graph[x, y].neighbours.Add(graph[x + 1, y]);
                        if (y > 0 && tiles[x + 1, y] != 1 && tiles[x, y - 1] != 1)
                            graph[x, y].neighbours.Add(graph[x + 1, y - 1]);
                        if (y < mapSizeY - 1 && tiles[x + 1, y] != 1 && tiles[x, y + 1] != 1)
                            graph[x, y].neighbours.Add(graph[x + 1, y + 1]);
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

        TileType tt = tileTypes[tiles[targetX, targetY]];

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

        return tileTypes[tiles[x, y]].isWalkable;
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
            pathString = string.Concat(pathString, n.x, n.y);
        }
        return pathString;
    }

    //Converts a path from string
    public List<Node> pathFromString(string path)
    {
        List<Node> pathList = new List<Node>();
        int x = 0;
        int y = 0;
        for (int i=0; i<path.Length; i=i+2)
        {
            x = (int)char.GetNumericValue(path[i]);
            y = (int)char.GetNumericValue(path[i + 1]);
            pathList.Add(graph[x,y]);
        }
        return pathList;
    }

    public bool containsNode(Dictionary<int,Node> list, Node node)
    {
        for(int i=0; i<list.Count; i++)
        {
            Node current = list.ElementAt(i).Value;
            if(current.x==node.x && current.y == node.y)
            {
                return true;
            }
        }
        return false;
    }
}
