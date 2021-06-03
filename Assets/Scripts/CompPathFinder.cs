using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompPathFinder : MonoBehaviour
{
    public PathFinder.AStarPathFinder mPathFinder = new PathFinder.AStarPathFinder();
    public GridRenderer mGridRenderer;

    private int goalX = 7;
    private int goalY = 8;

    // Start is called before the first frame update
    void Start()
    {
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            mGridRenderer.Reset();
            FindPath();
        }

        if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (mPathFinder.Status == PathFinder.AStarPathFinder.PathFinderStatus.RUNNING)
            {
                mPathFinder.SearchStep();
            }
        }
    }

    IEnumerator Coroutine_FindPath()
    {
        mPathFinder.mGrid = mGridRenderer.mGrid;

        float cw = mGridRenderer.GridCellWidth;
        float ch = mGridRenderer.GridCellHeight;

        int sx = (int)(transform.position.x / cw);
        int sy = (int)(transform.position.y / ch);

        mPathFinder.SearchInitialize(sx, sy, goalX, goalY);

        PathFinder.AStarPathFinder.PathFinderStatus status = mPathFinder.Status;
        while(status == PathFinder.AStarPathFinder.PathFinderStatus.RUNNING)
        {
            mPathFinder.SearchStep();
            yield return new WaitForSeconds(1.0f);
        }
        
        if(status == PathFinder.AStarPathFinder.PathFinderStatus.FAILURE)
        {
            Debug.Log("Error: Path not found");
        }
        else
        {
            //yield return StartCoroutine(MoveToDestination())
        }
    }

    void FindPath()
    {
        //StartCoroutine(Coroutine_FindPath());
        mPathFinder.mGrid = mGridRenderer.mGrid;

        float cw = mGridRenderer.GridCellWidth;
        float ch = mGridRenderer.GridCellHeight;

        int sx = (int)(transform.position.x / cw);
        int sy = (int)(transform.position.y / ch);

        mPathFinder.SearchInitialize(sx, sy, goalX, goalY);
    }
}
