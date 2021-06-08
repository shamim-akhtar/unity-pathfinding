using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAI.PathFinding;

public class GraphMap_Viz_Editor : MonoBehaviour
{
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
                mEditor.RayCast_SelectGraphNode();
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
                mEditor.RayCast_ConnectGraphNode();
            }
        }
    }

    void Start()
    {
        for(int i = 0; i < mX; ++i)
        {
            for(int j = 0; j < mY; ++j)
            {
                float x = i * mSpacing;
                float y = j * mSpacing;
                GameObject obj = Instantiate(PrefabGraphNode, new Vector3(x, y, 0.0f), Quaternion.identity);
                string name = "node_" + i.ToString() + "_" + j.ToString();
                GraphNodeData data = new GraphNodeData(name, x, y);
                GraphNode<GraphNodeData> node = new GraphNode<GraphNodeData>(data);
                mGraph.AddNode(node);

                // Keep a reference of the graph node in the graph node viz.
                obj.GetComponent<GraphNode_Viz>().Node = node;
                obj.GetComponent<ConstantScreenSizeForSprite>().Camera = Camera.main;

                if(ParentForGraphNodes != null)
                {
                    obj.transform.SetParent(ParentForGraphNodes);
                }

                mGraphNodeGameObjDic.Add(data, obj);
            }
        }

        mGraph.CalculateExtent();

        Camera.main.transform.position = new Vector3(mGraph.Extent.center.x, mGraph.Extent.center.y, -10.0f);
        Camera.main.orthographicSize = 1.2f * mGraph.Extent.height / 2.0f;

        // set all delegates
        mOnSelectedGraphNode += OnSelectedGraphNode;
        mOnUnSelectGraphNode += OnUnSelectGraphNode;
        mOnConnectGraphNodes += OnConnectGraphNodes;

        //
        mFsm.Add(new SelectionState(this));
        mFsm.Add(new ConnectingState(this));

        mFsm.SetCurrentState((int)ModeType.SELECTION);
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

    public void RayCast_SelectGraphNode()
    {
        Vector2 rayPos = new Vector2(
            Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
            Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0f);

        if (hit && 
            hit.transform.gameObject != mSelectedGraphNode &&
            hit.transform.gameObject.GetComponent<GraphNode_Viz>() != null)
        {
            hit.transform.gameObject.GetComponent<GraphNode_Viz>().SetColor(Color.cyan);
            SetSelectGraphNode(hit.transform.gameObject);
            SetMode(ModeType.JOINING);
        }
        else
        {
            SetUnSelectGraphNode(mSelectedGraphNode);
            SetMode(ModeType.SELECTION);
        }
    }

    public void RayCast_ConnectGraphNode()
    {
        Vector2 rayPos = new Vector2(
            Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
            Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0f);

        if (hit && 
            hit.transform.gameObject.GetComponent< GraphNode_Viz>() != null &&
            hit.transform.gameObject != mSelectedGraphNode &&
            ObjectNotInNeighbour(mSelectedGraphNode, hit.transform.gameObject))
        {
            hit.transform.gameObject.GetComponent<GraphNode_Viz>().SetColor(Color.cyan);
            ConnectGraphNodes(mSelectedGraphNode, hit.transform.gameObject);
        }
        else
        {
            SetUnSelectGraphNode(mSelectedGraphNode);
            SetMode(ModeType.SELECTION);
        }
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

    public void ConnectGraphNodes(GameObject a, GameObject b)
    {
        // Show visual connection.
        GraphNode_Viz aViz = a.GetComponent<GraphNode_Viz>();
        GraphNode_Viz bViz = b.GetComponent<GraphNode_Viz>();

        //aViz.SetColor(Color.blue);
        bViz.SetColor(Color.green);

        // Add the edge to graph.
        mGraph.AddUndirectedEdge(aViz.Node, bViz.Node, GraphNodeData.Distance(aViz.Node.Value, bViz.Node.Value));
        //mGraph.AddDirectedEdge(aViz.Node, bViz.Node, GraphNodeData.Distance(aViz.Node.Value, bViz.Node.Value));
        mOnConnectGraphNodes?.Invoke(a, b);
    }

    public void SetSelectGraphNode(GameObject obj)
    {
        if (mSelectedGraphNode != null)
        {
            mOnUnSelectGraphNode?.Invoke(mSelectedGraphNode);
        }
        mSelectedGraphNode = obj;
        mOnSelectedGraphNode?.Invoke(obj);

    }

    public void SetUnSelectGraphNode(GameObject obj)
    {
        if (mSelectedGraphNode != null)
        {
            mOnUnSelectGraphNode?.Invoke(mSelectedGraphNode);
        }
        mSelectedGraphNode = null;
    }

    #region Delegates
    public delegate void DelegateWithGraphNode(GameObject obj);
    public delegate void DelegateWithTwoGraphNode(GameObject a, GameObject b);
    public DelegateWithGraphNode mOnSelectedGraphNode;
    public DelegateWithGraphNode mOnUnSelectGraphNode;
    public DelegateWithTwoGraphNode mOnConnectGraphNodes;
    #endregion

    #region Delegates Implementation
    void OnSelectedGraphNode(GameObject obj)
    {
        if (obj == null) return;
        GraphNode_Viz viz = obj.GetComponent<GraphNode_Viz>();
        if(viz)
        {
            viz.SetColor(Color.red);
            // Show neighbours colors.
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
            for(int i = 0; i < viz.Node.Neighbors.Count; ++i)
            {
                GameObject neighbourObj = mGraphNodeGameObjDic[viz.Node.Neighbors[i].Value];
                if(neighbourObj != null)
                {
                    neighbourObj.GetComponent<GraphNode_Viz>().UnSetColor();
                }
            }
        }
    }
    void OnConnectGraphNodes(GameObject a, GameObject b)
    {
        Line line = mLineFactory.GetLine(a.transform.position, b.transform.position, 0.1f, Color.black);
    }
    #endregion
}
