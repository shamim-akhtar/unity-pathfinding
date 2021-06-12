using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAI.PathFinding;

public class GraphNode_Viz : MonoBehaviour
{
    public GraphNodeData Data { get; set; }
    public GraphNode<GraphNodeData> Node { get; set; }
    public SpriteRenderer mInnerSprite;

    public Color DEFAULT_COLOR = new Color(0.2f, 0.2f, 0.2f, 0.2f);

    //public Line mLine;
    List<GameObject> mLines = new List<GameObject>();

    float mOriginalCameraSize = 10.0f;
    float mLineWidth = 0.2f;

    public void SetColor(Color color)
    {
        mInnerSprite.color = color;
    }

    public void ResetColor()
    {
        SetColor(DEFAULT_COLOR);
    }

    private void Start()
    {
        SetColor(DEFAULT_COLOR);
        mOriginalCameraSize = Camera.main.orthographicSize;
    }

    private LineRenderer GetOrCreateLine(int index)
    {
        if(index >= mLines.Count)
        {
            GameObject obj = new GameObject();
            obj.name = "line_" + index.ToString();
            obj.transform.SetParent(transform);
            LineRenderer lr = obj.AddComponent<LineRenderer>();
            mLines.Add(obj);
        }
        return mLines[index].GetComponent<LineRenderer>();
    }

    public void ShowNeighbourLines(bool flag)
    {
        for (int i = 0; i < Node.Neighbours.Count; ++i)
        {
            Vector3 endPoint = new Vector3(Node.Neighbours[i].Value.Point.x, Node.Neighbours[i].Value.Point.y, 0.0f);
            LineRenderer lr = GetOrCreateLine(i);
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = Color.white;
            lr.endColor = Color.black;
            lr.startWidth = mLineWidth;
            lr.endWidth = mLineWidth;

            SetPoints(lr, new Vector2(transform.position.x, transform.position.y), Node.Neighbours[i].Value.Point);

            lr.gameObject.SetActive(flag);
        }
    }

    private void SetPoints(LineRenderer line, Vector2 start, Vector2 end)
    {
        // calculate two points on the circle based on the angle between the start and end point.
        Vector2 d = end - start;
        float angle1 = Mathf.Atan2(d.y, d.x);
        float pa = angle1 + Mathf.Deg2Rad * 30.0f;

        float px = Mathf.Cos(pa) * transform.localScale.x * 0.2f;
        float py = Mathf.Sin(pa) * transform.localScale.y * 0.2f;

        float angle2 = Mathf.Atan2((start.y-end.y), (start.x-end.x));
        float ea = angle2 - Mathf.Deg2Rad * 30.0f;

        float ex = Mathf.Cos(ea) * transform.localScale.x * 0.2f;
        float ey = Mathf.Sin(ea) * transform.localScale.y * 0.2f;

        line.SetPosition(0, new Vector3(start.x + px, start.y + py, 0.0f));
        line.SetPosition(1, new Vector3(end.x + ex, end.y + ey, 0.0f));
    }

    void LateUpdate()
    {
        // set the lines to constant line width no matter the size of the camera.
        float lw = Camera.main.orthographicSize / mOriginalCameraSize * mLineWidth;
        for(int i = 0; i < mLines.Count; ++i)
        {
            LineRenderer lr = mLines[i].GetComponent<LineRenderer>();
            lr.startWidth = lw;
            lr.endWidth = lw;
        }

        if (Node == null) return;
        for (int i = 0; i < Node.Neighbours.Count; ++i)
        {
            Vector3 endPoint = new Vector3(Node.Neighbours[i].Value.Point.x, Node.Neighbours[i].Value.Point.y, 0.0f);
            LineRenderer lr = GetOrCreateLine(i);
            SetPoints(lr, new Vector2(transform.position.x, transform.position.y), Node.Neighbours[i].Value.Point);
        }
    }
}
