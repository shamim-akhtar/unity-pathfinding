using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphNodeData
{
    public string Name { get; set; }
    public Vector2 Point { get; set; }

    public GraphNodeData()
    { }
    public GraphNodeData(string name, Vector2 point)
    {
        Name = name;
        Point = point;
    }
    public GraphNodeData(string name, float x, float y)
    {
        Name = name;
        Point = new Vector2(x, y);
    }

    public static int Distance(GraphNodeData a, GraphNodeData b)
    {
        return (int)(a.Point - b.Point).sqrMagnitude;
    }
}