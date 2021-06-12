using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAI.PathFinding;

public class GraphMap_Viz_Play : MonoBehaviour
{
    public string SceneName = "graphdemo";
    public GameObject PrefabGraphNode;

    public Transform ParentForGraphNodes;

    //public LineFactory mLineFactory;
    public GameObject PrefabNPC;
    private GraphPathFinder_Viz mGraphPathFinder_Viz;

    private Dictionary<GraphNodeData, GameObject> mGraphNodeGameObjDic =
        new Dictionary<GraphNodeData, GameObject>();

    public SampleGraph mGraph = new SampleGraph();

    private void Start()
    {
        mGraph.mOnAddNode += OnAddNode;
        mGraph.mOnAddDirectedEdge += OnAddDirectedEdge;

        // create the dummy NPC

        GameObject npc = Instantiate(PrefabNPC);
        mGraphPathFinder_Viz = npc.AddComponent<GraphPathFinder_Viz>();
        mGraphPathFinder_Viz.mGraphMap_Viz_Play = this;

        LoadGraph();
    }

    private void LoadGraph()
    {
        SampleGraph.Load(mGraph, SceneName);
        SetRandomStartPoint();
        mGraphPathFinder_Viz.transform.position = new Vector3(
            mGraphPathFinder_Viz.StartNode.Value.Point.x,
            mGraphPathFinder_Viz.StartNode.Value.Point.y,
            0.0f);
    }

    private void SetRandomStartPoint()
    {
        int randIndex = Random.Range(0, mGraph.Nodes.Count);
        mGraphPathFinder_Viz.StartNode = mGraph.Nodes[randIndex];
    }

    public void OnAddNode(GraphNode<GraphNodeData> node)
    {
        GameObject obj = Instantiate(
            PrefabGraphNode, 
            new Vector3(node.Value.Point.x, node.Value.Point.y, 0.0f), 
            Quaternion.identity);

        obj.name = node.Value.Name;
        obj.GetComponent<ConstantScreenSizeForSprite>().Camera = Camera.main;

        if (ParentForGraphNodes != null)
        {
            obj.transform.SetParent(ParentForGraphNodes);
        }

        GraphNode_Viz viz = obj.GetComponent<GraphNode_Viz>();
        viz.Data = node.Value;
        viz.Node = node;
        Color c = Color.cyan;
        c.a = 0.2f;
        viz.SetColor(c);
        viz.mOriginalCameraSize = Camera.main.orthographicSize;
        mGraphNodeGameObjDic.Add(node.Value, obj);
    }

    void OnAddDirectedEdge(GraphNode<GraphNodeData> from, GraphNode<GraphNodeData> to)
    {
        GameObject a = mGraphNodeGameObjDic[from.Value];
        GameObject b = mGraphNodeGameObjDic[to.Value];

        a.GetComponent<GraphNode_Viz>().ShowNeighbourLines(true);
    }

    public void RayCast_SetGoal()
    {
        Vector2 rayPos = new Vector2(
            Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
            Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0f);

        if (hit && hit.transform.gameObject.GetComponent<GraphNode_Viz>() != null)
        {
            GraphNode_Viz viz = hit.transform.gameObject.GetComponent<GraphNode_Viz>();
            mGraphPathFinder_Viz.SetGoal(viz.Node);
            mGraphPathFinder_Viz.FindPath_Play();
        }
    }

    private void Update()
    {
        //We are in the player mPathFindingMode. So left mouse button click should
        //relocate the goal point.
        if (Input.GetMouseButtonDown(0))
        {
            RayCast_SetGoal();
        }
    }
}
