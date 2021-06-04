using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAI.PathFinding;

public class RectGridMap_Visualizer : MonoBehaviour
{
    public int Cols = 10;
    public int Rows = 10;

    private float GridCellWidth = 1f;
    private float GridCellHeight = 1f;

    public int goalX = 7;
    public int goalY = 8;

    public GameObject PrefabCell;

    public RectGridMap mGrid { get; private set; }
    private PathFinder<Vector2Int> mPathFinder;

    private GameObject[,] mGridCellSprites;

    Color COLOR_WALKABLE = new Color(1.0f, 1.0f, 1.0f, 0.0f);
    Color COLOR_NON_WALKABLE = new Color(0.0f, 0.0f, 0.0f, 1.0f);
    Color COLOR_OPEN_LIST = new Color(0.0f, 0.0f, 1.0f, 0.3f);
    Color COLOR_CLOSED_LIST = new Color(0.0f, 0.0f, 0.0f, 0.3f);
    Color COLOR_CURRENT_NODE = new Color(1.0f, 0.0f, 0.0f, 0.3f);
    Color COLOR_SOLUTION = new Color(0.0f, 1.0f, 1.0f, 0.7f);
    Color COLOR_DESTINATION = new Color(0.0f, 1.0f, 0.0f, 0.7f);

    void CreateGrid()
    {
        for (int i = 0; i < Cols; ++i)
        {
            for (int j = 0; j < Rows; ++j)
            {
                GameObject obj = Instantiate(PrefabCell, new Vector3(GridCellWidth * i, GridCellHeight * j, 0.0f), Quaternion.identity);
                obj.transform.parent = transform;
                RectGridCell sc = obj.GetComponent<RectGridCell>();

                sc.mGridCellData = mGrid.GetLocationData(mGrid.GetCell(i, j));
                mGridCellSprites[i, j] = obj;
                sc.SetInnerColor(COLOR_WALKABLE);
            }
        }


        GameObject obj1 = mGridCellSprites[goalX, goalY];
        RectGridCell cellScript = obj1.GetComponent<RectGridCell>();

        if (cellScript)
        {
            cellScript.SetInnerColor(COLOR_DESTINATION);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        GameObject obj = Instantiate(PrefabCell);
        BoxCollider2D bc = obj.GetComponent<BoxCollider2D>();
        if(bc != null)
        {
            GridCellWidth = 2.0f * bc.bounds.extents.x;
            GridCellHeight = 2.0f * bc.bounds.extents.y;
        }
        obj.SetActive(false);

        mGrid = new RectGridMap(Cols, Rows);
        mGridCellSprites = new GameObject[Cols, Rows];

        CreateGrid();

        Camera.main.orthographicSize = ((Cols + 1) * GridCellWidth) / 2.0f;
        Camera.main.transform.position = new Vector3(
            ((Cols - 1) * GridCellWidth) / 2,
            ((Rows - 1) * GridCellHeight) / 2,
            -10.0f);

        // add the visual delegates to show pathfinding in action.
        //CompPathFinder pf = mNpc.GetComponent<CompPathFinder>();
        mPathFinder = new PathFinder<Vector2Int>();
        mPathFinder.SetGCostFunction(RectGridMap.GetCostBetweenTwoCells);
        mPathFinder.SetHeuristicCostFunction(RectGridMap.GetManhattanCost);
        mPathFinder.onAddToClosedList += OnAddToClosedList;
        mPathFinder.onAddToOpenList += OnAddToOpenList;
        mPathFinder.onChangeCurrentNode += OnChangeCurrentNode;
        mPathFinder.onDestinationFound += OnDestinationFound;
    }


    public void OnChangeCurrentNode(PathFinderNode<Vector2Int> node)
    {
        GameObject obj = mGridCellSprites[node.Location.x, node.Location.y];
        RectGridCell cellScript = obj.GetComponent<RectGridCell>();

        if (cellScript)
        {
            cellScript.SetInnerColor(COLOR_CURRENT_NODE);
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
            cellScript.SetInnerColor(COLOR_OPEN_LIST);
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
            cellScript.SetInnerColor(COLOR_CLOSED_LIST);
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
                cellScript.SetInnerColor(COLOR_SOLUTION);
            }
        }
    }

    public void Reset()
    {
        mPathFinder.Reset();

        for (int i = 0; i < Cols; ++i)
        {
            for (int j = 0; j < Rows; ++j)
            {
                GameObject obj = mGridCellSprites[i, j];
                RectGridCell sc = obj.GetComponent<RectGridCell>();
                sc.mGridCellData = mGrid.GetLocationData(mGrid.GetCell(i, j));

                if (sc.mGridCellData.IsWalkable)
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
        RectGridCell cellScript = obj1.GetComponent<RectGridCell>();

        if (cellScript)
        {
            cellScript.SetInnerColor(COLOR_DESTINATION);
        }
    }

    void Update()
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
                RectGridCell sc = obj.GetComponent<RectGridCell>();
                if (sc != null)
                {
                    sc.mGridCellData.IsWalkable = !sc.mGridCellData.IsWalkable;

                    if (sc.mGridCellData.IsWalkable)
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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Reset();
            FindPath();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (mPathFinder.Status == PathFinder<Vector2Int>.PathFinderStatus.RUNNING)
            {
                mPathFinder.Step();
            }
        }
    }

    void FindPath()
    {
        Vector2Int start = Vector2Int.zero;
        mPathFinder.Initialize(mGrid, start, new Vector2Int(goalX, goalY));
    }
}
