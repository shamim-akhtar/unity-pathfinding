using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAI.PathFinding;

public class GraphPathFinder_Viz : MonoBehaviour
{
    public PathFinder<GraphNodeData> mPathFinder;
    public GraphMap_Viz_Play mGraphMap_Viz_Play;
    public NPCMovement mNPCMovement;

    public GraphNode<GraphNodeData> StartNode { get; set; }

    private bool mReachedGoal = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void SetPathFindingAlgorithm(PathFindingAlgorithm algo)
    {
        switch (algo)
        {
            case PathFindingAlgorithm.AStar:
                {
                    mPathFinder = new AStarPathFinder<GraphNodeData>();
                    break;
                }
            case PathFindingAlgorithm.Dijkstra:
                {
                    mPathFinder = new DijkstraPathFinder<GraphNodeData>();
                    break;
                }
            case PathFindingAlgorithm.Greedy_Best_First:
                {
                    mPathFinder = new GreedyPathFinder<GraphNodeData>();
                    break;
                }
        }

        InitPathFinder();
    }

    private void InitPathFinder()
    {
        mPathFinder.GCostFunction = SampleGraph.GetCostBetweenTwoCells;
        mPathFinder.HCostFunction = SampleGraph.GetManhattanCost;

        mPathFinder.onAddToClosedList += mGraphMap_Viz_Play.OnAddToClosedList;
        mPathFinder.onAddToOpenList += mGraphMap_Viz_Play.OnAddToOpenList;
        mPathFinder.onChangeCurrentNode += mGraphMap_Viz_Play.OnChangeCurrentNode;
        mPathFinder.onDestinationFound += mGraphMap_Viz_Play.OnDestinationFound;
    }

    public void SetGoal(GraphNode<GraphNodeData> destination)
    {
        if (mPathFinder == null)
        {
            SetPathFindingAlgorithm(PathFindingAlgorithm.AStar);
        }
        if (mPathFinder.Status == PathFinderStatus.RUNNING)
        {
            Debug.Log("Path finder already running");
            return;
        }

        // NOTE: Remember to call Reset as we are doing a new search.
        mPathFinder.Reset();
        mPathFinder.Initialize(StartNode, destination);
        mReachedGoal = false;
    }

    // Use this function is you are controlling the path finding step from 
    // the caller side. You can use to interactively call Step for pathfinding
    public void FindPath_Step()
    {
        if (mReachedGoal) return;
        if (mPathFinder.Status == PathFinderStatus.RUNNING)
        {
            mPathFinder.Step();
        }

        if (mPathFinder.Status == PathFinderStatus.FAILURE)
        {
            Debug.Log("Pathfinder could not find the path to the destination.");
        }

        if (mPathFinder.Status == PathFinderStatus.SUCCESS)
        {
            //StartCoroutine(Coroutine_MoveThroughPathNodes());
            CollectWayPointsFromPathFinder();
        }
    }
    void CollectWayPointsFromPathFinder()
    {
        List<Vector2> reverseIndices = new List<Vector2>();

        // accumulate the nodes.
        PathFinder<GraphNodeData>.PathFinderNode node = mPathFinder.CurrentNode;
        while (node != null)
        {
            reverseIndices.Add(node.Location.Value.Point);
            node = node.Parent;
        }

        for (int i = reverseIndices.Count - 1; i >= 0; i -= 1)
        {
            mNPCMovement.AddMoveToPoint(reverseIndices[i]);
        }
        StartNode = (GraphNode<GraphNodeData>)mPathFinder.Goal;
    }

    // Use this function is you to continuously find path using 
    // coroutine.
    public void FindPath_Play()
    {
        StartCoroutine(Coroutine_FindPathAndMove());
    }

    IEnumerator Coroutine_FindPathAndMove()
    {
        while (mPathFinder.Status == PathFinderStatus.RUNNING)
        {
            mPathFinder.Step();
            yield return null;
        }

        if (mPathFinder.Status == PathFinderStatus.FAILURE)
        {
            Debug.Log("Pathfinder could not find the path to the destination.");
            yield return null;
        }

        if (mPathFinder.Status == PathFinderStatus.SUCCESS)
        {
            //StartCoroutine(Coroutine_MoveThroughPathNodes());
            CollectWayPointsFromPathFinder();
        }
    }

    //IEnumerator Coroutine_MoveThroughPathNodes()
    //{
    //    if (!mReachedGoal)
    //    {
    //        List<Vector2> reverseIndices = new List<Vector2>();

    //        // accumulate the nodes.
    //        PathFinder< GraphNodeData>.PathFinderNode node = mPathFinder.CurrentNode;
    //        while (node != null)
    //        {
    //            reverseIndices.Add(node.Location.Value.Point);
    //            node = node.Parent;
    //        }

    //        for (int i = reverseIndices.Count - 1; i >= 0; i -= 1)
    //        {
    //            yield return StartCoroutine(Coroutine_MoveTo(reverseIndices[i], 2.0f));
    //        }
    //        mReachedGoal = true;
    //        StartNode = (GraphNode<GraphNodeData>)mPathFinder.Goal;
    //    }
    //}

    //// coroutine to swap tiles smoothly
    //private IEnumerator Coroutine_MoveOverSeconds(GameObject objectToMove, Vector3 end, float seconds)
    //{
    //    float elapsedTime = 0;
    //    Vector3 startingPos = objectToMove.transform.position;
    //    //player_moving = true;
    //    while (elapsedTime < seconds)
    //    {
    //        objectToMove.transform.position = Vector3.Lerp(startingPos, end, (elapsedTime / seconds));
    //        elapsedTime += Time.deltaTime;

    //        yield return new WaitForEndOfFrame();
    //    }
    //    //player_moving = false;
    //    objectToMove.transform.position = end;
    //}

    //IEnumerator Coroutine_MoveTo(Vector2 p, float duration = 0.1f)
    //{
    //    Vector3 endP = new Vector3(p.x, p.y, transform.position.z);
    //    yield return StartCoroutine(Coroutine_MoveOverSeconds(transform.gameObject, endP, duration));
    //}
}
