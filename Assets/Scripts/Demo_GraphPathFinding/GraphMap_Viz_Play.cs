using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GameAI.PathFinding;
using Lean.Gui;

public class GraphMap_Viz_Play : MonoBehaviour
{
    public string SceneName = "graphdemo";
    public GameObject PrefabGraphNode;

    public Transform ParentForGraphNodes;
    public Transform GoalGrameObject;

    //public GameObject PrefabNPC;
    public GraphPathFinder_Viz mGraphPathFinder_Viz;

    private Dictionary<GraphNodeData, GameObject> mGraphNodeGameObjDic =
        new Dictionary<GraphNodeData, GameObject>();

    public SampleGraph mGraph = new SampleGraph();
    public Color COLOR_OPEN_LIST = new Color(0.0f, 0.0f, 1.0f, 0.3f);
    public Color COLOR_CLOSED_LIST = new Color(0.0f, 0.0f, 0.0f, 0.3f);
    public Color COLOR_CURRENT_NODE = new Color(1.0f, 0.0f, 0.0f, 0.3f);
    public Color COLOR_SOLUTION = new Color(0.0f, 1.0f, 1.0f, 0.7f);

    #region UI Elements
    public Text mModeTypeText;
    public Text mInteractiveText;
    public Text mAlgorithmText;
    public Button mPlayButton;
    public Button mStepButton;
    public Button mResetButton;
    public LeanSwitch mLeanSwitchAlgo;

    public GameObject mCostPanel;
    public Text mFCostText;
    public Text mGCostText;
    public Text mHCostText;

    public GameObject mToggleInteractive;
    public LeanToggle mToggleCostFunction;
    public Text mCostFunctionText;
    enum CostFunctionType
    {
        MANHATTAN,
        EUCLIDEN,
    }
    CostFunctionType mCostFunctionType = CostFunctionType.MANHATTAN;
    enum PathFindingMode
    {
        GAMEPLAY_MODE,
        INTERACTIVE_MODE,
    }
    PathFindingMode mPathFindingMode = PathFindingMode.GAMEPLAY_MODE;

    int mPathFindingAlgo = 0; // Astar, 1 = Djikstra and 2 = Greedy best-first

    #endregion

