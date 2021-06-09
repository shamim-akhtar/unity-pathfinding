using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GameAI
{
    namespace PathFinding
    {
        public class GraphNode<T>
        {
            private List<float> costs;
            private T data;
            private List<GraphNode<T>> neighbors = null;

            public T Value
            {
                get
                {
                    return data;
                }
                set
                {
                    data = value;
                }
            }

            public GraphNode() { }
            public GraphNode(T value)
            {
                data = value;
            }
            public GraphNode(T value, List<GraphNode<T>> neighbors)
            {
                data = value;
                this.neighbors = neighbors;
            }

            public List<GraphNode<T>> Neighbors
            {
                get
                {
                    if (neighbors == null)
                        neighbors = new List<GraphNode<T>>();

                    return neighbors;
                }
                set
                {
                    neighbors = value;
                }
            }

            public List<float> Costs
            {
                get
                {
                    if (costs == null)
                        costs = new List<float>();

                    return costs;
                }
            }
        }

        public class Graph<T> : IMap<GraphNode<T>>
        {
            private List<GraphNode<T>> nodeSet;

            #region Delegates
            public delegate void DelegateGraphNode(GraphNode<T> n);
            public delegate void DelegateGraphNode_2(GraphNode<T> a, GraphNode<T> b);
            public DelegateGraphNode mOnAddNode;
            public DelegateGraphNode mOnRemoveNode;
            public DelegateGraphNode_2 mOnAddDirectedEdge;
            #endregion

            public Graph() : this(null) { }
            public Graph(List<GraphNode<T>> nodeSet)
            {
                if (nodeSet == null)
                    this.nodeSet = new List<GraphNode<T>>();
                else
                    this.nodeSet = nodeSet;
            }

            public void AddNode(GraphNode<T> node)
            {
                // adds a node to the graph
                nodeSet.Add(node);
                mOnAddNode?.Invoke(node);
            }

            public void AddNode(T value)
            {
                // adds a node to the graph
                GraphNode<T> n = new GraphNode<T>(value);
                nodeSet.Add(n);
                mOnAddNode?.Invoke(n);
            }

            public void AddDirectedEdge(GraphNode<T> from, GraphNode<T> to, float cost)
            {
                from.Neighbors.Add(to);
                from.Costs.Add(cost);
                mOnAddDirectedEdge?.Invoke(from, to);
            }

            public void AddUndirectedEdge(GraphNode<T> from, GraphNode<T> to, float cost)
            {
                //from.Neighbors.Add(to);
                //from.Costs.Add(cost);

                //to.Neighbors.Add(from);
                //to.Costs.Add(cost);
                AddDirectedEdge(from, to, cost);
                AddDirectedEdge(to, from, cost);
            }

            public static GraphNode<T> FindByValue(List<GraphNode<T>> nodes, T value)
            {
                // search the list for the value
                foreach (GraphNode<T> node in nodes)
                    if (node.Value.Equals(value))
                        return node;

                // if we reached here, we didn't find a matching node
                return null;
            }

            public bool Contains(T value)
            {
                return FindByValue(nodeSet, value) != null;
            }

            public bool Remove(T value)
            {
                // first remove the node from the nodeset
                GraphNode<T> nodeToRemove = (GraphNode<T>)FindByValue(nodeSet, value);
                if (nodeToRemove == null)
                    // node wasn't found
                    return false;

                mOnRemoveNode(nodeToRemove);
                // otherwise, the node was found
                nodeSet.Remove(nodeToRemove);

                // enumerate through each node in the nodeSet, removing edges to this node
                foreach (GraphNode<T> gnode in nodeSet)
                {
                    int index = gnode.Neighbors.IndexOf(nodeToRemove);
                    if (index != -1)
                    {
                        // remove the reference to the node and associated cost
                        gnode.Neighbors.RemoveAt(index);
                        gnode.Costs.RemoveAt(index);
                    }
                }

                return true;
            }

            public List<GraphNode<T>> Nodes
            {
                get
                {
                    return nodeSet;
                }
            }

            public float Count
            {
                get { return nodeSet.Count; }
            }
            public List<GraphNode<T>> GetNeighbours(GraphNode<T> loc)
            {
                return loc.Neighbors;
            }
        }
    }
}
