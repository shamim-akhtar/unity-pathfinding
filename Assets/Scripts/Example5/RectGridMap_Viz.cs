using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAI.PathFinding;

public class RectGridMap_Viz : RectGridMapMono
{
    // The prefabs for visualization
    public GameObject PrefabCell;

    public Transform mGoalObject;
    //public PathFinding_Viz_Interactive mPFInteractive;

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
    public Color COLOR_GCOST = new Color(0.0f, 0.0f, 0.0f, 1.0f);
    public Color COLOR_HCOST = new Color(0.0f, 0.0f, 0.0f, 1.0f);
    public Color COLOR_FCOST = new Color(1.0f, 0.0f, 0.0f, 1.0f);

    // The sprites to represent each grid cell.
    [HideInInspector]
    public GameObject[,] mGridCellSprites;

    void CreateGridSprites()
    {
        mGridCellSprites = new GameObject[mPathFinderMap.Cols, mPathFinderMap.Rows];
        for (int i = 0; i < mPathFinderMap.Cols; ++i)
        {
            for (int j = 0; j < mPathFinderMap.Rows; ++j)
            {
                GameObject obj = Instantiate(PrefabCell,
                    new Vector3(
                        GridCellWidth * i,
                        GridCellHeight * j,
                        0.0f),
                    Quaternion.identity);
                obj.transform.parent = transform;
                RectGridCell_Viz sc = obj.GetComponent<RectGridCell_Viz>();

                sc.mGridCellData = mPathFinderMap.GetCell(i, j);
                mGridCellSprites[i, j] = obj;

                sc.SetHCostColor(COLOR_HCOST);
                sc.SetFCostColor(COLOR_FCOST);
                sc.SetGCostColor(COLOR_GCOST);
            }
        }

        ResetPathFindingInfo();
    }

    private void Start()
    {
        CreateGridSprites();
    }

    #region Delegate implementations
    private void Update_RectGridCell_Viz(PathFinderNode<RectGridCell> node, Color color)
    {
        GameObject obj = mGridCellSprites[node.Location.Index.x, node.Location.Index.y];
        RectGridCell_Viz cellScript = obj.GetComponent<RectGridCell_Viz>();

        if (cellScript)
        {
            cellScript.SetInnerColor(color);
            cellScript.SetHCost(node.Hcost);
            cellScript.SetGCost(node.GCost);
            cellScript.SetFCost(node.Fcost);
        }
    }

    public void OnChangeCurrentNode(PathFinderNode<RectGridCell> node)
    {
        Update_RectGridCell_Viz(node, COLOR_CURRENT_NODE);
    }

    public void OnAddToOpenList(PathFinderNode<RectGridCell> node)
    {
        Update_RectGridCell_Viz(node, COLOR_OPEN_LIST);
    }

    public void OnAddToClosedList(PathFinderNode<RectGridCell> node)
    {
        Update_RectGridCell_Viz(node, COLOR_CLOSED_LIST);
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


    public void ResetPathFindingInfo()
    {
        for (int i = 0; i < mPathFinderMap.Cols; ++i)
        {
            for (int j = 0; j < mPathFinderMap.Rows; ++j)
            {
                GameObject obj = mGridCellSprites[i, j];
                RectGridCell_Viz sc = obj.GetComponent<RectGridCell_Viz>();
                sc.mGridCellData = mPathFinderMap.GetCell(i, j);

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

    public void ToggleWalkable(RectGridCell_Viz sc)
    {
        if (sc == null)
            return;

        int x = sc.mGridCellData.Index.x;
        int y = sc.mGridCellData.Index.y;

        // because there is only one grid and one set of locations 
        // so we just need to make the walkable/nonwalkable once.
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

    public void RayCastAndToggleWalkable()
    {
        Vector2 rayPos = new Vector2(
            Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
            Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0f);

        if (hit)
        {
            GameObject obj = hit.transform.gameObject;
            RectGridCell_Viz sc = obj.GetComponent<RectGridCell_Viz>();
            ToggleWalkable(sc);
        }
    }

    public void MakeAllCellsWalkable()
    {
        for (int i = 0; i < mPathFinderMap.Cols; ++i)
        {
            for (int j = 0; j < mPathFinderMap.Rows; ++j)
            {
                GameObject obj = mGridCellSprites[i, j];
                RectGridCell_Viz sc = obj.GetComponent<RectGridCell_Viz>();
                sc.mGridCellData = mPathFinderMap.GetCell(i, j);
                sc.mGridCellData.IsWalkable = true;
                sc.SetInnerColor(COLOR_WALKABLE);
                sc.ClearTexts();
            }
        }
    }

    public bool RayCastAndSetGoal()
    {
        Vector2 rayPos = new Vector2(
            Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
            Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0f);

        if (hit)
        {
            GameObject obj = hit.transform.gameObject;
            RectGridCell_Viz sc = obj.GetComponent<RectGridCell_Viz>();
            if (sc == null) return false;

            //float x = hit.point.x;
            //float y = hit.point.y;

            Vector3 pos = mGoalObject.transform.position;
            //Vector2Int p = GetWorldPosToGridIndex(new Vector3(x, y, 0.0f));
            pos.x = sc.mGridCellData.Index.x;
            pos.y = sc.mGridCellData.Index.y;
            mGoalObject.transform.position = pos;
            return true;
        }
        return false;
    }
}
