using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace GameAI
{
    namespace PathFinding
    {
        public class RectGridCell
        {
            public Vector2Int Index { get; set; }
            public bool IsWalkable { get; set; }
            public float Cost { get; set; }
            public RectGridCell()
            { }
        }

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
        public class RectGridMap : IMap<RectGridCell>
        {
            // the max number of colums in the grid.
            protected int mX;
            // the max number of rows in the grid
            protected int mY;

            // the 2d array of Vecto2Int.
            // This stucture stores the 2d indices of the grid cells.
            protected Vector2Int[,] mIndices;
            protected RectGridCell[,] mMapCell;

            public int Cols { get { return mX; } }
            public int Rows { get { return mY; } }

            public int NumX { get { return mX; } }
            public int NumY { get { return mY; } }

            // Construct a grid with the max cols and rows.
            public RectGridMap(int numX, int numY)
            {
                mX = numX;
                mY = numY;

                mIndices = new Vector2Int[mX, mY];
                mMapCell = new RectGridCell[mX, mY];

                // create all the grid cells (Index data) with default values.
                for (int i = 0; i < mX; ++i)
                {
                    for (int j = 0; j < mY; ++j)
                    {
                        RectGridCell data = new RectGridCell();
                        data.Cost = 1.0f;
                        data.IsWalkable = true;

                        mIndices[i,j] = new Vector2Int(i, j);
                        mMapCell[i, j] = data;
                        data.Index = mIndices[i, j];
                    }
                }                
            }

            // static method to save the map to a file.
            public static void Save(RectGridMap map, string filename)
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Create(Application.persistentDataPath + "/" + filename);

                try
                {
                    bf.Serialize(file, map.mX);
                    bf.Serialize(file, map.mY);
                    for (int i = 0; i < map.mX; ++i)
                    {
                        for (int j = 0; j < map.mY; ++j)
                        {
                            RectGridCell data = map.mMapCell[i, j];

                            bf.Serialize(file, data.Cost);
                            bf.Serialize(file, data.IsWalkable);
                        }
                    }
                }
                catch (SerializationException e)
                {
                    Debug.Log("Failed to save map. Reason: " + e.Message);
                    throw;
                }
                finally
                {
                    file.Close();
                }
            }

            // static method to load a map from a file.
            public static RectGridMap Load(string filen)
            {
                string filename = Application.persistentDataPath + "/" + filen;
                RectGridMap map = null;
                if (!File.Exists(filename))
                    return null;

                BinaryFormatter bf = new BinaryFormatter();
                using (FileStream file = new FileStream(filename, FileMode.Open))
                {
                    try
                    {
                        int mX = 0;
                        int mY = 0;
                        mX = (int)bf.Deserialize(file);
                        mY = (int)bf.Deserialize(file);

                        map = new RectGridMap(mX, mY);

                        for (int i = 0; i < map.mX; ++i)
                        {
                            for (int j = 0; j < map.mY; ++j)
                            {
                                RectGridCell data = map.mMapCell[i, j];

                                data.Cost = (float)bf.Deserialize(file);
                                data.IsWalkable = (bool)bf.Deserialize(file);
                            }
                        }
                    }
                    catch (SerializationException e)
                    {
                        Debug.Log("Failed to load map. Reason: " + e.Message);
                        map = null;
                    }
                    finally
                    {
                        file.Close();
                    }
                }
                return map;
            }

            public RectGridCell GetCell(int i, int j)
            {
                return mMapCell[i, j];
            }

            // Get the neighbours. This method must be implemented for 
            // any type of grid that you create. For a rectangular
            // grid it is getting the 8 adjacent indices
            public List<RectGridCell> GetNeighbours(RectGridCell loc)
            {
                List<RectGridCell> neighbours = new List<RectGridCell>();

                int x = loc.Index.x;
                int y = loc.Index.y;

                // Check up.
                if (y < mY - 1)
                {
                    int i = x;
                    int j = y + 1;

                    if (mMapCell[i,j].IsWalkable)
                    {
                        neighbours.Add(mMapCell[i, j]);
                    }
                }
                // Check top-right
                if (y < mY - 1 && x < mX - 1)
                {
                    int i = x + 1;
                    int j = y + 1;

                    if (mMapCell[i, j].IsWalkable)
                    {
                        neighbours.Add(mMapCell[i, j]);
                    }
                }
                // Check right
                if (x < mX - 1)
                {
                    int i = x + 1;
                    int j = y;

                    if (mMapCell[i, j].IsWalkable)
                    {
                        neighbours.Add(mMapCell[i, j]);
                    }
                }
                // Check right-down
                if (x < mX - 1 && y > 0)
                {
                    int i = x + 1;
                    int j = y - 1;

                    if (mMapCell[i, j].IsWalkable)
                    {
                        neighbours.Add(mMapCell[i, j]);
                    }
                }
                // Check down
                if (y > 0)
                {
                    int i = x;
                    int j = y - 1;

                    if (mMapCell[i, j].IsWalkable)
                    {
                        neighbours.Add(mMapCell[i, j]);
                    }
                }
                // Check down-left
                if (y > 0 && x > 0)
                {
                    int i = x - 1;
                    int j = y - 1;

                    if (mMapCell[i, j].IsWalkable)
                    {
                        neighbours.Add(mMapCell[i, j]);
                    }
                }
                // Check left
                if (x > 0)
                {
                    int i = x - 1;
                    int j = y;

                    Vector2Int v = mIndices[i, j];

                    if (mMapCell[i, j].IsWalkable)
                    {
                        neighbours.Add(mMapCell[i, j]);
                    }
                }
                // Check left-top
                if (x > 0 && y < mY - 1)
                {
                    int i = x - 1;
                    int j = y + 1;

                    if (mMapCell[i, j].IsWalkable)
                    {
                        neighbours.Add(mMapCell[i, j]);
                    }
                }

                return neighbours;
            }

            public static float GetManhattanCost(RectGridCell a, RectGridCell b)
            {
                return Mathf.Abs(a.Index.x - b.Index.x) + Mathf.Abs(a.Index.y - b.Index.y);
            }

            public static float GetEuclideanCost(RectGridCell a, RectGridCell b)
            {
                return GetCostBetweenTwoCells(a, b);
            }

            public static float GetCostBetweenTwoCells(RectGridCell a, RectGridCell b)
            {
                return Mathf.Sqrt(
                        (a.Index.x - b.Index.x) * (a.Index.x - b.Index.x) + 
                        (a.Index.y - b.Index.y) * (a.Index.y - b.Index.y)
                    );
            }
        }
    }
}
