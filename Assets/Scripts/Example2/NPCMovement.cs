using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAI.PathFinding;

public class NPCMovement : MonoBehaviour
{
    public AStarPathFinder<RectGridCell> mPathFinder;

    // We will need to have access to the map so that 
    // our path finder can work.
    public RectGridMapMono mMap;
    public GridVisualizer mGridViz;

    // Start is called before the first frame update
    void Start()
    {
        mPathFinder = new AStarPathFinder<RectGridCell>();
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
        
    }

    public void FindPathAndMoveTo(Transform destination)
    {
        if (mPathFinder.Status == PathFinder<RectGridCell>.PathFinderStatus.RUNNING)
        {
            Debug.Log("Path finder already running");
            return;
        }

        Vector2Int goal = mMap.GetWorldPosToGridIndex(destination.position);

        StartCoroutine(Coroutine_FindPath(goal));
    }

    IEnumerator Coroutine_FindPath(Vector2Int goalIndex)
    {
        Vector2Int startIndex = mMap.GetWorldPosToGridIndex(transform.position);
        RectGridCell start = mGridViz.mRectGridMapMono.mPathFinderMap.GetCell(startIndex.x, startIndex.y);
        RectGridCell goal = mGridViz.mRectGridMapMono.mPathFinderMap.GetCell(goalIndex.x, goalIndex.y);


        if (mGridViz != null)
        {
            mGridViz.Reset();
        }
        // NOTE: Remember to call Reset as we are doing a new search.
        mPathFinder.Reset();
        mPathFinder.Initialize(mMap.mPathFinderMap, start, goal);
        while(mPathFinder.Status == PathFinder<RectGridCell>.PathFinderStatus.RUNNING)
        {
            mPathFinder.Step();
            yield return null;
        }

        if(mPathFinder.Status == PathFinder<RectGridCell>.PathFinderStatus.FAILURE)
        {
            Debug.Log("Pathfinder could not find the path to the destination.");
            yield return null;
        }

        if(mPathFinder.Status == PathFinder<RectGridCell>.PathFinderStatus.SUCCESS)
        {
            List<Vector2Int> reverseIndices = new List<Vector2Int>();

            // accumulate the nodes.
            PathFinderNode<RectGridCell> node = mPathFinder.CurrentNode;
            while(node != null)
            {
                reverseIndices.Add(node.Location.Index);
                node = node.Parent;
            }

            StopCoroutine("Coroutine_MoveTo");
            for(int i = reverseIndices.Count - 1; i >= 0; i -= 1)
            {
                yield return StartCoroutine(Coroutine_MoveTo(reverseIndices[i]));
            }
        }
    }

    //private bool player_moving = false;
    // coroutine to swap tiles smoothly
    public IEnumerator Coroutine_MoveOverSeconds(GameObject objectToMove, Vector3 end, float seconds)
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
