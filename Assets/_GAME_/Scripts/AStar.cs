using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class AStar
{
    private static Vector2 gridSize; 

    private class Node //Node class - each cell in the tilemap will represent a node
    {
        public Vector2Int positon;
        public Node parent;
        public int gCost;
        public int hCost;
        public int fCost => gCost + hCost;
        public bool isObstacle;

        public Node(Vector2Int positon, Node parent, int gCost, int hCost, bool isObstacle)
        {
            this.positon = positon;
            this.parent = parent;
            this.gCost = gCost;
            this.hCost = hCost;
            this.isObstacle = isObstacle;
        }
    }

    private static int CalculateHCost(Vector2Int current, Vector2Int target) //Calculates the Heuristic which is an estimate cost 
    {                                                                        //from current node to target node
        return Mathf.Abs(current.x - target.x) + Mathf.Abs(current.y - target.y);
    }

    public static List<Vector2> FindPath(Vector2 start, Vector2 target, Vector2 gridCellSize, System.Func<Vector2, bool> isObstacle) //A* path finder
    {
        gridSize = gridCellSize;
        Vector2Int startGridPos = GridUtils.WorldToGrid(start);
        Vector2Int targetGridPos = GridUtils.WorldToGrid(target);

        List<Node> openList = new List<Node>();
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();

        Node startNode = new Node(startGridPos, null, 0, CalculateHCost(startGridPos, targetGridPos), false);
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            Node currentNode = GetLowestFCost(openList);

            openList.Remove(currentNode);
            closedSet.Add(currentNode.positon);

            if (currentNode.positon == targetGridPos)
            {
                return GeneratePath(currentNode);
            }

            foreach (Vector2Int adjacentPosition in GetAdjacentPositions(currentNode.positon))
            {
                if (closedSet.Contains(adjacentPosition) || isObstacle(GridUtils.GridToWorld(adjacentPosition)))
                {
                    continue;
                }

                int gCost = currentNode.gCost + 1;
                int hCost = CalculateHCost(adjacentPosition, targetGridPos);

                Node adjacentNode = new Node(adjacentPosition, currentNode, gCost, hCost, false);

                int index = openList.FindIndex(node =>  node.positon == adjacentNode.positon);
                if (index != -1)
                {
                    if (gCost < openList[index].gCost)
                    {
                        openList[index].parent = currentNode;
                        openList[index].gCost = gCost;
                    }
                }
                else
                {
                    openList.Add(adjacentNode);
                }
            }
        }

        return null;
    }

    private static Node GetLowestFCost(List<Node> nodes) //finds the node with lowest F cost
    {
        Node lowestCostNode = nodes[0];
        for (int i = 1; i < nodes.Count; i++)
        {
            if (nodes[i].fCost < lowestCostNode.fCost || (nodes[i].fCost == lowestCostNode.fCost && nodes[i].hCost < lowestCostNode.hCost))
            {
                lowestCostNode = nodes[i];
            }

        }
        return lowestCostNode;
    }

    private static List<Vector2> GeneratePath(Node endNode) //generates the path to travel 
    {
        List<Vector2> path = new List<Vector2>();
        Node currentNode = endNode;
        while (currentNode != null) 
        {
            path.Add(GridUtils.GridToWorld(currentNode.positon));
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }

    private static List<Vector2Int> GetAdjacentPositions(Vector2Int position) //gets a list of adjacent positions of a node
    {
        return new List<Vector2Int>
        {
            position + Vector2Int.up,
            position + Vector2Int.down,
            position + Vector2Int.left,
            position + Vector2Int.right,
        };
    }

}
