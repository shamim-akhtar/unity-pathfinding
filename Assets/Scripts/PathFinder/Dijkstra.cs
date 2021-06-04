using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameAI
{
    namespace PathFinding
    {
        public class DijkstraPathFinder<T> : PathFinder<T>
        {
            protected override void AlgorithmSpecificImplementation(T cell)
            {
                if (IsInList(mClosedList, cell) == -1)
                {
                    float G = CurrentNode.GCost + GCostFunction(CurrentNode.Location, cell);
                    float H = 0.0f;// HCostFunction(cell, Goal);
                    //Dijkstra doesn't include the Heuristic cost

                    // Check if the cell is already there in the open list.
                    int idOList = IsInList(mOpenList, cell);
                    if (idOList == -1)
                    {
                        // The cell does not exist in the open list.
                        // We will add the cell to the open list.

                        PathFinderNode<T> n = new PathFinderNode<T>(cell, CurrentNode, G, H);
                        mOpenList.Add(n);
                        onAddToOpenList?.Invoke(n);
                    }
                    else
                    {
                        // if the cell exists in the openlist then check if the G cost is less than the 
                        // one already in the list.
                        float oldG = mOpenList[idOList].GCost;
                        if (G < oldG)
                        {
                            // change the parent and update the cost to the new G
                            mOpenList[idOList].Parent = CurrentNode;
                            mOpenList[idOList].SetGCost(G);
                            onAddToOpenList?.Invoke(mOpenList[idOList]);
                        }
                    }
                }
            }
        }
    }
}