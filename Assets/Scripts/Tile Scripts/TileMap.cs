using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

        // Setup the "Q" -- the list of nodes we haven't checked yet.
        List<Node> unvisited = new List<Node>();

        Node source = graph[
                            unit.GetComponent<Unit>().tileX,
                            unit.GetComponent<Unit>().tileY
                            ];

        Node target = graph[x,y];

        dist[source] = 0;
        prev[source] = null;

        // Initialize everything to have INFINITY distance, since
        // we don't know any better right now. Also, it's possible
        // that some nodes CAN'T be reached from the source,
        // which would make INFINITY a reasonable value
        foreach (Node v in graph)
        {
            if (v != source)
            {
                dist[v] = Mathf.Infinity;
                prev[v] = null;
            }

            unvisited.Add(v);
        }

        while (unvisited.Count > 0)
        {
            // "u" is going to be the unvisited node with the smallest distance.
            Node u = null;

            foreach (Node possibleU in unvisited)
            {
                if (u == null || dist[possibleU] < dist[u])
                {
                    u = possibleU;
                }
            }

            if (u == target)
            {
                break;  // Exit the while loop!
            }

            unvisited.Remove(u);

            foreach (Node v in u.neighbours)
            {
                //float alt = dist[u] + u.DistanceTo(v);
                float alt = dist[u] + CostToEnterTile(x,y,v.x,v.y);
                if (alt < dist[v])
                {
                    dist[v] = alt;
                    prev[v] = u;
                }
            }
        }

        // If we get there, the either we found the shortest route
        // to our target, or there is no route at ALL to our target.

        if (prev[target] == null)
        {
            // No route between our target and the source
            return;
        }

        List<Node> currentPath = new List<Node>();

        Node curr = target;

        // Step through the "prev" chain and add it to our path
        while (curr != null)
        {
            currentPath.Add(curr);
            curr = prev[curr];
        }

        // Right now, currentPath describes a route from out target to our source
        // So we need to invert it!

        currentPath.Reverse();

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

    public float pathfindingHeuristic(Node start, Node goal)
    {
        return (float)System.Math.Sqrt(Mathf.Pow(start.x - goal.x, 2) + Mathf.Pow(start.y - goal.y, 2));
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

    //Determines the cost to enter a tile in position x,y
    public float CostToEnterTile(int sourceX, int sourceY, int targetX, int targetY)
    {

        TileType tt = tileTypes[tiles[targetX, targetY]];

        if (UnitCanEnterTile(targetX, targetY) == false) {
            return Mathf.Infinity;
        }

        float cost = tt.movementCost;

        if (sourceX != targetX && sourceY != targetY)
        {
            // We are moving diagonally!  Fudge the cost for tie-breaking
            // Purely a cosmetic thing!
            cost += 0.001f;
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
}
