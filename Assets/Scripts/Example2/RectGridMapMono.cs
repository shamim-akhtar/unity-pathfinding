using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAI.PathFinding;

public class RectGridMapMono : MonoBehaviour
{
    public string SceneName = "example2";
    public RectGridMap mPathFinderMap;
    public int Cols = 20;
    public int Rows = 20;

    string mFilename;

    // Start is called before the first frame update
    void Awake()
    {
        // load the grid.
        mFilename = SceneName + ".rectgridmap";
        mPathFinderMap = RectGridMap.Load(mFilename);
        if(mPathFinderMap == null)
        {
            // No map found. Create a new one.
            mPathFinderMap = new RectGridMap(Cols, Rows);
        }
    }

    private void OnDestroy()
    {
        RectGridMap.Save(mPathFinderMap, mFilename);
    }

    // You will need to implement this based on your grid cell size.
    public Vector2Int GetWorldPosToGridIndex(Vector3 pos)
    {
        int x = (int)pos.x;
        int y = (int)pos.y;

        if (x >= 0 && x < Cols && y >= 0 && y < Rows)
            return mPathFinderMap.GetCell(x, y);

        return Vector2Int.zero;
    }
}
