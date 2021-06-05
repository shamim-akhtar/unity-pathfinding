using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameAI
{
    namespace PathFinding
    {
        /// <summary>
        /// This is a Rectangular Grid implementatation of the Map.
        /// Whenever you are using a Rectangular (or square) grid 
        /// for your map, you can use this example implementation.
        /// This class shows how you can create a concrete map implementation
        /// for your path finding.
        /// This grid map used Vector2Int to store the x and y indices.
        /// There are other example implementation of map grid as well.
        /// I will implement a few other types of map grids for demonstration.
        /// </summary>
        public class RectGridMap : Map<Vector2Int>
        {
            // the max number of colums in the grid.
            protected int mX;
            // the max number of rows in the grid
            protected int mY;

            // the 2d array of Vecto2Int.
            // This stucture stores the 2d indices of the grid cells.
            protected Vector2Int[,] mIndices;

            // Construct a grid with the max cols and rows.
            public RectGridMap(int numX, int numY) : base()
            {
                mX = numX;
                mY = numY;

                mIndices = new Vector2Int[mX, mY];

                // create all the grid cells (location data) with default values.
                for (int i = 0; i < mX; ++i)
                {
                    for (int j = 0; j < mY; ++j)
                    {
                        LocationData<Vector2Int> data = new LocationData<Vector2Int>();
                        data.Cost = 1.0f;
                        data.IsWalkable = true;

                        mIndices[i,j] = new Vector2Int(i, j); 
                        data.Location = mIndices[i, j];

                        mLocations.Add(data.Location, data);
                    }
                }                
            }

            public Vector2Int GetCell(int i, int j)
            {
                return mIndices[i, j];
            }

            // Get the neighbours. This method must be implemented for 
            // any type of grid that you create. For a rectangular
            // grid it is getting the 8 adjacent indices
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

                    Vector2Int v = mIndices[i, j];

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

                    Vector2Int v = mIndices[i,j];

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

                    Vector2Int v = mIndices[i,j];

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

                    Vector2Int v = mIndices[i, j];

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

                    Vector2Int v = mIndices[i, j];

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

                    Vector2Int v = mIndices[i,j];

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

                    Vector2Int v = mIndices[i, j];

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

                    Vector2Int v = mIndices[i,j];

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
