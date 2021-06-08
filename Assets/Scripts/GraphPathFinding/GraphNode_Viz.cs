using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAI.PathFinding;

public class GraphNode_Viz : MonoBehaviour
{
    public GraphNode<GraphNodeData> Node { get; set; }
    public SpriteRenderer mInnerSprite;

    private Stack<Color> mColorStack = new Stack<Color>();
    public void SetColor(Color color)
    {
        mColorStack.Push(color);
        mInnerSprite.color = mColorStack.Peek();
    }
    public void UnSetColor()
    {
        mColorStack.Pop();
        mInnerSprite.color = mColorStack.Peek();
    }

    private void Start()
    {
        SetColor(Color.gray);
    }
}
