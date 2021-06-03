using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathFinder
{
    public class AStarPathFinder
    {
        public Grid mGrid { get; set; }

        #region Delegates for Action callbacks
        // Some callbacks to handle on changes to the internal values.
        // these callbacks can be used by the game to display visually 
        // changes to the cells and lists.
        public delegate void DelegateOnChangeCurrentNode(Node node);
        public DelegateOnChangeCurrentNode onChangeCurrentNode;
        public delegate void DelegateOnAddToOpenList(Node node);
        public DelegateOnChangeCurrentNode onAddToOpenList;
        public delegate void DelegateOnAddToClosedList(Node node);
        public DelegateOnChangeCurrentNode onAddToClosedList;
        public delegate void DelegateOnDestinationFound(Node node);
        public DelegateOnChangeCurrentNode onDestinationFound;
        #endregion

        private Grid.GridCell mStartCell;
        private Grid.GridCell mGoalCell;

        public enum PathFinderStatus
        {
            NOT_STARTED,
            SUCCESS,
            FAILURE,
            RUNNING,
        }

        public class Node
        {
            public Node Parent { get; private set; }
            public Grid.GridCell Cell { get; private set; }
            public float Fcost { get; private set; }
            public float GCost { get; private set; }
            public float Hcost { get; private set; }

            public Node(Grid.GridCell cell, Node parent, float gCost, float hCost)
            {
                Cell = cell;
                Parent = parent;
                Hcost = hCost;
                GCost = gCost;
                Fcost = hCost + gCost;
            }
        }

        #region The open and closed Lists.
        private List<Node> mOList = new List<Node>();
        private List<Node> mCList = new List<Node>();

        // A helper method to find the least cost node from a list,
        // remove it and return the node.
        private Node GetRemoveLeastCostElement(List<Node> myList)
        {
            // Find the hightest priority.
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

            Node n = myList[best_index];
            myList.RemoveAt(best_index);

            return n;
        }

        bool IsCellInList(List<Node> myList, Grid.GridCell cell)
        {
            for(int i = 0; i < myList.Count; ++i)
            {
                if (cell == myList[i].Cell) return true;
            }
            return false;
        }

        private Node mRoot = null;
        #endregion

        public PathFinderStatus Status { get; private set; } = PathFinderStatus.NOT_STARTED;

        public void SearchInitialize(int startx, int starty, int endx, int endy)
        {
            if(Status == PathFinderStatus.RUNNING)
            {
                Debug.Log("Path finder already running.");
                return;
            }

            mOList.Clear();
            mCList.Clear();

            mStartCell = mGrid.GetCell(startx, starty);
            mGoalCell = mGrid.GetCell(endx, endy);

            mRoot = new Node(mStartCell, null, 0f, Grid.GetManhattanCost(mStartCell, mGoalCell));
            mOList.Add(mRoot);

            currentNode = mRoot;

            Status = PathFinderStatus.RUNNING;
        }

        Node currentNode = null;
        public PathFinderStatus SearchStep()
        {
            // Add the current node to the closed list.
            mCList.Add(currentNode);
            onAddToClosedList?.Invoke(currentNode);

            // Get the least cost element from the open list. 
            currentNode = GetRemoveLeastCostElement(mOList);
            onChangeCurrentNode?.Invoke(currentNode);

            // Check if the node contains the Goal cell.
            if(currentNode.Cell == mGoalCell)
            {
                Debug.Log("Found destination.");
                Status = PathFinderStatus.SUCCESS;
                onDestinationFound?.Invoke(currentNode);
                return Status;
            }

            // find the neighbours.
            List<Grid.GridCell> neighbours = mGrid.GetNeighbours(currentNode.Cell);

            foreach(Grid.GridCell cell in neighbours)
            {
                // first of all check if the node is already in the closedlist.
                // if so then we do not need to continue search for this node.
                if (!IsCellInList(mCList, cell))
                {
                    if (!IsCellInList(mOList, cell))
                    {
                        // we will add the cell to the open list.
                        // to do that we will first have to calculate the G and the H.
                        // 
                        // Remember G is the cost from the start till now.
                        // So to get G we will get the G cost of the currentNode
                        // and add 1 (since we are only travelling on sideways).
                        // We can actually implement a function to calculate the cost 
                        // between two adjacent cells. 
                        // For now we will just add 1.0f

                        float G = currentNode.GCost + /*1.0f;*/Grid.GetCostBetweenTwoCells(currentNode.Cell, cell);
                        float H = Grid.GetManhattanCost(cell, mGoalCell);
                        Node n = new Node(cell, currentNode, G, H);
                        mOList.Add(n);
                        onAddToOpenList?.Invoke(n);
                    }
                }
            }

            return PathFinderStatus.RUNNING;
        }
    }
}