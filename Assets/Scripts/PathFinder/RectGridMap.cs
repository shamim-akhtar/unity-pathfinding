using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameAI
{
    namespace PathFinding
    {
        public class RectGridMap : Map<Vector2Int>
        {
            protected int mX;
            protected int mY;

            public RectGridMap(int numX, int numY) : base()
            {
                mX = numX;
                mY = numY;

                for (int i = 0; i < mX; ++i)
                {
                    for (int j = 0; j < mY; ++j)
                    {
                        LocationData<Vector2Int> data = new LocationData<Vector2Int>();
                        data.Cost = 1.0f;
                        data.IsWalkable = true;
                        data.Location = new Vector2Int(i, j);

                        mIndices.Add(data.Location);
                        mLocations.Add(data.Location, data);
                    }
                }                
            }

            public Vector2Int GetCell(int i, int j)
            {
                return mIndices[i * mX + j];
            }

            public override List<Vector2Int> GetNeighbours(Vector2Int loc)
            {
                List<Vector2Int> neighbours = new List<Vector2Int>();

                int x = loc.x;
                int y = loc.y;

                // Check up.
                if (y < mY - 1)
                {
                    int i = x;
                    int j = y + 1;

                    Vector2Int v = mIndices[i * mX + j];

                    if (mLocations[v].IsWalkable)
                    {
                        neighbours.Add(v);
                    }
                }
                // Check top-right
                if (y < mY - 1 && x < mX - 1)
                {
                    int i = x + 1;
                    int j = y + 1;

                    Vector2Int v = mIndices[i * mX + j];

                    if (mLocations[v].IsWalkable)
                    {
                        neighbours.Add(v);
                    }
                }
                // Check right
                if (x < mX - 1)
                {
                    int i = x + 1;
                    int j = y;

                    Vector2Int v = mIndices[i * mX + j];

                    if (mLocations[v].IsWalkable)
                    {
                        neighbours.Add(v);
                    }
                }
                // Check right-down
                if (x < mX - 1 && y > 0)
                {
                    int i = x + 1;
                    int j = y - 1;

                    Vector2Int v = mIndices[i * mX + j];

                    if (mLocations[v].IsWalkable)
                    {
                        neighbours.Add(v);
                    }
                }
                // Check down
                if (y > 0)
                {
                    int i = x;
                    int j = y - 1;

                    Vector2Int v = mIndices[i * mX + j];

                    if (mLocations[v].IsWalkable)
                    {
                        neighbours.Add(v);
                    }
                }
                // Check down-left
                if (y > 0 && x > 0)
                {
                    int i = x - 1;
                    int j = y - 1;

                    Vector2Int v = mIndices[i * mX + j];

                    if (mLocations[v].IsWalkable)
                    {
                        neighbours.Add(v);
                    }
                }
                // Check left
                if (x > 0)
                {
                    int i = x - 1;
                    int j = y;

                    Vector2Int v = mIndices[i * mX + j];

                    if (mLocations[v].IsWalkable)
                    {
                        neighbours.Add(v);
                    }
                }
                // Check left-top
                if (x > 0 && y < mY - 1)
                {
                    int i = x - 1;
                    int j = y + 1;

                    Vector2Int v = mIndices[i * mX + j];

                    if (mLocations[v].IsWalkable)
                    {
                        neighbours.Add(v);
                    }
                }

                return neighbours;
            }

            public static float GetManhattanCost(Vector2Int a, Vector2Int b)
            {
                return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
            }

            public static float GetCostBetweenTwoCells(Vector2Int a, Vector2Int b)
            {
                return Mathf.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));
            }
        }
    }
}
