using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GraphNodeData : IEquatable<GraphNodeData>
{
    public string Name { get; set; }
    public Vector2 Point { get; set; }

    public GraphNodeData()
    {
    }

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

    public override bool Equals(object obj) => this.Equals(obj as GraphNodeData);
    public bool Equals(GraphNodeData p)
    {
        if (p is null)
        {
            return false;
        }

        // Optimization for a common success case.
        if (System.Object.ReferenceEquals(this, p))
        {
            return true;
        }

        // If run-time types are not exactly the same, return false.
        if (this.GetType() != p.GetType())
        {
            return false;
        }

        // Return true if the fields match.
        // Note that the base class is not invoked because it is
        // System.Object, which defines Equals as reference equality.
        return (Name == p.Name) && (Point == p.Point);
    }

    public override int GetHashCode() => (Name, Point).GetHashCode();

    public static float Distance(GraphNodeData a, GraphNodeData b)
    {
        return (a.Point - b.Point).magnitude;
    }
}