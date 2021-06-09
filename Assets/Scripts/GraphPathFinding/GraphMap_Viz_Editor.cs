using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAI.PathFinding;

public class GraphMap_Viz_Editor : MonoBehaviour
{
    public string SceneName = "graphdemo";
    public int mY = 10;
    public int mX = 10;
    public float mSpacing = 1.0f;

    public GameObject PrefabGraphNode;

    public Transform ParentForGraphNodes;

    private SampleGraph mGraph = new SampleGraph();
    private Dictionary<GraphNodeData, GameObject> mGraphNodeGameObjDic = 
        new Dictionary<GraphNodeData, GameObject>();


    public LineFactory mLineFactory;

    enum ModeType
    {
        SELECTION,
        JOINING,
    }
    #region Private data
    private GameObject mSelectedGraphNode;
    //private GameObject mSelectedGraphNode2;
    //ModeType mMode = ModeType.SELECTION;
    Patterns.FiniteStateMachine mFsm = new Patterns.FiniteStateMachine();
    #endregion

    public class SelectionState : Patterns.State
    {
        public GraphMap_Viz_Editor mEditor;

        public SelectionState(GraphMap_Viz_Editor ed) : base()
        {
            mEditor = ed;
            ID = (int)ModeType.SELECTION;
            Name = "SelectionState";
        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void Update()
        {
            base.Update();
            if (Input.GetMouseButtonDown(0))
            {
                RayCast_SelectGraphNode();
            }
        }

        public void RayCast_SelectGraphNode()
        {
            Vector2 rayPos = new Vector2(
                Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
                Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
            RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0f);

            if (hit &&
                hit.transform.gameObject.GetComponent<GraphNode_Viz>() != null)
            {
                mEditor.SetSelectGraphNode(hit.transform.gameObject);
                mEditor.AddSelectedGameObjectToGraph();
                mEditor.mOnSelectedGraphNode?.Invoke(mEditor.mSelectedGraphNode.GetComponent<GraphNode_Viz>());
            }
            else
            {
                mEditor.SetUnSelectGraphNode();
            }
        }
    }

    public class ConnectingState : Patterns.State
    {
        public GraphMap_Viz_Editor mEditor;

        public ConnectingState(GraphMap_Viz_Editor ed) : base()
        {
            mEditor = ed;
            ID = (int)ModeType.JOINING;
            Name = "ConnectingState";
        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void Update()
        {
            base.Update();
            if (Input.GetMouseButtonDown(0))
            {
                RayCast_ConnectGraphNode();
            }
        }

        public void RayCast_ConnectGraphNode()
        {
            Vector2 rayPos = new Vector2(
                Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
                Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
            RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0f);

            if (hit &&
                hit.transform.gameObject.GetComponent<GraphNode_Viz>() != null 
                //&& hit.transform.gameObject != mSelectedGraphNode 
                //&& ObjectNotInNeighbour(mSelectedGraphNode, hit.transform.gameObject)
                )
            {
                //hit.transform.gameObject.GetComponent<GraphNode_Viz>().SetColor(Color.cyan);
                mEditor.ConnectGraph(hit.transform.gameObject);
            }
            else
            {
                mEditor.SetUnSelectGraphNode();
                mEditor.SetMode(ModeType.SELECTION);
            }
        }
    }

    void Start()
    {
        mGraph.mOnAddNode += OnAddNode;
        mGraph.mOnAddDirectedEdge += OnAddDirectedEdge;

        for(int i = 0; i < mX; ++i)
        {
            for(int j = 0; j < mY; ++j)
            {
                float x = i * mSpacing;
                float y = j * mSpacing;
                GameObject obj = Instantiate(PrefabGraphNode, new Vector3(x, y, 0.0f), Quaternion.identity);
                string name = "node_" + i.ToString() + "_" + j.ToString();
                obj.name = name;
                obj.GetComponent<ConstantScreenSizeForSprite>().Camera = Camera.main;

                if(ParentForGraphNodes != null)
                {
                    obj.transform.SetParent(ParentForGraphNodes);
                }

                GraphNodeData data = new GraphNodeData(obj.name, obj.transform.position.x, obj.transform.position.y);
                obj.GetComponent<GraphNode_Viz>().Data = data;
                mGraphNodeGameObjDic.Add(data, obj);
            }
        }

        AdjustCameraView();

        // set all delegates
        mOnSelectedGraphNode += OnSelectedGraphNode;
        mOnUnSelectGraphNode += OnUnSelectGraphNode;
        //mOnConnectGraphNodes += OnConnectGraphNodes;

        //
        mFsm.Add(new SelectionState(this));
        mFsm.Add(new ConnectingState(this));

        mFsm.SetCurrentState((int)ModeType.SELECTION);
    }

    public void AddSelectedGameObjectToGraph()
    {
        if (mSelectedGraphNode == null)
            return;

        GraphNodeData data = mSelectedGraphNode.GetComponent<GraphNode_Viz>().Data;

        if (mGraph.Contains(data))
            return;

        mGraph.AddNode(new GraphNode<GraphNodeData>(data));
    }

    public void OnAddNode(GraphNode<GraphNodeData> node)
    {
        GameObject obj = mGraphNodeGameObjDic[node.Value];
        obj.GetComponent<GraphNode_Viz>().Node = node;
        obj.GetComponent<GraphNode_Viz>().SetColor(Color.yellow);
    }
    void OnAddDirectedEdge(GraphNode<GraphNodeData> from, GraphNode<GraphNodeData> to)
    {
        GameObject a = mGraphNodeGameObjDic[from.Value];
        GameObject b = mGraphNodeGameObjDic[to.Value];

        b.GetComponent<GraphNode_Viz>().SetColor(Color.green);
        Line line = mLineFactory.GetLine(a.transform.position, b.transform.position, 0.1f, Color.cyan);
        a.GetComponent<GraphNode_Viz>().mLine = line;
    }