    private void Start()
    {
        mGraph.mOnAddNode += OnAddNode;
        mGraph.mOnAddDirectedEdge += OnAddDirectedEdge;
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
        GoalGrameObject.position = mGraphPathFinder_Viz.transform.position;
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
            GoalGrameObject.transform.position = hit.transform.position;

            if (mPathFindingMode == PathFindingMode.GAMEPLAY_MODE)
            {
                mGraphPathFinder_Viz.FindPath_Play();
            }
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

    #region UI Functions


    public void SetToggleCostFunction()
    {
        if (mGraphPathFinder_Viz.mPathFinder == null)
            return;
        if (mGraphPathFinder_Viz.mPathFinder != null && mGraphPathFinder_Viz.mPathFinder.Status == PathFinderStatus.RUNNING)
        {
            // disable selection when running.
            if (mToggleCostFunction.On)
                mToggleCostFunction.TurnOff();
            else
                mToggleCostFunction.TurnOn();
            return;
        }

        if (mCostFunctionType == CostFunctionType.MANHATTAN)
        {
            mCostFunctionType = CostFunctionType.EUCLIDEN;
            mCostFunctionText.text = "Euclidean Cost";
        }
        else
        {
            mCostFunctionType = CostFunctionType.MANHATTAN;
            mCostFunctionText.text = "Manhattan Cost";
        }
        SetCostFunction(mCostFunctionType);
    }
    void SetCostFunction(CostFunctionType cf)
    {
        switch (cf)
        {
            case CostFunctionType.MANHATTAN:
                {
                    mGraphPathFinder_Viz.mPathFinder.HeuristicCost = SampleGraph.GetManhattanCost;
                    break;
                }
            case CostFunctionType.EUCLIDEN:
                {
                    mGraphPathFinder_Viz.mPathFinder.HeuristicCost = SampleGraph.GetEuclideanCost;
                    break;
                }
        }
    }

    public void SetToggleInteractive()
    {
        if (mPathFindingMode == PathFindingMode.GAMEPLAY_MODE)
        {
            SetPathFindingMode(PathFindingMode.INTERACTIVE_MODE);
        }
        else
        {
            SetPathFindingMode(PathFindingMode.GAMEPLAY_MODE);
        }
    }

    void SetPathFindingMode(PathFindingMode m)
    {
        mPathFindingMode = m;
        if (mPathFindingMode == PathFindingMode.GAMEPLAY_MODE)
        {
            mPlayButton.gameObject.SetActive(false);
            mStepButton.gameObject.SetActive(false);
            mResetButton.gameObject.SetActive(false);
        }
        else
        {
            mPlayButton.gameObject.SetActive(true);
            mStepButton.gameObject.SetActive(true);
            mResetButton.gameObject.SetActive(true);
        }
    }

    public void OnPlayPathFinding()
    {
        mGraphPathFinder_Viz.FindPath_Play();
    }

    public void OnStepPathFinding()
    {
        mGraphPathFinder_Viz.FindPath_Step();
    }

    public void OnResetPathFinding()
    {
        mFCostText.text = "";
        mGCostText.text = "";
        mHCostText.text = "";
    }

    public void OnSelectAlgorithm()
    {
        if (mGraphPathFinder_Viz.mPathFinder != null && mGraphPathFinder_Viz.mPathFinder.Status == PathFinderStatus.RUNNING)
        {
            // disable selection when running.
            mLeanSwitchAlgo.State = (int)mPathFindingAlgo;
            return;
        }
        mPathFindingAlgo = mLeanSwitchAlgo.State;
        if (mPathFindingAlgo == 0)
        {
            mAlgorithmText.text = "Astar";
            mAlgorithmText.alignment = TextAnchor.MiddleLeft;
        }
        if (mPathFindingAlgo == 1)
        {
            mAlgorithmText.text = "Dijkstra";
            mAlgorithmText.alignment = TextAnchor.MiddleCenter;
        }
        if (mPathFindingAlgo == 2)
        {
            mAlgorithmText.text = "Greedy Best-First";
            mAlgorithmText.alignment = TextAnchor.MiddleRight;
        }
        mGraphPathFinder_Viz.SetPathFindingAlgorithm((PathFindingAlgorithm)mPathFindingAlgo);
        SetCostFunction(mCostFunctionType);
    }

    public void SetToggle()
    {
        SceneManager.LoadScene("Demo_GraphPathFinding_Editor");
    }
    #endregion

    #region Delegates for Interactive Pathfinding.

    private void Update_GraphNode_Viz(PathFinder<GraphNodeData>.PathFinderNode node, Color color)
    {
        GameObject obj = mGraphNodeGameObjDic[node.Location.Value];
        GraphNode_Viz cellScript = obj.GetComponent<GraphNode_Viz>();

        if (cellScript)
        {
            cellScript.SetColor(color);
            //cellScript.SetHCost(node.Hcost);
            //cellScript.SetGCost(node.GCost);
            //cellScript.SetFCost(node.Fcost);
        }
    }

    public void OnChangeCurrentNode(PathFinder<GraphNodeData>.PathFinderNode node)
    {
        mFCostText.text = node.Fcost.ToString("F2");
        mGCostText.text = node.GCost.ToString("F2");
        mHCostText.text = node.Hcost.ToString("F2");
        Update_GraphNode_Viz(node, COLOR_CURRENT_NODE);
    }

    public void OnAddToOpenList(PathFinder<GraphNodeData>.PathFinderNode node)
    {
        Update_GraphNode_Viz(node, COLOR_OPEN_LIST);
    }

    public void OnAddToClosedList(PathFinder<GraphNodeData>.PathFinderNode node)
    {
        Update_GraphNode_Viz(node, COLOR_CLOSED_LIST);
    }

    public void OnDestinationFound(PathFinder<GraphNodeData>.PathFinderNode node)
    {
        List<PathFinder<GraphNodeData>.PathFinderNode> path = new List<PathFinder<GraphNodeData>.PathFinderNode>();
        GameObject obj = mGraphNodeGameObjDic[node.Location.Value];

        PathFinder<GraphNodeData>.PathFinderNode n = node;
        while (n != null)
        {
            path.Add(n);
            n = n.Parent;
        }

        for (int i = path.Count - 1; i >= 0; i = i - 1)
        {
            n = path[i];
            obj = mGraphNodeGameObjDic[n.Location.Value];
            RectGridCell_Viz cellScript = obj.GetComponent<RectGridCell_Viz>();

            if (cellScript)
            {
                cellScript.SetInnerColor(COLOR_SOLUTION);
            }
        }
    }
    #endregion
}
