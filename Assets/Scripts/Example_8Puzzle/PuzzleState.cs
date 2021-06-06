using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Puzzle
{
    public class PuzzleState : IEquatable<PuzzleState>
    {
        public int[] Arr
        {
            get;
            private set;
        }
        public int NumRowsOrCols
        {
            get;
        }
        private int _emptyTileIndex;

        public int GetEmptyTileIndex()
        {
            return _emptyTileIndex;
        }

        public PuzzleState(int rows_or_cols)
        {
            NumRowsOrCols = rows_or_cols;
            Arr = new int[NumRowsOrCols * NumRowsOrCols];
            for (int i = 0; i < Arr.Length; ++i)
            {
                Arr[i] = i;
            }
            _emptyTileIndex = Arr.Length - 1;
        }

        public PuzzleState(int[] arr)
        {
            NumRowsOrCols = (int)System.Math.Sqrt(arr.Length);

            Arr = new int[NumRowsOrCols * NumRowsOrCols];
            for (int i = 0; i < Arr.Length; ++i)
            {
                Arr[i] = arr[i];
                if (arr[i] == (Arr.Length - 1)) _emptyTileIndex = i;
            }
        }

        public PuzzleState(PuzzleState other)
        {
            NumRowsOrCols = other.NumRowsOrCols;
            _emptyTileIndex = other._emptyTileIndex;
            Arr = new int[NumRowsOrCols * NumRowsOrCols];
            other.Arr.CopyTo(Arr, 0);
        }

        public static bool Equals(PuzzleState a, PuzzleState b)
        {
            for (int i = 0; i < a.Arr.Length; i++)
            {
                if (a.Arr[i] != b.Arr[i]) return false;
            }
            return true;
        }

        public bool Equals(PuzzleState other)
        {
            if (other is null)
                return false;

            return Equals(this, other);
        }

        public override bool Equals(object obj) => Equals(obj as PuzzleState);
        public override int GetHashCode()
        {
            int hc = Arr.Length;
            foreach (int val in Arr)
            {
                hc = unchecked(hc * 314159 + val);
            }
            return hc;
        }

        public int FindEmptyTileIndex()
        {
            for (int i = 0; i < Arr.Length; i++)
                if (Arr[i] == Arr.Length - 1) return i;
            return Arr.Length;
        }

        public void SwapWithEmpty(int index)
        {
            int tmp = Arr[index];
            Arr[index] = Arr[_emptyTileIndex];
            Arr[_emptyTileIndex] = tmp;
            _emptyTileIndex = index;
        }

     //   public void Randomize(int depth = 4)
     //   {
     //       //System.Random rnd = new System.Random();
     //       //Arr = Arr.OrderBy(x => rnd.Next()).ToArray();
     //       //FindEmptyTileIndex();
     //   }

     //   public void RandomizeSolvable()
     //   {
     //       Randomize();
     //       while(!IsSolvable(this))
     //       {
     //           Randomize();
     //       }
     //       FindEmptyTileIndex();
     //   }

     //   public static PuzzleState RandomSolvablePuzzle(int rows)
     //   {
     //       PuzzleState p = new PuzzleState(rows);
     //       p.RandomizeSolvable();
     //       return p;
     //   }

     //   public static bool IsSolvable(PuzzleState state)
	    //{
		   // int inv_count = 0;
		   // for (int i = 0; i< state.Arr.Length - 1; i++)
			  //  for (int j = i + 1; j< state.Arr.Length; j++)
				 //   if ((state.Arr[j] > 0) && (state.Arr[i] > 0) && state.Arr[i] > state.Arr[j])
					//    inv_count++;
		   // return (inv_count % 2 == 0);
	    //}

        public int GethammingCost()
        {
            int cost = 0;
            for (int i = 0; i < Arr.Length; ++i)
            {
                if (Arr[i] == Arr.Length - 1) continue;
                if (Arr[i] != i + 1) cost += 1;
            }
            return cost;
        }

        public int GetManhattanCost()
        {
            int cost = 0;
            for (int i = 0; i < Arr.Length; ++i)
            {
                int v = Arr[i];
                if (v == Arr.Length - 1) continue;

                int gx = v % NumRowsOrCols;
                int gy = v / NumRowsOrCols;

                int x = i % NumRowsOrCols;
                int y = i / NumRowsOrCols;

                int mancost = System.Math.Abs(x - gx) + System.Math.Abs(y - gy);
                cost += mancost;
            }
            return cost;
        }
    };
}
