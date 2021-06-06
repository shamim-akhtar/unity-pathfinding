using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAI.PathFinding;

public class GridVisualizer : MonoBehaviour
{
    //The path finder map.
    public RectGridMapMono mRectGridMapMono;

    // The prefabs for visualization
    public GameObject PrefabCell;

    [HideInInspector]
    public float GridCellWidth = 1f;
    [HideInInspector]
    public float GridCellHeight = 1f;

    public Color COLOR_WALKABLE = new Color(0f, 0.0f, 1.0f, 0.0f);
    public Color COLOR_NON_WALKABLE = new Color(0.0f, 0.0f, 0.0f, 1.0f);
    public Color COLOR_OPEN_LIST = new Color(0.0f, 0.0f, 1.0f, 0.3f);
    public Color COLOR_CLOSED_LIST = new Color(0.0f, 0.0f, 0.0f, 0.3f);
    public Color COLOR_CURRENT_NODE = new Color(1.0f, 0.0f, 0.0f, 0.3f);
    public Color COLOR_SOLUTION = new Color(0.0f, 1.0f, 1.0f, 0.7f);
    public Color COLOR_DESTINATION = new Color(0.0f, 1.0f, 0.0f, 0.7f);
    public Color COLOR_START = new Color(0.0f, 1.0f, 1.0f, 0.7f);

    // The sprites to represent each grid cell.
    [HideInInspector]
    public GameObject[,] mGridCellSprites;
    //public Camera mCamera;

    void CreateGrid()
    {
        mGridCellSprites = new GameObject[mRectGridMapMono.mPathFinderMap.Cols, mRectGridMapMono.mPathFinderMap.Rows];
        for (int i = 0; i < mRectGridMapMono.mPathFinderMap.Cols; ++i)
        {
            for (int j = 0; j < mRectGridMapMono.mPathFinderMap.Rows; ++j)
            {
                GameObject obj = Instantiate(PrefabCell,
                    new Vector3(
                        GridCellWidth * i,
                        GridCellHeight * j,
                        0.0f),
                    Quaternion.identity);
                obj.transform.parent = transform;
                RectGridCell_Viz sc = obj.GetComponent<RectGridCell_Viz>();

                sc.mGridCellData = mRectGridMapMono.mPathFinderMap.GetCell(i, j);
                mGridCellSprites[i, j] = obj;
                sc.SetInnerColor(COLOR_WALKABLE);
            }
        }

        Reset();
    }

    private void Start()
    {
        CreateGrid();
    }

    #region Delegate implementations

    public void OnChangeCurrentNode(PathFinderNode<RectGridCell> node)
    {
        GameObject obj = mGridCellSprites[node.Location.Index.x, node.Location.Index.y];
        RectGridCell_Viz cellScript = obj.GetComponent<RectGridCell_Viz>();

        if (cellScript)
        {
            cellScript.SetInnerColor(COLOR_CURRENT_NODE);
            cellScript.SetHCost(node.Hcost);
            cellScript.SetGCost(node.GCost);
            cellScript.SetFCost(node.Fcost);
        }
    }

    public void OnAddToOpenList(PathFinderNode<RectGridCell> node)
    {
        GameObject obj = mGridCellSprites[node.Location.Index.x, node.Location.Index.y];
        RectGridCell_Viz cellScript = obj.GetComponent<RectGridCell_Viz>();

        if (cellScript)
        {
            cellScript.SetInnerColor(COLOR_OPEN_LIST);
            cellScript.SetHCost(node.Hcost);
            cellScript.SetGCost(node.GCost);
            cellScript.SetFCost(node.Fcost);
        }
    }

    public void OnAddToClosedList(PathFinderNode<RectGridCell> node)
    {
        GameObject obj = mGridCellSprites[node.Location.Index.x, node.Location.Index.y];
        RectGridCell_Viz cellScript = obj.GetComponent<RectGridCell_Viz>();

        if (cellScript)
        {
            cellScript.SetInnerColor(COLOR_CLOSED_LIST);
            cellScript.SetHCost(node.Hcost);
            cellScript.SetGCost(node.GCost);
            cellScript.SetFCost(node.Fcost);
        }
    }

    public void OnDestinationFound(PathFinderNode<RectGridCell> node)
    {
        List<PathFinderNode<RectGridCell>> path = new List<PathFinderNode<RectGridCell>>();
        GameObject obj = mGridCellSprites[node.Location.Index.x, node.Location.Index.y];

        PathFinderNode<RectGridCell> n = node;
        while (n != null)
        {
            path.Add(n);
            n = n.Parent;
        }

        for (int i = path.Count - 1; i >= 0; i = i - 1)
        {
            n = path[i];
            obj = mGridCellSprites[n.Location.Index.x, n.Location.Index.y];
            RectGridCell_Viz cellScript = obj.GetComponent<RectGridCell_Viz>();

            if (cellScript)
            {
                cellScript.SetInnerColor(COLOR_SOLUTION);
            }
        }
    }
    #endregion


    public void Reset()
    {
        for (int i = 0; i < mRectGridMapMono.mPathFinderMap.Cols; ++i)
        {
            for (int j = 0; j < mRectGridMapMono.mPathFinderMap.Rows; ++j)
            {
                GameObject obj = mGridCellSprites[i, j];
                RectGridCell_Viz sc = obj.GetComponent<RectGridCell_Viz>();
                sc.mGridCellData = mRectGridMapMono.mPathFinderMap.GetCell(i, j);

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
                RectGridCell_Viz sc = obj.GetComponent<RectGridCell_Viz>();
                if (sc != null)
                {
                    int x = sc.mGridCellData.Index.x;
                    int y = sc.mGridCellData.Index.y;

                    // because there is only one grid and one set of locations 
                    // so we just need to make the walkable/nonwalkable once.
                    sc.mGridCellData.IsWalkable = !sc.mGridCellData.IsWalkable;

                    if (sc.mGridCellData.IsWalkable)
                    {
                        mGridCellSprites[x, y].GetComponent<RectGridCell_Viz>().SetInnerColor(COLOR_WALKABLE);
                    }
                    else
                    {
                        mGridCellSprites[x, y].GetComponent<RectGridCell_Viz>().SetInnerColor(COLOR_NON_WALKABLE);
                    }
                }
            }
        }
    }
}
