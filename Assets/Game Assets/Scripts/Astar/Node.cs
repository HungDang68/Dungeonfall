using UnityEngine;
using System;

public class Node : IComparable<Node>
{
    public Vector2Int gridPosition;
    public int gCost = 0;
    public int hCost = 0;
    public Node parentNode;

    public Node(Vector2Int gridPosition)
    {
        this.gridPosition = gridPosition;

        parentNode = null;
    }
    public int FCost
    {
        get
        {
            return gCost + hCost;
        }
    }
    public int CompareTo(Node other)
    {
        int compare = FCost.CompareTo(other.FCost);

        if (compare == 0)
        {
            compare = hCost.CompareTo(other.hCost);
        }

        return compare;
    }
}
