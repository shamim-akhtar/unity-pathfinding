using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAI.PathFinding;
using Oware;

/// <summary>
/// A sample graph for creating and testing a graph based path finding.
/// </summary>
public class SampleGraph : Graph<GraphNode<GraphNodeData>>
{
    public Rect Extent { get { return mExtent; } }
    private Rect mExtent;

    public SampleGraph()
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
            GraphNodeData d = Nodes[i].Value.Value;
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

    public void CreateGraphEditor(int xNum, int yNum)
    {

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
            GraphNodeData d = graph.Nodes[i].Value.Value;

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