    public void AdjustCameraView()
    {
        //ParentForGraphNodes.
        //mGraph.CalculateExtent();
        //Camera.main.transform.position = new Vector3(mGraph.Extent.center.x, mGraph.Extent.center.y, -10.0f);
        //Camera.main.orthographicSize = 1.2f * mGraph.Extent.height / 2.0f;
    }

    // Update is called once per frame
    void Update()
    {
        mFsm.Update();
    }

    void SetMode(ModeType type)
    {
        mFsm.SetCurrentState((int)type);
    }

    public bool ObjectNotInNeighbour(GameObject parent, GameObject child)
    {
        GraphNode_Viz p = parent.GetComponent<GraphNode_Viz>();
        GraphNode_Viz c = parent.GetComponent<GraphNode_Viz>();

        for(int i = 0; i < p.Node.Neighbors.Count; ++i)
        {
            if (c.Node.Value.Equals(p.Node.Neighbors[i].Value))
                return false;
        }
        return true;
    }

    public void ConnectGraph(GameObject b)
    {
        GraphNode_Viz bViz = b.GetComponent<GraphNode_Viz>();
        // check if b is a graphnode if not then change the state to selection.
        if(mGraph.Contains(bViz.Data))
        {
            // Show visual connection.
            GraphNode_Viz aViz = mSelectedGraphNode.GetComponent<GraphNode_Viz>();

            // Add the edge to graph.
            mGraph.AddUndirectedEdge(aViz.Node, bViz.Node, GraphNodeData.Distance(aViz.Node.Value, bViz.Node.Value));
        }
        else
        {
            SetMode(ModeType.SELECTION);
        }
    }

    public void SetSelectGraphNode(GameObject obj)
    {
        if (mSelectedGraphNode != null)
        {
            mOnUnSelectGraphNode?.Invoke(mSelectedGraphNode);
        }
        mSelectedGraphNode = obj;

        // check if the selected node is a already in graph.
        if (mGraph.Contains(obj.GetComponent<GraphNode_Viz>().Data))
        {
            // change the mode to JOINING.
            mFsm.SetCurrentState((int)ModeType.JOINING);
        }

        //mOnSelectedGraphNode?.Invoke(obj.GetComponent<GraphNode_Viz>());
    }

    public void SetUnSelectGraphNode()
    {
        if (mSelectedGraphNode != null)
        {
            mOnUnSelectGraphNode?.Invoke(mSelectedGraphNode);
        }
        mSelectedGraphNode = null;
    }

    #region Delegates
    public delegate void DelegateWithGraphNode(GameObject obj);
    public delegate void DelegateWithGraphNode_Viz(GraphNode_Viz viz);
    public delegate void DelegateWithTwoGraphNode(GameObject a, GameObject b);
    public DelegateWithGraphNode_Viz mOnSelectedGraphNode;
    public DelegateWithGraphNode mOnUnSelectGraphNode;
    public DelegateWithTwoGraphNode mOnConnectGraphNodes;
    #endregion

    #region Delegates Implementation
    void OnSelectedGraphNode(GraphNode_Viz viz)
    {
        //if (obj == null) return;
        // viz = obj.GetComponent<GraphNode_Viz>();
        if (viz == null) return;

        viz.SetColor(Color.red);
        // Show neighbours colors.
        if (mGraph.Contains(viz.Data))
        {
            for (int i = 0; i < viz.Node.Neighbors.Count; ++i)
            {
                GameObject neighbourObj = mGraphNodeGameObjDic[viz.Node.Neighbors[i].Value];
                if (neighbourObj != null)
                {
                    neighbourObj.GetComponent<GraphNode_Viz>().SetColor(Color.green);
                }
            }
        }
    }
    void OnUnSelectGraphNode(GameObject obj)
    {
        if (obj == null) return;
        GraphNode_Viz viz = obj.GetComponent<GraphNode_Viz>();
        if (viz)
        {
            viz.UnSetColor();
            // Reset neighbours colors.

            if (mGraph.Contains(viz.Data))
            {
                for (int i = 0; i < viz.Node.Neighbors.Count; ++i)
                {
                    GameObject neighbourObj = mGraphNodeGameObjDic[viz.Node.Neighbors[i].Value];
                    if (neighbourObj != null)
                    {
                        neighbourObj.GetComponent<GraphNode_Viz>().UnSetColor();
                    }
                }
            }
        }
    }

    public void ClearGraph()
    {
        for(int i = 0; i < mGraph.Nodes.Count; ++i)
        {
            if(mGraph.Nodes[i].Neighbors != null)
                mGraph.Nodes[i].Neighbors.Clear();
            ((GraphNode<GraphNodeData>)(mGraph.Nodes[i])).Costs.Clear();
        }
        mGraph.Nodes.Clear();

        foreach(GameObject obj in mGraphNodeGameObjDic.Values)
        {
            GraphNode_Viz viz = obj.GetComponent<GraphNode_Viz>();
            viz.ResetColor();
            viz.Node = null;
        }

        mLineFactory.SetAllInActive();
    }

    public void SaveGraph()
    {
        SampleGraph.Save(mGraph, SceneName);
    }

    public void LoadGraph()
    {
        ClearGraph();
        mGraph = new SampleGraph();
        mGraph.mOnAddNode += OnAddNode;
        mGraph.mOnAddDirectedEdge += OnAddDirectedEdge;
        SampleGraph.Load(mGraph, SceneName);

    }

    #endregion
}
