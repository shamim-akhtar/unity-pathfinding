using System;
using System.Collections;
using System.Collections.Generic;

namespace PathFinder
{
    public class Grid
    {
        protected int mX;
        protected int mY;

        #region Class GridCell
        public class GridCell : IEquatable<GridCell>
        {
            public int X { get; private set; }
            public int Y { get; private set; }

            public bool Walkable { get; set; }

            public GridCell(int x, int y)
            {
                this.X = x;
                this.Y = y;
                Walkable = true;
            }

            public override bool Equals(object obj) => this.Equals(obj as GridCell);

            public bool Equals(GridCell p)
            {
                if (p is null)
                {
                    return false;
                }

                // Optimization for a common success case.
                if (System.Object.ReferenceEquals(this, p))
                {
                    return true;
                }

                // If run-time types are not exactly the same, return false.
                if (this.GetType() != p.GetType())
                {
                    return false;
                }

                // Return true if the fields match.
                // Note that the base class is not invoked because it is
                // System.Object, which defines Equals as reference equality.
                return (X == p.X) && (Y == p.Y);
            }

            public override int GetHashCode() => (X, Y).GetHashCode();

            public static bool operator ==(GridCell lhs, GridCell rhs)
            {
                if (lhs is null)
                {
                    if (rhs is null)
                    {
                        return true;
                    }

                    // Only the left side is null.
                    return false;
                }
                // Equals handles case of null on right side.
                return lhs.Equals(rhs);
            }

            public static bool operator !=(GridCell lhs, GridCell rhs) => !(lhs == rhs);
        }
        #endregion

        protected GridCell[,] mIndices;

        #region Properties
        public int Cols { get { return mX; } }
        public int Rows { get { return mY; } }
        #endregion


        public Grid(int numX, int numY)
        {
            mX = numX;
            mY = numY;

            mIndices = new GridCell[mX, mY];
            for(int i = 0; i < mX; ++i)
            {
                for(int j = 0; j < mY; ++j)
                {
                    mIndices[i, j] = new GridCell(i, j);
                }
            }
        }

        public GridCell GetCell(int x, int y)
        {
            if (x < 0 || x >= mX || y < 0 || y >= mY) 
                return null;
            return mIndices[x, y];
        }

        public List<GridCell> GetNeighbours(GridCell cell)
        {
            return GetNeighbours(cell.X, cell.Y);
        }

        // Return the walkable neighbours for the given grid index.
        public List<GridCell> GetNeighbours(int x, int y)
        {
            List<GridCell> neighbours = new List<GridCell>();

            // Check up.
            if (y < mY - 1)
            {
                int i = x;
                int j = y + 1;
                if (mIndices[i, j].Walkable)
                {
                    neighbours.Add(mIndices[i, j]);
                }
            }
            // Check top-right
            if (y < mY -1 && x < mX - 1)
            {
                int i = x + 1;
                int j = y + 1;
                if (mIndices[i, j].Walkable)
                {
                    neighbours.Add(mIndices[i, j]);
                }
            }
            // Check right
            if (x < mX - 1)
            {
                int i = x + 1;
                int j = y;
                if (mIndices[i, j].Walkable)
                {
                    neighbours.Add(mIndices[i, j]);
                }
            }
            // Check right-down
            if (x < mX - 1 && y > 0)
            {
                int i = x + 1;
                int j = y - 1;
                if (mIndices[i, j].Walkable)
                {
                    neighbours.Add(mIndices[i, j]);
                }
            }
            // Check down
            if (y > 0)
            {
                int i = x;
                int j = y - 1;
                if (mIndices[i, j].Walkable)
                {
                    neighbours.Add(mIndices[i, j]);
                }
            }
            // Check down-left
            if (y > 0 && x > 0)
            {
                int i = x - 1;
                int j = y - 1;
                if (mIndices[i, j].Walkable)
                {
                    neighbours.Add(mIndices[i, j]);
                }
            }
            // Check left
            if (x > 0)
            {
                int i = x - 1;
                int j = y;
                if (mIndices[i, j].Walkable)
                {
                    neighbours.Add(mIndices[i, j]);
                }
            }
            // Check left-top
            if (x > 0 && y < mY - 1)
            {
                int i = x - 1;
                int j = y + 1;
                if (mIndices[i, j].Walkable)
                {
                    neighbours.Add(mIndices[i, j]);
                }
            }

            return neighbours;
        }

        public static int GetManhattanCost(GridCell a, GridCell b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        public static float GetCostBetweenTwoCells(GridCell a, GridCell b)
        {
            return (float)Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }
    }
}