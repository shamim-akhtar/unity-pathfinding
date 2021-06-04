using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell : MonoBehaviour
{
    public TextMesh FCostText;
    public TextMesh HCostText;
    public TextMesh GCostText;

    public SpriteRenderer InnerSprite;
    public SpriteRenderer OuterSprite;

    public PathFinder.Grid.GridCell mGridCell;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetInnerColor(Color col)
    {
        InnerSprite.color = col;
    }

    public void SetOuterColor(Color col)
    {
        OuterSprite.color = col;
    }

    public void SetFCost(float cost)
    {
        FCostText.text = cost.ToString("F2");
    }

    public void SetHCost(float cost)
    {
        HCostText.text = cost.ToString("F0");
    }

    public void SetGCost(float cost)
    {
        GCostText.text = cost.ToString("F2");
    }

    public void ClearTexts()
    {
        GCostText.text = "";
        HCostText.text = "";
        FCostText.text = "";
    }
}
