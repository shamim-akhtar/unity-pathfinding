using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Patterns;
using Lean.Gui;
using GameAI.PathFinding;

public class PathFinderDemo : MonoBehaviour
{
    enum ModeType
    {
        EDITOR,
        PLAYER,
    }
    enum PathFindingMode
    {
        GAMEPLAY_MODE,
        INTERACTIVE_MODE,
    }
    PathFindingMode mPathFindingMode = PathFindingMode.GAMEPLAY_MODE;

    #region Public variables exposed to Unity Editor
    public Text mModeTypeText;
    public Text mInteractiveText;
    public Text mAlgorithmText;
    public Button mClearGridButton;
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

    private PathFinder<RectGridCell>.CostFunction mCostFunction;
    enum CostFunctionType
    {
        MANHATTAN,
        EUCLIDEN,
    }
    CostFunctionType mCostFunctionType = CostFunctionType.MANHATTAN;

    public RectGridMap_Viz mRectGridMap_Vis;
    public PathFinder_Viz mPathFinder_Viz;
    #endregion

    private FiniteStateMachine mFsm = new FiniteStateMachine();
    int mPathFindingAlgo = 0; // Astar, 1 = Djikstra and 2 = Greedy best-first

    void Start()
    {
        mFsm.Add(new State((int)ModeType.EDITOR, OnEnterEditor, OnExitEditor, OnUpdateEditor));
        mFsm.Add(new State((int)ModeType.PLAYER, OnEnterPlayer, OnExitPlayer, OnUpdatePlayer));
        mFsm.SetCurrentState((int)ModeType.PLAYER);
    }

    void Update()
    {
        mFsm.Update();
    }

    public void SetToggle()
    {
        if(mFsm.GetCurrentState().ID == (int)ModeType.EDITOR)
        {
            mFsm.SetCurrentState((int)ModeType.PLAYER);
        }
        else
        {
            mFsm.SetCurrentState((int)ModeType.EDITOR);
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

    public void SetToggleCostFunction()
    {
        if (mPathFinder_Viz.mPathFinder == null)
            return;
        if (mPathFinder_Viz.mPathFinder != null && mPathFinder_Viz.mPathFinder.Status == PathFinderStatus.RUNNING)
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
                    mPathFinder_Viz.mPathFinder.HCostFunction = RectGridMap.GetManhattanCost;
                    break;
                }
            case CostFunctionType.EUCLIDEN:
                {
                    mPathFinder_Viz.mPathFinder.HCostFunction = RectGridMap.GetEuclideanCost;
                    break;
                }
        }
    }

    public void OnPlayPathFinding()
    {
        mPathFinder_Viz.FindPath_Play();
    }

    public void OnStepPathFinding()
    {
        mPathFinder_Viz.FindPath_Step();
    }

    public void OnResetPathFinding()
    {
        mFCostText.text = "";
        mGCostText.text = "";
        mHCostText.text = "";
        mRectGridMap_Vis.ResetPathFindingInfo();
    }

    public void OnSelectAlgorithm()
    {
        if(mPathFinder_Viz.mPathFinder != null && mPathFinder_Viz.mPathFinder.Status == PathFinderStatus.RUNNING)
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
        if(mPathFindingAlgo == 1)
        {
            mAlgorithmText.text = "Dijkstra";
            mAlgorithmText.alignment = TextAnchor.MiddleCenter;
        }
        if (mPathFindingAlgo == 2)
        {
            mAlgorithmText.text = "Greedy Best-First";
            mAlgorithmText.alignment = TextAnchor.MiddleRight;
        }
        mPathFinder_Viz.SetPathFindingAlgorithm((PathFindingAlgorithm)mPathFindingAlgo);
        SetCostFunction(mCostFunctionType);
        mPathFinder_Viz.mPathFinder.onFailure += OnPathFindingCompleted;
        mPathFinder_Viz.mPathFinder.onSuccess += OnPathFindingCompleted;
        mPathFinder_Viz.mPathFinder.onStarted += OnPathFindingStarted;
        mPathFinder_Viz.mPathFinder.onChangeCurrentNode += OnChangeCurrentNode;
    }

    void OnPathFindingStarted()
    {
        //// you cannot switch algorithm when path finding is running.
        //mLeanSwitchAlgo.gameObject.SetActive(false);
        //mResetButton.gameObject.SetActive(false);
        Debug.Log("Disabled switch.");
    }

    void OnPathFindingCompleted()
    {
        //// you cannot switch algorithm when path finding is running.
        //mLeanSwitchAlgo.gameObject.SetActive(true);
        //mResetButton.gameObject.SetActive(true);
        Debug.Log("Enabled switch.");
    }

    public void OnChangeCurrentNode(PathFinder<Vector2Int>.PathFinderNode node)
    {
        mFCostText.text = node.Fcost.ToString("F2");
        mGCostText.text = node.GCost.ToString("F2");
        mHCostText.text = node.Hcost.ToString("F2");
    }

    public void ClearGrid()
    {
        mRectGridMap_Vis.MakeAllCellsWalkable();
    }

    #region FSM delegate implementation
    void OnEnterEditor()
    {
        if(mModeTypeText != null)
        {
            mModeTypeText.text = "Editing Mode";
            mToggleInteractive.SetActive(false);
            mPlayButton.gameObject.SetActive(false);
            mStepButton.gameObject.SetActive(false);
            mResetButton.gameObject.SetActive(false);
            mRectGridMap_Vis.ResetPathFindingInfo();
            mLeanSwitchAlgo.gameObject.SetActive(false);
            mAlgorithmText.gameObject.SetActive(false);
            mInteractiveText.gameObject.SetActive(false);
            mClearGridButton.gameObject.SetActive(true);
            mToggleCostFunction.gameObject.SetActive(false);
            mCostPanel.SetActive(false);
        }
    }
    void OnExitEditor()
    { }

    void OnUpdateEditor()
    {
        //We are in the ditor mPathFindingMode. So left mouse button click should
        //make the grid walkable/non-walkable.
        if (Input.GetMouseButtonDown(0))
        {
            mRectGridMap_Vis.RayCastAndToggleWalkable();
        }
    }

    void OnEnterPlayer()
    {
        if (mModeTypeText != null)
        {
            mModeTypeText.text = "Pathfinding Mode";
            mToggleInteractive.SetActive(true);
            SetPathFindingMode(mPathFindingMode);
            mLeanSwitchAlgo.gameObject.SetActive(true);
            mAlgorithmText.gameObject.SetActive(true);
            mInteractiveText.gameObject.SetActive(true);
            mClearGridButton.gameObject.SetActive(false);
            mCostPanel.SetActive(true);
            mToggleCostFunction.gameObject.SetActive(true);
            OnSelectAlgorithm();
        }
    }
    void OnExitPlayer()
    { }
    void OnUpdatePlayer()
    {
        //We are in the player mPathFindingMode. So left mouse button click should
        //relocate the goal point.
        if (Input.GetMouseButtonDown(0))
        {
            // We only enable playing the path finding
            // if the button click is on the grid.
            if (mRectGridMap_Vis.RayCastAndSetGoal())
            {
                mPathFinder_Viz.SetGoal(mRectGridMap_Vis.mGoalObject);
                if (mPathFindingMode == PathFindingMode.GAMEPLAY_MODE)
                {
                    mPathFinder_Viz.FindPath_Play();
                }
            }
        }
    }
    #endregion
}
