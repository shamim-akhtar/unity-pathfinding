using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAI.PathFinding;

public class InteractivePathFinding : MonoBehaviour
{
    public AStarPathFinder<Vector2Int> mPathFinder;

    // We will need to have access to the map so that 
    // our path finder can work.
    public RectGridMapMono mMap;
    public GridVisualizer mGridViz;

    // Start is called before the first frame update
    void Start()
    {
        mPathFinder = new AStarPathFinder<Vector2Int>();
        mPathFinder = new AStarPathFinder<Vector2Int>();
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
            if (mPathFinder.Status == PathFinder<Vector2Int>.PathFinderStatus.RUNNING)
            {
                mPathFinder.Step();
            }

            if (mPathFinder.Status == PathFinder<Vector2Int>.PathFinderStatus.FAILURE)
            {
                Debug.Log("Pathfinder could not find the path to the destination.");
            }
            if (mPathFinder.Status == PathFinder<Vector2Int>.PathFinderStatus.SUCCESS)
            {
            }
        }
    }

    public void FindPathAndMoveTo(Transform destination)
    {
        if (mPathFinder.Status == PathFinder<Vector2Int>.PathFinderStatus.RUNNING)
        {
            Debug.Log("Path finder already running");
            return;
        }

        Vector2Int goal = mMap.GetWorldPosToGridIndex(destination.position);
        Vector2Int start = mMap.GetWorldPosToGridIndex(transform.position);

        if (mGridViz != null)
        {
            mGridViz.Reset();
        }
        // NOTE: Remember to call Reset as we are doing a new search.
        mPathFinder.Reset();
        mPathFinder.Initialize(mMap.mPathFinderMap, start, goal);
    }
}
