using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAI.PathFinding;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Oware;

/// <summary>
/// A sample graph for creating and testing a graph based path finding.
/// </summary>
public class SampleGraph : Graph<GraphNodeData>
{
    public Rect Extent { get { return mExtent; } }
    private Rect mExtent;

    public SampleGraph()
    {
    }

    /// <summary>
    /// Remove nodes that do not have any neighbours.
    /// </summary>
    public void RemoveDanglingNodes()
    {

    }

    public void CalculateExtent()
    {
        float minX = Mathf.Infinity;
        float minY = Mathf.Infinity;
        float maxX = -Mathf.Infinity;
        float maxY = -Mathf.Infinity;
        for (int i = 0; i < Nodes.Count; ++i)
        {
            GraphNodeData d = Nodes[i].Value;
            Vector2 p = d.Point;

            if (minX > p.x) minX = p.x;
            if (minY > p.y) minY = p.y;
            if (maxX <= p.x) maxX = p.x;
            if (maxY <= p.y) maxY = p.y;
        }

        mExtent.xMin = minX;
        mExtent.xMax = maxX;
        mExtent.yMin = minY;
        mExtent.yMax = maxY;
    }

    GraphNode<GraphNodeData> FindByName(string name)
    {
        for(int i = 0; i < Nodes.Count; ++i)
        {
            if (name.Equals(Nodes[i].Value.Name))
                return (GraphNode<GraphNodeData>)(Nodes[i]);
        }
        return null;
    }

    public static void Save(SampleGraph graph, string filen)
    {
        string filename = Application.persistentDataPath + "/" + filen;

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(filename);

        try
        {
            bf.Serialize(file, graph.Nodes.Count);
            for (int i = 0; i < graph.Nodes.Count; ++i)
            {
                GraphNodeData d = graph.Nodes[i].Value;

                bf.Serialize(file, d.Name);
                bf.Serialize(file, d.Point.x);
                bf.Serialize(file, d.Point.y);
            }
            // the edges.
            for (int i = 0; i < graph.Nodes.Count; ++i)
            {
                List<Node<GraphNodeData>> neighbours = graph.Nodes[i].Neighbours;
                if (neighbours != null)
                {
                    bf.Serialize(file, neighbours.Count);

                    for (int j = 0; j < neighbours.Count; ++j)
                    {
                        GraphNode<GraphNodeData> gn = (GraphNode<GraphNodeData>)(neighbours[j]);
                        bf.Serialize(file, gn.Value.Name);
                        bf.Serialize(file, ((GraphNode<GraphNodeData>)graph.Nodes[i]).Costs[j]);
                    }
                }
                else
                {
                    bf.Serialize(file, 0);
                }
            }
        }
        catch (SerializationException e)
        {
            Debug.Log("Failed to save graph map. Reason: " + e.Message);
            throw;
        }
        finally
        {
            file.Close();
        }
    }

    // static method to load a map from a file.
    public static bool Load(SampleGraph graph, string filen)
    {
        string filename = Application.persistentDataPath + "/" + filen;
        if (!File.Exists(filename))
            return false;

        BinaryFormatter bf = new BinaryFormatter();
        using (FileStream file = new FileStream(filename, FileMode.Open))
        {
            try
            {
                int nodeCount = 0;
                nodeCount = (int)bf.Deserialize(file);
                for (int i = 0; i < nodeCount; ++i)
                {
                    GraphNodeData d = new GraphNodeData();
                    d.Name = (string)bf.Deserialize(file);
                    float x = (float)bf.Deserialize(file);
                    float y = (float)bf.Deserialize(file);
                    d.Point = new Vector2(x, y);
                    graph.AddNode(new GraphNode<GraphNodeData>(d));
                }
                for (int i = 0; i < graph.Nodes.Count; ++i)
                {
                    int neighbourCount = 0;
                    neighbourCount = (int)bf.Deserialize(file);

                    for (int j = 0; j < neighbourCount; ++j)
                    {
                        string name = (string)bf.Deserialize(file);
                        float cost = (float)bf.Deserialize(file);

                        GraphNode<GraphNodeData> gn = graph.FindByName(name);
                        graph.AddDirectedEdge((GraphNode<GraphNodeData>)graph.Nodes[i], gn, cost);
                    }
                }
            }
            catch (SerializationException e)
            {
                Debug.Log("Failed to load graph map. Reason: " + e.Message);
            }
            finally
            {
                file.Close();
            }
        }
        return true;
    }

    public static float GetManhattanCost(GraphNodeData a, GraphNodeData b)
    {
        return Mathf.Abs(a.Point.x - b.Point.x) + Mathf.Abs(a.Point.y - b.Point.y);
    }

    public static float GetEuclideanCost(GraphNodeData a, GraphNodeData b)
    {
        return GetCostBetweenTwoCells(a, b);
    }

    public static float GetCostBetweenTwoCells(GraphNodeData a, GraphNodeData b)
    {
        return Mathf.Sqrt(
                (a.Point.x - b.Point.x) * (a.Point.x - b.Point.x) +
                (a.Point.y - b.Point.y) * (a.Point.y - b.Point.y)
            );
    }

    public static SampleGraph CreateSampleGraph()
    {
        SampleGraph graph = new SampleGraph();

        graph.AddNode(new GraphNode<GraphNodeData>(
            new GraphNodeData(
                "Tampines Central 5", 1.352419f, 103.945524f
                )
            ));

        graph.AddNode(new GraphNode<GraphNodeData>(
             new GraphNodeData(
                 "Tampines Regional Library", 1.352187f, 103.941308f
                 )
             ));

        graph.AddNode(new GraphNode<GraphNodeData>(
             new GraphNodeData(
                 "250 Tampines Street 12", 1.350056f, 103.944820f
                 )
             ));

        graph.AddNode(new GraphNode<GraphNodeData>(
             new GraphNodeData(
                 "250 Tampines Street 12", 1.350056f, 103.944820f
                 )
             ));
        graph.AddNode(new GraphNode<GraphNodeData>(
             new GraphNodeData(
                "Kokoro Piano Studio ", 1.349692f, 103.946074f
                )
            ));
        graph.AddNode(new GraphNode<GraphNodeData>(
             new GraphNodeData(
                "Kokoro Piano Studio ", 1.349692f, 103.946074f
                )
            ));
        graph.AddNode(new GraphNode<GraphNodeData>(
             new GraphNodeData(
                "Dyslexia Association of Singapore", 1.349726f, 103.94757f
                )
            ));

        graph.AddNode(new GraphNode<GraphNodeData>(
             new GraphNodeData(
                "Starbucks", 1.35211f, 103.94437f
                )
            ));
        graph.AddNode(new GraphNode<GraphNodeData>(
            new GraphNodeData(
                "SPC Tampines", 1.34872f, 103.938612f
                )
            ));
        graph.AddNode(new GraphNode<GraphNodeData>(
            new GraphNodeData(
                "Captain Kim Korean BBQ & Hotpot",1.352345f, 103.94184f
                )
            ));

        LatLngUTMConverter mLatLonConverter = new LatLngUTMConverter("WGS 84");
        for(int i = 0; i < graph.Nodes.Count; i++)
        {
            GraphNodeData d = graph.Nodes[i].Value;

            LatLngUTMConverter.UTMResult res = mLatLonConverter.convertLatLngToUtm(d.Point.x, d.Point.y);
            float e = (float)res.Easting;
            float n = (float)res.Northing;
            d.Point = new Vector2(e, n);

        }

        graph.CalculateExtent();

        // create edges
        //graph.mGraph.AddDirectedEdge()
        return graph;
    }
}
