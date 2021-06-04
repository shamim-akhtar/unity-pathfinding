using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAI.PathFinding;

public class RectGridMap_Visualizer_Algo : MonoBehaviour
{
    [HideInInspector]
    public PathFinder<Vector2Int> mPathFinder;

    [HideInInspector]
    public GameObject[,] mGridCellSprites;

    public RectGridMap_Visualizer.PathFindingAlgorithm mAlgorithm = RectGridMap_Visualizer.PathFindingAlgorithm.AStar;
    public RectGridMap_Visualizer mVisualizer;
    public Camera mCamera;

    void CreateGrid()
    {
        mGridCellSprites = new GameObject[mVisualizer.Cols, mVisualizer.Rows];
        for (int i = 0; i < mVisualizer.Cols; ++i)
        {
            for (int j = 0; j < mVisualizer.Rows; ++j)
            {
                GameObject obj = Instantiate(mVisualizer.PrefabCell, 
                    new Vector3(
                        mVisualizer.GridCellWidth * i, 
                        mVisualizer.GridCellHeight * j, 
                        0.0f), 
                    Quaternion.identity);
                obj.transform.parent = transform;
                RectGridCell sc = obj.GetComponent<RectGridCell>();

                sc.mGridCellData = mVisualizer.mGrid.GetLocationData(mVisualizer.mGrid.GetCell(i, j));
                mGridCellSprites[i, j] = obj;
                sc.SetInnerColor(mVisualizer.COLOR_WALKABLE);
            }
        }

        GameObject obj1 = mGridCellSprites[mVisualizer.goalX, mVisualizer.goalY];
        RectGridCell cellScript = obj1.GetComponent<RectGridCell>();

        if (cellScript)
        {
            cellScript.SetInnerColor(mVisualizer.COLOR_DESTINATION);
        }
    }

    public void Init()
    {
        CreateGrid();

        mCamera.transform.position = new Vector3(
            ((mVisualizer.Cols - 1) * mVisualizer.GridCellWidth) / 2,
            ((mVisualizer.Rows - 1) * mVisualizer.GridCellHeight) / 2,
            -10.0f);

        switch(mAlgorithm)
        {
            case RectGridMap_Visualizer.PathFindingAlgorithm.AStar:
                {
                    mPathFinder = new AStarPathFinder<Vector2Int>();
                    break;
                }
            case RectGridMap_Visualizer.PathFindingAlgorithm.Dijkstra:
                {
                    mPathFinder = new DijkstraPathFinder<Vector2Int>();
                    break;
                }
            case RectGridMap_Visualizer.PathFindingAlgorithm.Greedy_Best_First:
                {
                    mPathFinder = new GreedyPathFinder<Vector2Int>();
                    break;
                }
        }
        //mPathFinder = new AStarPathFinder<Vector2Int>();
        mPathFinder.SetGCostFunction(RectGridMap.GetCostBetweenTwoCells);
        mPathFinder.SetHeuristicCostFunction(RectGridMap.GetManhattanCost);
        mPathFinder.onAddToClosedList += OnAddToClosedList;
        mPathFinder.onAddToOpenList += OnAddToOpenList;
        mPathFinder.onChangeCurrentNode += OnChangeCurrentNode;
        mPathFinder.onDestinationFound += OnDestinationFound;
    }

    #region Delegate implementations

    public void OnChangeCurrentNode(PathFinderNode<Vector2Int> node)
    {
        GameObject obj = mGridCellSprites[node.Location.x, node.Location.y];
        RectGridCell cellScript = obj.GetComponent<RectGridCell>();

        if (cellScript)
        {
            cellScript.SetInnerColor(mVisualizer.COLOR_CURRENT_NODE);
            cellScript.SetHCost(node.Hcost);
            cellScript.SetGCost(node.GCost);
            cellScript.SetFCost(node.Fcost);
        }
    }

    public void OnAddToOpenList(PathFinderNode<Vector2Int> node)
    {
        GameObject obj = mGridCellSprites[node.Location.x, node.Location.y];
        RectGridCell cellScript = obj.GetComponent<RectGridCell>();

        if (cellScript)
        {
            cellScript.SetInnerColor(mVisualizer.COLOR_OPEN_LIST);
            cellScript.SetHCost(node.Hcost);
            cellScript.SetGCost(node.GCost);
            cellScript.SetFCost(node.Fcost);
        }
    }

    public void OnAddToClosedList(PathFinderNode<Vector2Int> node)
    {
        GameObject obj = mGridCellSprites[node.Location.x, node.Location.y];
        RectGridCell cellScript = obj.GetComponent<RectGridCell>();

        if (cellScript)
        {
            cellScript.SetInnerColor(mVisualizer.COLOR_CLOSED_LIST);
            cellScript.SetHCost(node.Hcost);
            cellScript.SetGCost(node.GCost);
            cellScript.SetFCost(node.Fcost);
        }
    }

    public void OnDestinationFound(PathFinderNode<Vector2Int> node)
    {
        List<PathFinderNode<Vector2Int>> path = new List<PathFinderNode<Vector2Int>>();

        PathFinderNode<Vector2Int> n = node;
        while (n != null)
        {
            path.Add(n);
            n = n.Parent;
        }

        for (int i = path.Count - 1; i >= 0; i = i - 1)
        {
            n = path[i];
            GameObject obj = mGridCellSprites[n.Location.x, n.Location.y];
            RectGridCell cellScript = obj.GetComponent<RectGridCell>();

            if (cellScript)
            {
                cellScript.SetInnerColor(mVisualizer.COLOR_SOLUTION);
            }
        }
    }
    #endregion


    public void Reset()
    {
        mPathFinder.Reset();

        for (int i = 0; i < mVisualizer.Cols; ++i)
        {
            for (int j = 0; j < mVisualizer.Rows; ++j)
            {
                GameObject obj = mGridCellSprites[i, j];
                RectGridCell sc = obj.GetComponent<RectGridCell>();
                sc.mGridCellData = mVisualizer.mGrid.GetLocationData(mVisualizer.mGrid.GetCell(i, j));

                if (sc.mGridCellData.IsWalkable)
                {
                    sc.SetInnerColor(mVisualizer.COLOR_WALKABLE);
                }
                else
                {
                    sc.SetInnerColor(mVisualizer.COLOR_NON_WALKABLE);
                }
                sc.ClearTexts();
            }
        }
        GameObject obj1 = mGridCellSprites[mVisualizer.goalX, mVisualizer.goalY];
        RectGridCell cellScript = obj1.GetComponent<RectGridCell>();

        if (cellScript)
        {
            cellScript.SetInnerColor(mVisualizer.COLOR_DESTINATION);
        }
    }

    private void Update()
    {
    }
}
