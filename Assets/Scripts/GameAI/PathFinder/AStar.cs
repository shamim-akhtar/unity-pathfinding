using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameAI
{
    namespace PathFinding
    {
        public class AStarPathFinder<T> : PathFinder<T>
        {
            protected override void AlgorithmSpecificImplementation(Node<T> cell)
            {
                // first of all check if the node is already in the closedlist.
                // if so then we do not need to continue search for this node.
                if (IsInList(mClosedList, cell.Value) == -1)
                {
                    // The cell does not exist in the closed list.

                    // Calculate the cost of the node from its parent.
                    // Remember G is the cost from the start till now.
                    // So to get G we will get the G cost of the currentNode
                    // and add the cost from currentNode to this cell.
                    // We can actually implement a function to calculate the cost 
                    // between two adjacent cells. 

                    float G = CurrentNode.GCost + NodeTraversalCost(CurrentNode.Location.Value, cell.Value);
                    float H = HeuristicCost(cell.Value, Goal.Value);

                    // Check if the cell is already there in the open list.
                    int idOList = IsInList(mOpenList, cell.Value);
                    if (idOList == -1)
                    {
                        // The cell does not exist in the open list.
                        // We will add the cell to the open list.

                        PathFinderNode n = new PathFinderNode(cell, CurrentNode, G, H);
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