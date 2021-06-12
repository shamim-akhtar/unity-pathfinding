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

        public enum PathFinderStatus
        {
            NOT_STARTED,
            SUCCESS,
            FAILURE,
            RUNNING,
        }

        abstract public class Node<T>
        {
            public T Value { get; private set; }
            public Node(T value)
            {
                Value = value;
            }

            abstract public List<Node<T>> GetNeighbours();

        }

        abstract public class PathFinder<T>
        {
            public class PathFinderNode
            {
                public PathFinderNode Parent { get; set; }
                public Node<T> Location { get; private set; }
                public float Fcost { get; private set; }
                public float GCost { get; private set; }
                public float Hcost { get; private set; }

                public PathFinderNode(Node<T> location, PathFinderNode parent, float gCost, float hCost)
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

            #region Delegates for Action callbacks
            // Some callbacks to handle on changes to the internal values.
            // these callbacks can be used by the game to display visually the
            // changes to the cells and lists.
            public delegate void DelegateOnChangeCurrentNode(PathFinderNode node);
            public DelegateOnChangeCurrentNode onChangeCurrentNode;
            public delegate void DelegateOnAddToOpenList(PathFinderNode node);
            public DelegateOnChangeCurrentNode onAddToOpenList;
            public delegate void DelegateOnAddToClosedList(PathFinderNode node);
            public DelegateOnChangeCurrentNode onAddToClosedList;
            public delegate void DelegateOnDestinationFound(PathFinderNode node);
            public DelegateOnChangeCurrentNode onDestinationFound;
            public delegate void DelegateNoArgument();
            public DelegateNoArgument onStarted;
            public DelegateNoArgument onRunning;
            public DelegateNoArgument onFailure;
            public DelegateNoArgument onSuccess;
            #endregion

            public PathFinderStatus Status { get; private set; } = PathFinderStatus.NOT_STARTED;

            #region The Open and Closed lists and associated functions
            // The open list for the path finder.
            protected List<PathFinderNode> mOpenList = new List<PathFinderNode>();

            // The closed list
            protected List<PathFinderNode> mClosedList = new List<PathFinderNode>();

            // A helper method to find the least cost node from a list
            protected PathFinderNode GetLeastCostNode(List<PathFinderNode> myList)
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

                PathFinderNode n = myList[best_index];
                return n;
            }

            protected int IsInList(List<PathFinderNode> myList, T cell)
            {
                for (int i = 0; i < myList.Count; ++i)
                {
                    if (EqualityComparer<T>.Default.Equals(myList[i].Location.Value, cell))
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

                CurrentNode = null;

                mOpenList.Clear();
                mClosedList.Clear();

                Status = PathFinderStatus.NOT_STARTED;
            }

            public Node<T> Start { get; private set; }
            public Node<T> Goal { get; private set; }

            public PathFinderNode CurrentNode { get; private set; }

            public delegate float CostFunction(T a, T b);
            public CostFunction HCostFunction { get; set; }
            public CostFunction GCostFunction { get; set; }

            // Initialize a new search.
            // Note that a search can only be initialized if 
            // the path finder is not already running.
            // call Reset before initializing a new search.
            public void Initialize(Node<T> start, Node<T> goal)
            {
                if (Status == PathFinderStatus.RUNNING)
                {
                    // Cannot reset path finder. Path finding in progress.
                    return;
                }

                Start = start;
                Goal = goal;

                float H = HCostFunction(Start.Value, Goal.Value);
                PathFinderNode root = new PathFinderNode(Start, null, 0f, H);
                mOpenList.Add(root);

                CurrentNode = root;
                onChangeCurrentNode?.Invoke(CurrentNode);
                onStarted?.Invoke();
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
                    onFailure?.Invoke();
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
                if (EqualityComparer<T>.Default.Equals(CurrentNode.Location.Value, Goal.Value))
                {
                    Debug.Log("Found destination.");
                    Status = PathFinderStatus.SUCCESS;
                    onDestinationFound?.Invoke(CurrentNode);
                    onSuccess?.Invoke();
                    return Status;
                }

                // find the neighbours.
                List<Node<T>> neighbours = CurrentNode.Location.GetNeighbours();

                foreach (Node<T> cell in neighbours)
                {
                    AlgorithmSpecificImplementation(cell);
                }

                Status = PathFinderStatus.RUNNING;
                onRunning?.Invoke();
                return Status;
            }

            abstract protected void AlgorithmSpecificImplementation(Node<T> cell);
        }
    }
}