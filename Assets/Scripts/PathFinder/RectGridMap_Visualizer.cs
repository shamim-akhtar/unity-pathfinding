using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAI.PathFinding;

public class RectGridMap_Visualizer : MonoBehaviour
{
    public int Cols = 10;
    public int Rows = 10;

    public int goalX = 7;
    public int goalY = 8;

    public GameObject PrefabCell;

    public GameObject PrefabAlgoViz;

    public enum PathFindingAlgorithm
    {
        AStar,
        Dijkstra,
        Greedy_Best_First,
    }

    [HideInInspector]
    public float GridCellWidth = 1f;
    [HideInInspector]
    public float GridCellHeight = 1f;
    [HideInInspector]
    public RectGridMap mGrid;

    public Color COLOR_WALKABLE = new Color(1.0f, 1.0f, 1.0f, 0.0f);
    public Color COLOR_NON_WALKABLE = new Color(0.0f, 0.0f, 0.0f, 1.0f);
    public Color COLOR_OPEN_LIST = new Color(0.0f, 0.0f, 1.0f, 0.3f);
    public Color COLOR_CLOSED_LIST = new Color(0.0f, 0.0f, 0.0f, 0.3f);
    public Color COLOR_CURRENT_NODE = new Color(1.0f, 0.0f, 0.0f, 0.3f);
    public Color COLOR_SOLUTION = new Color(0.0f, 1.0f, 1.0f, 0.7f);
    public Color COLOR_DESTINATION = new Color(0.0f, 1.0f, 0.0f, 0.7f);

    private Dictionary<PathFindingAlgorithm, RectGridMap_Visualizer_Algo> mAlgoViz = 
        new Dictionary<PathFindingAlgorithm, RectGridMap_Visualizer_Algo>();


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
        CreateAlgoView(PathFindingAlgorithm.AStar);
        CreateAlgoView(PathFindingAlgorithm.Dijkstra);
        CreateAlgoView(PathFindingAlgorithm.Greedy_Best_First);
        SetCameraSizes();
    }

    void CreateAlgoView(PathFindingAlgorithm type)
    {
        GameObject algoGameObj = Instantiate(PrefabAlgoViz);
        algoGameObj.name = type.ToString();
        RectGridMap_Visualizer_Algo algoScr = algoGameObj.GetComponent<RectGridMap_Visualizer_Algo>();
        if (algoScr != null)
        {
            mAlgoViz.Add(type, algoScr);
            algoScr.mCamera = Camera.main;
            algoScr.mVisualizer = this;
            algoScr.mAlgorithm = type;
            algoScr.Init();
        }
    }
    void SetCameraSizes()
    {
        mAlgoViz[PathFindingAlgorithm.AStar].mCamera.orthographicSize = ((Cols + 1) * GridCellWidth)/2;
        mAlgoViz[PathFindingAlgorithm.Dijkstra].mCamera.orthographicSize = ((Cols + 1) * GridCellWidth)/2;
        mAlgoViz[PathFindingAlgorithm.Greedy_Best_First].mCamera.orthographicSize = ((Cols + 1) * GridCellWidth)/2;

        //mAlgoViz[PathFindingAlgorithm.AStar].mCamera.rect = new Rect(0, 0, 0.5f, 0.5f);
        //mAlgoViz[PathFindingAlgorithm.Dijkstra].mCamera.rect = new Rect(0.5f, 0, 0.5f, 0.5f);
        //mAlgoViz[PathFindingAlgorithm.Greedy_Best_First].mCamera.rect = new Rect(0, 0.5f, 0.5f, 0.5f);
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
                    int x = sc.mGridCellData.Location.x;
                    int y = sc.mGridCellData.Location.y;

                    // because there is only one grid and one set of locations 
                    // so we just need to make the walkable/nonwalkable once.
                    sc.mGridCellData.IsWalkable = !sc.mGridCellData.IsWalkable;
                    foreach (RectGridMap_Visualizer_Algo val in mAlgoViz.Values)
                    {
                        LocationData<Vector2Int> ld = mGrid.GetLocationData(mGrid.GetCell(x, y));

                        if (ld.IsWalkable)
                        {
                            val.mGridCellSprites[x,y].GetComponent<RectGridCell>().SetInnerColor(COLOR_WALKABLE);
                        }
                        else
                        {
                            val.mGridCellSprites[x, y].GetComponent<RectGridCell>().SetInnerColor(COLOR_NON_WALKABLE);
                        }
                    }


                    //// Set walkable for all three algorithm type.
                    //sc.mGridCellData.IsWalkable = !sc.mGridCellData.IsWalkable;

                    //if (sc.mGridCellData.IsWalkable)
                    //{
                    //    sc.SetInnerColor(COLOR_WALKABLE);
                    //}
                    //else
                    //{
                    //    sc.SetInnerColor(COLOR_NON_WALKABLE);
                    //}
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            FindPath();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            foreach (RectGridMap_Visualizer_Algo val in mAlgoViz.Values)
            {
                if (val.mPathFinder.Status == PathFinder<Vector2Int>.PathFinderStatus.RUNNING)
                {
                    val.mPathFinder.Step();
                }
            }
        }
    }

    void FindPath()
    {
        Vector2Int start = Vector2Int.zero;

        bool canInitialize = true;
        foreach(RectGridMap_Visualizer_Algo val in mAlgoViz.Values)
        {
            if (val.mPathFinder.Status == PathFinder<Vector2Int>.PathFinderStatus.RUNNING)
            {
                canInitialize = false;
                break;
            }
        }

        if(canInitialize)
        {
            foreach (RectGridMap_Visualizer_Algo val in mAlgoViz.Values)
            {
                val.Reset();
                val.mPathFinder.Initialize(mGrid, start, new Vector2Int(goalX, goalY));
            }
        }
    }
}
