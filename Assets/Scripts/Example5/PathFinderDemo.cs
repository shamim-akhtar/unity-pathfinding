using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Patterns;
using Lean.Gui;

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
    public Button mPlayButton;
    public Button mStepButton;
    public Button mResetButton;
    public LeanSwitch mLeanSwitchAlgo;

    public GameObject mToggleInteractive;

    public RectGridMap_Viz mRectGridMap_Vis;
    public PathFinder_Viz mPathFinder_Viz;
    #endregion

    private FiniteStateMachine mFsm = new FiniteStateMachine();

    void Start()
    {
        mFsm.Add(new State((int)ModeType.EDITOR, OnEnterEditor, OnExitEditor, OnUpdateEditor));
        mFsm.Add(new State((int)ModeType.PLAYER, OnEnterPlayer, OnExitPlayer, OnUpdatePlayer));
        mFsm.SetCurrentState((int)ModeType.PLAYER);

        //SetPathFindingMode(mPathFindingMode);
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
        mRectGridMap_Vis.ResetPathFindingInfo();
    }

    public void OnSelectAlgorithm()
    {
        int state = mLeanSwitchAlgo.State;
        if (state == 0)
        {
            mAlgorithmText.text = "Astar";
        }
        if(state == 1)
        {
            mAlgorithmText.text = "Dijkstra";
        }
        if (state == 2)
        {
            mAlgorithmText.text = "Greedy Best-First";
        }
    }

    #region FSM delegate implementation
    void OnEnterEditor()
    {
        if(mModeTypeText != null)
        {
            mModeTypeText.text = "Editor Mode";
            mToggleInteractive.SetActive(false);
            mPlayButton.gameObject.SetActive(false);
            mStepButton.gameObject.SetActive(false);
            mResetButton.gameObject.SetActive(false);
            mRectGridMap_Vis.ResetPathFindingInfo();
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
            mModeTypeText.text = "Player Mode";
            mToggleInteractive.SetActive(true);
            SetPathFindingMode(mPathFindingMode);
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
