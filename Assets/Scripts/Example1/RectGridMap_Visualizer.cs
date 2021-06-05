using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAI.PathFinding;

public class RectGridMap_Visualizer : MonoBehaviour
{
    public int Cols = 10;
    public int Rows = 10;

    [HideInInspector]
    public int goalX = 0;
    [HideInInspector]
    public int goalY = 0;

    [HideInInspector]
    public int startX = 0;
    [HideInInspector]
    public int startY = 0;

    public GameObject mStartGameObject;
    public GameObject mGoalGameObject;

    public GameObject PrefabCell;
    public GameObject PrefabAlgoViz;
    public PathFindingAlgorithm mAlgo = PathFindingAlgorithm.AStar;

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
    public Color COLOR_START = new Color(0.0f, 1.0f, 1.0f, 0.7f);

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

        startX = (int)(mStartGameObject.transform.position.x / GridCellWidth);
        startY = (int)(mStartGameObject.transform.position.y / GridCellHeight);
        goalX = (int)(mGoalGameObject.transform.position.x / GridCellWidth);
        goalY = (int)(mGoalGameObject.transform.position.y / GridCellHeight);

        mGrid = new RectGridMap(Cols, Rows);
        CreateAlgoView(PathFindingAlgorithm.AStar);
        CreateAlgoView(PathFindingAlgorithm.Dijkstra);
        CreateAlgoView(PathFindingAlgorithm.Greedy_Best_First);
        SetCameraSizes();

        //mStartGameObject.transform.position = new Vector3(startX, startY, 0.0f);
        //mGoalGameObject.transform.position = new Vector3(goalX, goalY, 0.0f);
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
    }

    void SetActiveView()
    {
        switch(mAlgo)
        {
            case PathFindingAlgorithm.AStar:
                {
                    mAlgoViz[mAlgo].gameObject.SetActive(true);
                    mAlgoViz[PathFindingAlgorithm.Dijkstra].gameObject.SetActive(false);
                    mAlgoViz[PathFindingAlgorithm.Greedy_Best_First].gameObject.SetActive(false);
                    break;
                }
            case PathFindingAlgorithm.Dijkstra:
                {
                    mAlgoViz[mAlgo].gameObject.SetActive(true);
                    mAlgoViz[PathFindingAlgorithm.AStar].gameObject.SetActive(false);
                    mAlgoViz[PathFindingAlgorithm.Greedy_Best_First].gameObject.SetActive(false);
                    break;
                }
            case PathFindingAlgorithm.Greedy_Best_First:
                {
                    mAlgoViz[mAlgo].gameObject.SetActive(true);
                    mAlgoViz[PathFindingAlgorithm.Dijkstra].gameObject.SetActive(false);
                    mAlgoViz[PathFindingAlgorithm.AStar].gameObject.SetActive(false);
                    break;
                }
        }
    }

    void Update()
    {
        SetActiveView();

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
        startX = (int)(mStartGameObject.transform.position.x / GridCellWidth);
        startY = (int)(mStartGameObject.transform.position.y / GridCellHeight);
        goalX = (int)(mGoalGameObject.transform.position.x / GridCellWidth);
        goalY = (int)(mGoalGameObject.transform.position.y / GridCellHeight);

        Vector2Int start = new Vector2Int(startX, startY);

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
