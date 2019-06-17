using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public int tileX;
    public int tileY;
    public int moveRate;
    public TileMap map;

    public List<Node> currentPath = null;

    void Update()
    {
        if (currentPath !=null)
        {
            int moveCounter = moveRate;
            int currNode = 0;
            while(currNode < currentPath.Count-1)
            {
                Vector3 start = map.TileCoordToWorldCoord(currentPath[currNode].x, currentPath[currNode].y);
                Vector3 end = map.TileCoordToWorldCoord(currentPath[currNode + 1].x, currentPath[currNode + 1].y);
                Debug.DrawLine(start, end);
                currNode = currNode + 1;
            }
            if (moveCounter == 0)
            {
                MoveNextTile();
                moveCounter = moveRate;
            }
            else
            {
                moveCounter = moveCounter - 1;
            }
        }
    }

    public void MoveNextTile()
    {
        if (currentPath == null)
            return;

        currentPath.RemoveAt(0);

        transform.position = map.TileCoordToWorldCoord(currentPath[0].x, currentPath[0].y);

        if(currentPath.Count == 1)
        {
            tileX = currentPath[0].x;
            tileY = currentPath[0].y;
            currentPath = null;
        }
    }
}
