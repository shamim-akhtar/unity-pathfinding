using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameAI
{
    namespace PathFinding
    {
        public class PathFinderNode<T>
        {
            public PathFinderNode<T> Parent { get; set; }
            public T Location { get; private set; }
            public float Fcost { get; private set; }
            public float GCost { get; private set; }
            public float Hcost { get; private set; }

            public PathFinderNode(T location, PathFinderNode<T> parent, float gCost, float hCost)
            {
                Location = location;
                Parent = parent;
                Hcost = hCost;
                SetGCost(gCost);
            }

            public void SetGCost(float c)
            {
                GCost = c;
                Fcost = GCost + Hcost;
            }
        }
        public class PathFinder<T>
        {
            #region Delegates for Action callbacks
            // Some callbacks to handle on changes to the internal values.
            // these callbacks can be used by the game to display visually the
            // changes to the cells and lists.
            public delegate void DelegateOnChangeCurrentNode(PathFinderNode<T> node);
            public DelegateOnChangeCurrentNode onChangeCurrentNode;
            public delegate void DelegateOnAddToOpenList(PathFinderNode<T> node);
            public DelegateOnChangeCurrentNode onAddToOpenList;
            public delegate void DelegateOnAddToClosedList(PathFinderNode<T> node);
            public DelegateOnChangeCurrentNode onAddToClosedList;
            public delegate void DelegateOnDestinationFound(PathFinderNode<T> node);
            public DelegateOnChangeCurrentNode onDestinationFound;
            #endregion

            public enum PathFinderStatus
            {
                NOT_STARTED,
                SUCCESS,
                FAILURE,
                RUNNING,
            }

            public PathFinderStatus Status { get; private set; } = PathFinderStatus.NOT_STARTED;

            #region The Open and Closed lists and associated functions
            // The open list for the path finder.
            protected List<PathFinderNode<T>> mOpenList = new List<PathFinderNode<T>>();

            // The closed list
            protected List<PathFinderNode<T>> mClosedList = new List<PathFinderNode<T>>();

            // A helper method to find the least cost node from a list
            private PathFinderNode<T> GetLeastCostNode(List<PathFinderNode<T>> myList)
            {
                int best_index = 0;
                float best_priority = myList[0].Fcost;
                for (int i = 1; i < myList.Count; i++)
                {
                    if (best_priority > myList[i].Fcost)
                    {
                        best_priority = myList[i].Fcost;
                        best_index = i;
                    }
                }

                PathFinderNode<T> n = myList[best_index];
                return n;
            }

            int IsInList(List<PathFinderNode<T>> myList, T cell)
            {
                for (int i = 0; i < myList.Count; ++i)
                {
                    if (EqualityComparer<T>.Default.Equals(myList[i].Location, cell))
                        return i;
                }
                return -1;
            }
            #endregion

            public void Reset()
            {
                if(Status == PathFinderStatus.RUNNING)
                {
                    // Cannot reset path finder. Path finding in progress.
                    return;
                }

                mOpenList.Clear();
                mClosedList.Clear();
            }

            public T Start { get; private set; }
            public T Goal { get; private set; }

            private PathFinderNode<T> mRoot;
            public PathFinderNode<T> CurrentNode { get; private set; }

            public delegate float CostFunction(T a, T b);
            public CostFunction HCostFunction { get; set; }
            public void SetHeuristicCostFunction(CostFunction cf)
            {
                HCostFunction = cf;
            }
            public CostFunction GCostFunction { get; set; }
            public void SetGCostFunction(CostFunction cf)
            {
                GCostFunction = cf;
            }

            public Map<T> Map { get; private set; }

            // Initialize a new search.
            // Note that a search can only be initialized if 
            // the path finder is not already running.
            // call Reset before initializing a new search.
            public void Initialize(Map<T> map, T start, T goal)
            {
                if (Status == PathFinderStatus.RUNNING)
                {
                    // Cannot reset path finder. Path finding in progress.
                    return;
                }

                Map = map;
                Start = start;
                Goal = goal;

                float H = HCostFunction(Start, Goal);
                mRoot = new PathFinderNode<T>(Start, null, 0f, H);
                mOpenList.Add(mRoot);

                CurrentNode = mRoot;
                onChangeCurrentNode?.Invoke(CurrentNode);

                Status = PathFinderStatus.RUNNING;
            }

            // take a search step.
            public PathFinderStatus Step()
            {
                // Add the current node to the closed list.
                mClosedList.Add(CurrentNode);

                // Call the delegate to inform any subscribers.
                onAddToClosedList?.Invoke(CurrentNode);

                // Get the least cost element from the open list. 
                // This becomes our new current node.
                CurrentNode = GetLeastCostNode(mOpenList);

                // Call the delegate to inform any subscribers.
                onChangeCurrentNode?.Invoke(CurrentNode);

                // Remove the node from the open list.
                mOpenList.Remove(CurrentNode);

                // Check if the node contains the Goal cell.
                if (EqualityComparer<T>.Default.Equals(CurrentNode.Location, Goal))
                {
                    Debug.Log("Found destination.");
                    Status = PathFinderStatus.SUCCESS;
                    onDestinationFound?.Invoke(CurrentNode);
                    return Status;
                }

                // find the neighbours.
                List<T> neighbours = Map.GetNeighbours(CurrentNode.Location);

                foreach (T cell in neighbours)
                {
                    // first of all check if the node is already in the closedlist.
                    // if so then we do not need to continue search for this node.
                    if (IsInList(mClosedList, cell) == -1)
                    {
                        // The cell does not exist in the closed list.

                        // Calculate the cost of the node from its parent.
                        // Remember G is the cost from the start till now.
                        // So to get G we will get the G cost of the currentNode
                        // and add the cost from currentNode to this cell.
                        // We can actually implement a function to calculate the cost 
                        // between two adjacent cells. 

                        float G = CurrentNode.GCost + GCostFunction(CurrentNode.Location, cell);
                        float H = HCostFunction(cell, Goal);

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

                Status = PathFinderStatus.RUNNING;
                return Status;
            }
        }
    }
}