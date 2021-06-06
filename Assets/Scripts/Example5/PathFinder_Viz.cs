using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAI.PathFinding;

public class PathFinder_Viz : MonoBehaviour
{
    public PathFinder<RectGridCell> mPathFinder;
    public RectGridMap_Viz mGridViz;

    private bool mReachedGoal = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void SetPathFindingAlgorithm(PathFindingAlgorithm algo)
    {
        switch(algo)
        {
            case PathFindingAlgorithm.AStar:
                {
                    mPathFinder = new AStarPathFinder<RectGridCell>();
                    break;
                }
            case PathFindingAlgorithm.Dijkstra:
                {
                    mPathFinder = new DijkstraPathFinder<RectGridCell>();
                    break;
                }
            case PathFindingAlgorithm.Greedy_Best_First:
                {
                    mPathFinder = new GreedyPathFinder<RectGridCell>();
                    break;
                }
        }

        InitPathFinder();
    }

    private void InitPathFinder()
    {
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

    public void SetGoal(Transform destination)
    {
        if(mPathFinder == null)
        {
            SetPathFindingAlgorithm(PathFindingAlgorithm.AStar);
        }
        if (mPathFinder.Status == PathFinder<RectGridCell>.PathFinderStatus.RUNNING)
        {
            Debug.Log("Path finder already running");
            return;
        }

        Vector2Int goalIndex = mGridViz.GetWorldPosToGridIndex(destination.position);
        Vector2Int startIndex = mGridViz.GetWorldPosToGridIndex(transform.position);

        RectGridCell start = mGridViz.mPathFinderMap.GetCell(startIndex.x, startIndex.y);
        RectGridCell goal = mGridViz.mPathFinderMap.GetCell(goalIndex.x, goalIndex.y);

        if (mGridViz != null)
        {
            mGridViz.ResetPathFindingInfo();
        }

        // NOTE: Remember to call Reset as we are doing a new search.
        mPathFinder.Reset();
        mPathFinder.Initialize(mGridViz.mPathFinderMap, start, goal);
        mReachedGoal = false;
    }

    // Use this function is you are controlling the path finding step from 
    // the caller side. You can use to interactively call Step for pathfinding
    public void FindPath_Step()
    {
        if (mReachedGoal) return;
        if (mPathFinder.Status == PathFinder<RectGridCell>.PathFinderStatus.RUNNING)
        {
            mPathFinder.Step();
        }

        if (mPathFinder.Status == PathFinder<RectGridCell>.PathFinderStatus.FAILURE)
        {
            Debug.Log("Pathfinder could not find the path to the destination.");
        }

        if (mPathFinder.Status == PathFinder<RectGridCell>.PathFinderStatus.SUCCESS)
        {
            StartCoroutine(Coroutine_MoveThroughPathNodes());
        }
    }

    // Use this function is you to continuously find path using 
    // coroutine.
    public void FindPath_Play()
    {
        StartCoroutine(Coroutine_FindPathAndMove());
    }

    IEnumerator Coroutine_FindPathAndMove()
    {
        while (mPathFinder.Status == PathFinder<RectGridCell>.PathFinderStatus.RUNNING)
        {
            mPathFinder.Step();
            yield return null;
        }

        if (mPathFinder.Status == PathFinder<RectGridCell>.PathFinderStatus.FAILURE)
        {
            Debug.Log("Pathfinder could not find the path to the destination.");
            yield return null;
        }

        if (mPathFinder.Status == PathFinder<RectGridCell>.PathFinderStatus.SUCCESS)
        {
            StartCoroutine(Coroutine_MoveThroughPathNodes());
        }
    }

    IEnumerator Coroutine_MoveThroughPathNodes()
    {
        if (!mReachedGoal)
        {
            List<Vector2Int> reverseIndices = new List<Vector2Int>();

            // accumulate the nodes.
            PathFinderNode<RectGridCell> node = mPathFinder.CurrentNode;
            while (node != null)
            {
                reverseIndices.Add(node.Location.Index);
                node = node.Parent;
            }

            StopCoroutine("Coroutine_MoveTo");
            for (int i = reverseIndices.Count - 1; i >= 0; i -= 1)
            {
                yield return StartCoroutine(Coroutine_MoveTo(reverseIndices[i]));
            }
            mReachedGoal = true;
        }
    }

    //private bool player_moving = false;
    // coroutine to swap tiles smoothly
    private IEnumerator Coroutine_MoveOverSeconds(GameObject objectToMove, Vector3 end, float seconds)
    {
        float elapsedTime = 0;
        Vector3 startingPos = objectToMove.transform.position;
        //player_moving = true;
        while (elapsedTime < seconds)
        {
            objectToMove.transform.position = Vector3.Lerp(startingPos, end, (elapsedTime / seconds));
            elapsedTime += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }
        //player_moving = false;
        objectToMove.transform.position = end;
    }

    IEnumerator Coroutine_MoveTo(Vector2Int p, float duration = 0.1f)
    {
        Vector3 endP = new Vector3(p.x, p.y, transform.position.z);
        yield return StartCoroutine(Coroutine_MoveOverSeconds(transform.gameObject, endP, duration));
    }
}
