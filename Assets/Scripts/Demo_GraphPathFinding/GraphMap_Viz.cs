using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAI.PathFinding;
using UnityEngine.UI;

public class GraphMap_Viz : MonoBehaviour
{
    public GameObject PrefabGraphNode;
    public GameObject PrefabEdge;

    public Transform mMarkerGroup;

    private SampleGraph mGraph;

    void CreateGraphViz(Graph<GraphNodeData> graph)
    {
        for(int i = 0; i < graph.Nodes.Count; ++i)
        {
            GraphNodeData d = graph.Nodes[i].Value;
            Vector2 p = d.Point;
            GameObject obj = Instantiate(PrefabGraphNode, new Vector3(d.Point.x, d.Point.y, 0.0f), Quaternion.identity);
            obj.transform.SetParent(mMarkerGroup.transform);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        mGraph = SampleGraph.CreateSampleGraph();
        CreateGraphViz(mGraph);
        Camera.main.transform.position = new Vector3(mGraph.Extent.center.x, mGraph.Extent.center.y, -10.0f);
        Camera.main.orthographicSize = mGraph.Extent.width / 2.0f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
