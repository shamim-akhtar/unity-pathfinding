using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAI.PathFinding;

public class InteractivePathFinding : MonoBehaviour
{
    public AStarPathFinder<GameAI.PathFinding.RectGridCell> mPathFinder;

    // We will need to have access to the map so that 
    // our path finder can work.
    public RectGridMapMono mMap;
    public GridVisualizer mGridViz;

    // Start is called before the first frame update
    void Start()
    {
        mPathFinder = new AStarPathFinder<GameAI.PathFinding.RectGridCell>();
        mPathFinder.SetGCostFunction(RectGridMap.GetCostBetweenTwoCells);
        mPathFinder.SetHeuristicCostFunction(RectGridMap.GetManhattanCost);

        if (mGridViz != null && mGridViz.gameObject.activeSelf)
        {
            mPathFinder.onAddToClosedList += mGridViz.OnAddToClosedList;
            mPathFinder.onAddToOpenList += mGridViz.OnAddToOpenList;
            mPathFinder.onChangeCurrentNode += mGridViz.OnChangeCurrentNode;
            mPathFinder.onDestinationFound += mGridViz.OnDestinationFound;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (mPathFinder.Status == PathFinder<GameAI.PathFinding.RectGridCell>.PathFinderStatus.RUNNING)
            {
                mPathFinder.Step();
            }

            if (mPathFinder.Status == PathFinder<GameAI.PathFinding.RectGridCell>.PathFinderStatus.FAILURE)
            {
                Debug.Log("Pathfinder could not find the path to the destination.");
            }
            if (mPathFinder.Status == PathFinder<GameAI.PathFinding.RectGridCell>.PathFinderStatus.SUCCESS)
            {
            }
        }
    }

    public void FindPathAndMoveTo(Transform destination)
    {
        if (mPathFinder.Status == PathFinder<GameAI.PathFinding.RectGridCell>.PathFinderStatus.RUNNING)
        {
            Debug.Log("Path finder already running");
            return;
        }

        Vector2Int goalIndex = mMap.GetWorldPosToGridIndex(destination.position);
        Vector2Int startIndex = mMap.GetWorldPosToGridIndex(transform.position);

        GameAI.PathFinding.RectGridCell start = mMap.mPathFinderMap.GetCell(startIndex.x, startIndex.y);
        GameAI.PathFinding.RectGridCell goal = mMap.mPathFinderMap.GetCell(goalIndex.x, goalIndex.y);

        if (mGridViz != null)
        {
            mGridViz.Reset();
        }
        // NOTE: Remember to call Reset as we are doing a new search.
        mPathFinder.Reset();
        mPathFinder.Initialize(mMap.mPathFinderMap, start, goal);
    }
}
