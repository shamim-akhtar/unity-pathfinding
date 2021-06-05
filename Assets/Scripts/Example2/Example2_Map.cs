using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAI.PathFinding;

public class Example2_Map : RectGridMapMono
{
    public NPCMovement mNPCMovement;
    public Transform mGoalObject;

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Vector2 rayPos = new Vector2(
                Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
                Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
            RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0f);

            if (hit)
            {
                float x = hit.point.x;
                float y = hit.point.y;

                Vector3 pos = mGoalObject.transform.position;
                pos.x = x;
                pos.y = y;
                mGoalObject.transform.position = pos;

                mNPCMovement.FindPathAndMoveTo(mGoalObject);
            }

            // we start out path finding and player movement immediately.
        }
    }
}
