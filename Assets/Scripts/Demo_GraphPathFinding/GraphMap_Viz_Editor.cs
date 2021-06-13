using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameAI.PathFinding;
using UnityEngine.SceneManagement;
using Lean.Gui;
using UnityEngine.EventSystems;

public class GraphMap_Viz_Editor : MonoBehaviour
{
    public string SceneName = "graphdemo";
    public int mY = 10;
    public int mX = 10;
    public float mSpacing = 1.0f;

    public LayerMask MapMask;
    public LayerMask GraphNodeMask;
    public FixedTouchField TouchPad;

    public GameObject PrefabGraphNode;

    public Transform ParentForGraphNodes;

    private SampleGraph mGraph = new SampleGraph();
    private Dictionary<GraphNodeData, GameObject> mGraphNodeGameObjDic = 
        new Dictionary<GraphNodeData, GameObject>();

    //public Text mTextToggleCameraMode;
    //public Text mTextToggleAddControlPts;
    //public LeanToggle mToggleCameraMode;
    //public LeanToggle mToggleAddControlPts;
    public LeanSwitch mLeanSwitchMode;
    public Text mTextMode;

    public CameraManiipulator2D mCameraManip;

    enum ModeType
    {
        CAMERA_MODE,
        GRAPHNODE_ADD,
        GRAPHNODE_SELECTION,
        GRAPHNODE_JOINING,
    }
    #region Private data
    private GameObject mSelectedGraphNode;
    Patterns.FiniteStateMachine mFsm = new Patterns.FiniteStateMachine();
    #endregion

    public class CameraModeState : Patterns.State
    {
        public GraphMap_Viz_Editor mEditor;

        public CameraModeState(GraphMap_Viz_Editor ed) : base()
        {
            mEditor = ed;
            ID = (int)ModeType.CAMERA_MODE;
            Name = "CameraModeState";
        }

        public override void Enter()
        {
            base.Enter();
            SetCameraMovementMode(true);
            mEditor.mTextMode.text = "Camera Mode";
        }

        public override void Exit()
        {
            base.Exit();
            SetCameraMovementMode(false);
        }

        void SetCameraMovementMode(bool flag)
        {
            if (flag)
            {
                mEditor.mCameraManip.enabled = true;
                mEditor.TouchPad.gameObject.SetActive(true);
            }
            else
            {
                mEditor.mCameraManip.enabled = false;
                mEditor.TouchPad.gameObject.SetActive(false);
            }
        }
    }

    public class GraphNodeAddState : Patterns.State
    {
        public GraphMap_Viz_Editor mEditor;

        public GraphNodeAddState(GraphMap_Viz_Editor ed) : base()
        {
            mEditor = ed;
            ID = (int)ModeType.GRAPHNODE_ADD;
            Name = "GraphNodeAddState";
        }

        public override void Enter()
        {
            base.Enter();
            mEditor.mTextMode.text = "Add Graph Points";
        }

