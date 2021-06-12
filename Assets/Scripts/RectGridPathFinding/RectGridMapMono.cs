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
}
