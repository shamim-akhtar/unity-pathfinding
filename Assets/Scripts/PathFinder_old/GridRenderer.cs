using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathFinder;

public class GridRenderer : MonoBehaviour
{
    public int Cols = 10;
    public int Rows = 10;
    public float GridCellWidth = 1f;
    public float GridCellHeight = 1f;

    public int goalX = 7;
    public int goalY = 8;

    public GameObject PrefabCell;

    public GameObject mNpc;

    public PathFinder.Grid mGrid { get; private set; }
    private GameObject[,] mGridCellSprites;

    Color COLOR_WALKABLE = new Color(1.0f, 1.0f, 1.0f, 0.0f);
    Color COLOR_NON_WALKABLE = new Color(0.0f, 0.0f, 0.0f, 1.0f);
    Color COLOR_OPEN_LIST = new Color(0.0f, 0.0f, 1.0f, 0.3f);
    Color COLOR_CLOSED_LIST = new Color(0.0f, 0.0f, 0.0f, 0.3f);
    Color COLOR_CURRENT_NODE = new Color(1.0f, 0.0f, 0.0f, 0.3f);
    Color COLOR_SOLUTION = new Color(0.0f, 1.0f, 1.0f, 0.7f);
    Color COLOR_DESTINATION = new Color(0.0f, 1.0f, 0.0f, 0.7f);

    IEnumerator Coroutine_CreateGrid()
    {
        for(int i = 0; i < Cols; ++i)
        {
            for(int j = 0; j < Rows; ++j)
            {
                GameObject obj = Instantiate(PrefabCell, new Vector3(GridCellWidth * i, GridCellHeight * j, 0.0f), Quaternion.identity);
                obj.transform.parent = transform;
                GridCell sc = obj.GetComponent<GridCell>();
                sc.mGridCell = mGrid.GetCell(i, j);
                mGridCellSprites[i, j] = obj;

                sc.SetInnerColor(COLOR_WALKABLE);
                yield return null;
            }
        }


        GameObject obj1 = mGridCellSprites[goalX, goalY];
        GridCell cellScript = obj1.GetComponent<GridCell>();

        if (cellScript)
        {
            cellScript.SetInnerColor(COLOR_DESTINATION);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        mGrid = new PathFinder.Grid(Cols, Rows);
        mGridCellSprites = new GameObject[Cols, Rows];

        StartCoroutine(Coroutine_CreateGrid());

        Camera.main.orthographicSize = ((Cols + 1) * GridCellWidth) / 2.0f;
        Camera.main.transform.position = new Vector3(
            ((Cols - 1) * GridCellWidth) / 2, 
            ((Rows - 1) * GridCellHeight) / 2, 
            -10.0f);

        // add the visual delegates to show pathfinding in action.
        CompPathFinder pf = mNpc.GetComponent<CompPathFinder>();
        pf.mPathFinder.onAddToClosedList += OnAddToClosedList;
        pf.mPathFinder.onAddToOpenList += OnAddToOpenList;
        pf.mPathFinder.onChangeCurrentNode += OnChangeCurrentNode;
        pf.mPathFinder.onDestinationFound += OnDestinationFound;
    }

    private void Update()
    {
        // toggle go/no go cells.
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 rayPos = new Vector2(
                Camera.main.ScreenToWorldPoint(Input.mousePosition).x, 
                Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
            RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0f);

            if (hit)
            {
                GameObject obj = hit.transform.gameObject;
                GridCell sc = obj.GetComponent<GridCell>();
                if(sc != null)
                {
                    sc.mGridCell.Walkable = !sc.mGridCell.Walkable;

                    if(sc.mGridCell.Walkable)
                    {
                        sc.SetInnerColor(COLOR_WALKABLE);
                    }
                    else
                    {
                        sc.SetInnerColor(COLOR_NON_WALKABLE);
                    }
                }
            }
        }
    }

    public void OnChangeCurrentNode(PathFinder.AStarPathFinder.Node node)
    {
        GameObject obj = mGridCellSprites[node.Cell.X, node.Cell.Y];
        GridCell cellScript = obj.GetComponent<GridCell>();

        if(cellScript)
        {
            cellScript.SetInnerColor(COLOR_CURRENT_NODE);
            cellScript.SetHCost(node.Hcost);
            cellScript.SetGCost(node.GCost);
            cellScript.SetFCost(node.Fcost);
        }
    }

    public void OnAddToOpenList(PathFinder.AStarPathFinder.Node node)
    {
        GameObject obj = mGridCellSprites[node.Cell.X, node.Cell.Y];
        GridCell cellScript = obj.GetComponent<GridCell>();

        if (cellScript)
        {
            cellScript.SetInnerColor(COLOR_OPEN_LIST);
            cellScript.SetHCost(node.Hcost);
            cellScript.SetGCost(node.GCost);
            cellScript.SetFCost(node.Fcost);
        }
    }

    public void OnAddToClosedList(PathFinder.AStarPathFinder.Node node)
    {
        GameObject obj = mGridCellSprites[node.Cell.X, node.Cell.Y];
        GridCell cellScript = obj.GetComponent<GridCell>();

        if (cellScript)
        {
            cellScript.SetInnerColor(COLOR_CLOSED_LIST);
            cellScript.SetHCost(node.Hcost);
            cellScript.SetGCost(node.GCost);
            cellScript.SetFCost(node.Fcost);
        }
    }

    public void OnDestinationFound(PathFinder.AStarPathFinder.Node node)
    {
        List<PathFinder.AStarPathFinder.Node> path = new List<AStarPathFinder.Node>();

        PathFinder.AStarPathFinder.Node n = node;
        while(n != null)
        {
            path.Add(n);
            n = n.Parent;
        }

        for(int i = path.Count - 1; i >=0; i = i - 1)
        {
            n = path[i];
            GameObject obj = mGridCellSprites[n.Cell.X, n.Cell.Y];
            GridCell cellScript = obj.GetComponent<GridCell>();

            if (cellScript)
            {
                cellScript.SetInnerColor(COLOR_SOLUTION);
            }
        }
    }

    public void Reset()
    {
        for (int i = 0; i < Cols; ++i)
        {
            for (int j = 0; j < Rows; ++j)
            {
                GameObject obj = mGridCellSprites[i, j];
                GridCell sc = obj.GetComponent<GridCell>();
                sc.mGridCell = mGrid.GetCell(i, j);

                if (sc.mGridCell.Walkable)
                {
                    sc.SetInnerColor(COLOR_WALKABLE);
                }
                else
                {
                    sc.SetInnerColor(COLOR_NON_WALKABLE);
                }
                sc.ClearTexts();
            }
        }
        GameObject obj1 = mGridCellSprites[goalX, goalY];
        GridCell cellScript = obj1.GetComponent<GridCell>();

        if (cellScript)
        {
            cellScript.SetInnerColor(COLOR_DESTINATION);
        }
    }
}
