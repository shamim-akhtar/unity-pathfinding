using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GameAI
{
    namespace PathFinding
    {
        public class Node<T>
        {
            // Private member-variables
            private T data;
            private List<Node<T>> neighbors = null;

            public Node() { }
            public Node(T data) : this(data, null) { }
            public Node(T data, List<Node<T>> neighbors)
            {
                this.data = data;
                this.neighbors = neighbors;
            }

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

            public List<Node<T>> Neighbors
            {
                get
                {
                    return neighbors;
                }
                set
                {
                    neighbors = value;
                }
            }
        }

        public class GraphNode<T> : Node<T>
        {
            private List<int> costs;

            public GraphNode() : base() { }
            public GraphNode(T value) : base(value) { }
            public GraphNode(T value, List<Node<T>> neighbors) : base(value, neighbors) { }

            new public List<Node<T>> Neighbors
            {
                get
                {
                    if (base.Neighbors == null)
                        base.Neighbors = new List<Node<T>>();

                    return base.Neighbors;
                }
            }

            public List<int> Costs
            {
                get
                {
                    if (costs == null)
                        costs = new List<int>();

                    return costs;
                }
            }
        }

        public class Graph<T> : IMap<Node<T>>
        {
            private List<Node<T>> nodeSet;

            public Graph() : this(null) { }
            public Graph(List<Node<T>> nodeSet)
            {
                if (nodeSet == null)
                    this.nodeSet = new List<Node<T>>();
                else
                    this.nodeSet = nodeSet;
            }

            public void AddNode(GraphNode<T> node)
            {
                // adds a node to the graph
                nodeSet.Add(node);
            }

            public void AddNode(T value)
            {
                // adds a node to the graph
                nodeSet.Add(new GraphNode<T>(value));
            }

            public void AddDirectedEdge(GraphNode<T> from, GraphNode<T> to, int cost)
            {
                from.Neighbors.Add(to);
                from.Costs.Add(cost);
            }

            public void AddUndirectedEdge(GraphNode<T> from, GraphNode<T> to, int cost)
            {
                from.Neighbors.Add(to);
                from.Costs.Add(cost);

                to.Neighbors.Add(from);
                to.Costs.Add(cost);
            }

            public static Node<T> FindByValue(List<Node<T>> nodes, T value)
            {
                // search the list for the value
                foreach (Node<T> node in nodes)
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

            public List<Node<T>> Nodes
            {
                get
                {
                    return nodeSet;
                }
            }

            public int Count
            {
                get { return nodeSet.Count; }
            }
            public List<Node<T>> GetNeighbours(Node<T> loc)
            {
                return loc.Neighbors;
            }
        }
    }
}
