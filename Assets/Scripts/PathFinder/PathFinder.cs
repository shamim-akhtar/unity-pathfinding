using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameAI
{
    namespace PathFinding
    {
        public enum PathFindingAlgorithm
        {
            AStar,
            Dijkstra,
            Greedy_Best_First,
        }

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
        abstract public class PathFinder<T>
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
            protected PathFinderNode<T> GetLeastCostNode(List<PathFinderNode<T>> myList)
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

            protected int IsInList(List<PathFinderNode<T>> myList, T cell)
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

            public IMap<T> Map { get; private set; }

            // Initialize a new search.
            // Note that a search can only be initialized if 
            // the path finder is not already running.
            // call Reset before initializing a new search.
            public void Initialize(IMap<T> map, T start, T goal)
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

                if(mOpenList.Count == 0)
                {
                    // we have exhausted our search. No solution is found.
                    Status = PathFinderStatus.FAILURE;
                    return Status;
                }

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
                    AlgorithmSpecificImplementation(cell);
                }

                Status = PathFinderStatus.RUNNING;
                return Status;
            }

            abstract protected void AlgorithmSpecificImplementation(T cell);
        }
    }
}