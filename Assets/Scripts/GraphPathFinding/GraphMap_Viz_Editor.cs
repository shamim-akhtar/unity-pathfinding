using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAI.PathFinding;

public class GraphMap_Viz_Editor : MonoBehaviour
{
    public int mY = 10;
    public int mX = 10;
    public float mSpacing = 1.0f;

    public GameObject PrefabGraphNode;

    public Transform ParentForGraphNodes;

    private SampleGraph mGraph = new SampleGraph();
    private Dictionary<GameObject, GraphNode<GraphNodeData>> mGameObjGraphNodeDic = 
        new Dictionary<GameObject, GraphNode<GraphNodeData>>();

    #region Private data
    private GameObject mSelectedGraphNode;
    #endregion

    void Start()
    {
        for(int i = 0; i < mX; ++i)
        {
            for(int j = 0; j < mY; ++j)
            {
                float x = i * mSpacing;
                float y = j * mSpacing;
                GameObject obj = Instantiate(PrefabGraphNode, new Vector3(x, y, 0.0f), Quaternion.identity);
                string name = "node_" + i.ToString() + "_" + j.ToString();
                GraphNodeData data = new GraphNodeData(name, x, y);
                GraphNode<GraphNodeData> node = new GraphNode<GraphNodeData>(data);
                mGraph.AddNode(node);

                // Keep a reference of the graph node in the graph node viz.
                obj.GetComponent<GraphNode_Viz>().Node = node;
                obj.GetComponent<ConstantScreenSizeForSprite>().Camera = Camera.main;

                if(ParentForGraphNodes != null)
                {
                    obj.transform.SetParent(ParentForGraphNodes);
                }
            }
        }

        mGraph.CalculateExtent();

        Camera.main.transform.position = new Vector3(mGraph.Extent.center.x, mGraph.Extent.center.y, -10.0f);
        Camera.main.orthographicSize = 1.2f * mGraph.Extent.height / 2.0f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RayCast_SelectGraphNode()
    {
        Vector2 rayPos = new Vector2(
            Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
            Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0f);

        if (hit)
        {
            GameObject obj = hit.transform.gameObject;
            SelectGraphNode(obj);
        }
    }

    public void SelectGraphNode(GameObject obj)
    {
        mSelectedGraphNode = obj;
    }
}