        public override void Update()
        {
            base.Update();
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }
                RayCast_AddControlPoint();
            }
        }

        public void RayCast_AddControlPoint()
        {
            Vector2 rayPos = new Vector2(
                Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
                Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
            RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0f, mEditor.MapMask);

            if (hit)
            {
                mEditor.CreateGraphNodeAndAddToGraph(hit.point);
            }
        }
    }

    public class GraphNodeSelectionState : Patterns.State
    {
        public GraphMap_Viz_Editor mEditor;

        public GraphNodeSelectionState(GraphMap_Viz_Editor ed) : base()
        {
            mEditor = ed;
            ID = (int)ModeType.GRAPHNODE_SELECTION;
            Name = "GraphNodeSelectionState";
        }

        public override void Enter()
        {
            base.Enter();
            mEditor.mTextMode.text = "Topology";
        }

        public override void Update()
        {
            base.Update();
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }
                RayCast_SelectGraphNode();
            }
        }

        public void RayCast_SelectGraphNode()
        {
            Vector2 rayPos = new Vector2(
                Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
                Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
            RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0f, mEditor.GraphNodeMask);

            if (hit &&
                hit.transform.gameObject.GetComponent<GraphNode_Viz>() != null)
            {
                mEditor.SetSelectGraphNode(hit.transform.gameObject);
                mEditor.mOnSelectedGraphNode?.Invoke(mEditor.mSelectedGraphNode.GetComponent<GraphNode_Viz>());
                mEditor.mFsm.SetCurrentState((int)ModeType.GRAPHNODE_JOINING);
            }
            else
            {
                mEditor.SetUnSelectGraphNode();
            }
        }
    }

    public class GraphNodeJoiningState : Patterns.State
    {
        public GraphMap_Viz_Editor mEditor;

        public GraphNodeJoiningState(GraphMap_Viz_Editor ed) : base()
        {
            mEditor = ed;
            ID = (int)ModeType.GRAPHNODE_JOINING;
            Name = "GraphNodeJoiningState";
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
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }
                RayCast_ConnectGraphNode();
            }
        }

        public void RayCast_ConnectGraphNode()
        {
            Vector2 rayPos = new Vector2(
                Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
                Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
            RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0f, mEditor.GraphNodeMask);

            if (hit && hit.transform.gameObject.GetComponent<GraphNode_Viz>() != null)
            {
                mEditor.ConnectGraph(hit.transform.gameObject);
            }
            else
            {
                mEditor.SetUnSelectGraphNode();
                mEditor.mFsm.SetCurrentState((int)ModeType.GRAPHNODE_SELECTION);
            }
        }
    }

    void Start()
    {
        mGraph.mOnAddNode += OnAddNode;
        mGraph.mOnAddDirectedEdge += OnAddDirectedEdge;

        // set all delegates
        mOnSelectedGraphNode += OnSelectedGraphNode;
        mOnUnSelectGraphNode += OnUnSelectGraphNode;
        //mOnConnectGraphNodes += OnConnectGraphNodes;

        //
        mFsm.Add(new CameraModeState(this));
        mFsm.Add(new GraphNodeSelectionState(this));
        mFsm.Add(new GraphNodeJoiningState(this));
        mFsm.Add(new GraphNodeAddState(this));

        mFsm.SetCurrentState((int)ModeType.CAMERA_MODE);
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

        a.GetComponent<GraphNode_Viz>().ShowNeighbourLines(true);
    }

    // Update is called once per frame
    void Update()
    {
        mFsm.Update();
    }

    public void CreateGraphNodeAndAddToGraph(Vector3 position)
    {
        GameObject obj = Instantiate(PrefabGraphNode, position, Quaternion.identity);
        string name = "node_" + position.x.ToString() + "_" + position.y.ToString();
        obj.name = name;
        obj.GetComponent<ConstantScreenSizeForSprite>().Camera = Camera.main;

        if (ParentForGraphNodes != null)
        {
            obj.transform.SetParent(ParentForGraphNodes);
        }

        GraphNodeData data = new GraphNodeData(obj.name, obj.transform.position.x, obj.transform.position.y);

        GraphNode_Viz viz = obj.GetComponent<GraphNode_Viz>();
        viz.Data = data;
        viz.SetColor(viz.DEFAULT_COLOR);
        viz.mOriginalCameraSize = Camera.main.orthographicSize;
        mGraphNodeGameObjDic.Add(data, obj);

        AddGameObjectToGraph(obj);
    }

    public bool ObjectNotInNeighbour(GameObject parent, GameObject child)
    {
        GraphNode_Viz p = parent.GetComponent<GraphNode_Viz>();
        GraphNode_Viz c = parent.GetComponent<GraphNode_Viz>();

        for(int i = 0; i < p.Node.Neighbours.Count; ++i)
        {
            if (c.Node.Value.Equals(p.Node.Neighbours[i].Value))
                return false;
        }
        return true;
    }

    public void SetSelectGraphNode(GameObject obj)
    {
        if (mSelectedGraphNode != null)
        {
            mOnUnSelectGraphNode?.Invoke(mSelectedGraphNode);
        }
        mSelectedGraphNode = obj;

        if (mGraph.Contains(obj.GetComponent<GraphNode_Viz>().Data))
        {
            mFsm.SetCurrentState((int)ModeType.GRAPHNODE_JOINING);
        }
    }

    public void AddSelectedGameObjectToGraph()
    {
        AddGameObjectToGraph(mSelectedGraphNode);
    }

    public void AddGameObjectToGraph(GameObject obj)
    {
        if (obj == null)
            return;

        GraphNodeData data = obj.GetComponent<GraphNode_Viz>().Data;

        if (mGraph.Contains(data))
            return;

        mGraph.AddNode(new GraphNode<GraphNodeData>(data));
    }

    public void ConnectGraph(GameObject b)
    {
        GraphNode_Viz bViz = b.GetComponent<GraphNode_Viz>();
        // check if b is a graphnode if not then change the state to selection.
        if(mGraph.Contains(bViz.Data))
        {
            GraphNode_Viz aViz = mSelectedGraphNode.GetComponent<GraphNode_Viz>();

            // Add the edge to graph.
            mGraph.AddDirectedEdge(aViz.Node, bViz.Node, GraphNodeData.Distance(aViz.Node.Value, bViz.Node.Value));

            SetSelectGraphNode(b);
            mOnSelectedGraphNode?.Invoke(mSelectedGraphNode.GetComponent<GraphNode_Viz>());
        }
        else
        {
            AddGameObjectToGraph(b);

            // add as a connection.

            GraphNode_Viz aViz = mSelectedGraphNode.GetComponent<GraphNode_Viz>();
            mGraph.AddDirectedEdge(aViz.Node, bViz.Node, GraphNodeData.Distance(aViz.Node.Value, bViz.Node.Value));

            SetSelectGraphNode(b);
            mOnSelectedGraphNode?.Invoke(mSelectedGraphNode.GetComponent<GraphNode_Viz>());
            mFsm.SetCurrentState((int)ModeType.GRAPHNODE_JOINING);
        }
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
        if (viz == null) return;

        viz.SetColor(Color.red);
        if (mGraph.Contains(viz.Data))
        {
            for (int i = 0; i < viz.Node.Neighbours.Count; ++i)
            {
                GameObject neighbourObj = mGraphNodeGameObjDic[viz.Node.Neighbours[i].Value];
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
            viz.SetColor(Color.yellow);
            if (mGraph.Contains(viz.Data))
            {
                for (int i = 0; i < viz.Node.Neighbours.Count; ++i)
                {
                    GameObject neighbourObj = mGraphNodeGameObjDic[viz.Node.Neighbours[i].Value];
                    if (neighbourObj != null)
                    {
                        //neighbourObj.GetComponent<GraphNode_Viz>().UnSetColor();

                        // we set to yellow because these nodes are already added to the graph.
                        neighbourObj.GetComponent<GraphNode_Viz>().SetColor(Color.yellow);
                    }
                }
            }
        }
    }

    public void ClearGraph()
    {
        for(int i = 0; i < mGraph.Nodes.Count; ++i)
        {
            if(mGraph.Nodes[i].Neighbours != null)
                mGraph.Nodes[i].Neighbours.Clear();
            ((GraphNode<GraphNodeData>)(mGraph.Nodes[i])).Costs.Clear();
        }
        mGraph.Nodes.Clear();

        foreach(GameObject obj in mGraphNodeGameObjDic.Values)
        {
            GraphNode_Viz viz = obj.GetComponent<GraphNode_Viz>();
            viz.ResetColor();
            viz.Node = null;
            Destroy(obj);
        }
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

    #region UI Implementation
    public void LoadGraphPlayMode()
    {
        SceneManager.LoadScene("Demo_GraphPathFinding_Play");
    }

    public void OnSelectMode()
    {
        int mode = mLeanSwitchMode.State;
        mFsm.SetCurrentState(mode);
    }

    #endregion
}
