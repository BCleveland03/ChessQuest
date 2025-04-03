using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TopDownGame
{
    public class AStarManager : MonoBehaviour
    {
        // Outlets
        public static AStarManager instance;

        //Methods
        private void Awake()
        {
            instance = this;
        }

        public List<Node> GeneratePath(Node start, Node end, bool eightDir)
        {
            List<Node> openSet = new List<Node>();

            foreach (Node node in FindObjectsOfType<Node>())
            {
                node.gScore = float.MaxValue;
            }

            start.gScore = 0;
            start.hScore = Vector2.Distance(start.transform.position, end.transform.position);
            openSet.Add(start);

            while (openSet.Count > 0)
            {
                int lowestF = default;

                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].FScore() < openSet[lowestF].FScore())
                    {
                        lowestF = i;
                    }
                }

                Node currentNode = openSet[lowestF];
                openSet.Remove(currentNode);

                if (currentNode == end)
                {
                    List<Node> path = new List<Node>();

                    path.Insert(0, end);

                    while (currentNode != start)
                    {
                        currentNode = currentNode.cameFrom;
                        path.Add(currentNode);
                    }

                    path.Reverse();
                    return path;
                }

                if (eightDir)
                {
                    foreach (Node connectedNode in currentNode.eightDirectionConnections)
                    {
                        float heldGScore = currentNode.gScore + Vector2.Distance(currentNode.transform.position, connectedNode.transform.position);

                        if (heldGScore < connectedNode.gScore)
                        {
                            connectedNode.cameFrom = currentNode;
                            connectedNode.gScore = heldGScore;
                            connectedNode.hScore = Vector2.Distance(connectedNode.transform.position, end.transform.position);

                            if (!openSet.Contains(connectedNode))
                            {
                                openSet.Add(connectedNode);
                            }
                        }
                    }
                }
                else
                {
                    foreach (Node connectedNode in currentNode.fourDirectionConnections)
                    {
                        float heldGScore = currentNode.gScore + Vector2.Distance(currentNode.transform.position, connectedNode.transform.position);

                        if (heldGScore < connectedNode.gScore)
                        {
                            connectedNode.cameFrom = currentNode;
                            connectedNode.gScore = heldGScore;
                            connectedNode.hScore = Vector2.Distance(connectedNode.transform.position, end.transform.position);

                            if (!openSet.Contains(connectedNode))
                            {
                                openSet.Add(connectedNode);
                            }
                        }
                    }
                }
            }

            return null;
        }

        public Node FindNearestNode(Vector2 nearPosition)
        {
            Node foundNode = null;
            float minDistance = float.MaxValue;

            foreach (Node node in NodesInScene())
            {
                float currentDistance = Vector2.Distance(nearPosition, node.transform.position);
                if (currentDistance < minDistance)
                {
                    minDistance = currentDistance;
                    foundNode = node;
                }
            }

            return foundNode;
        }

        public Node FindFurthestNode(Vector2 farPosition)
        {
            Node foundNode = null;
            float maxDistance = 0;

            foreach (Node node in NodesInScene())
            {
                float currentDistance = Vector2.Distance(farPosition, node.transform.position);
                if (currentDistance > maxDistance)
                {
                    maxDistance = currentDistance;
                    foundNode = node;
                }
            }

            return foundNode;
        }

        public Node[] NodesInScene()
        {
            return FindObjectsOfType<Node>();
        }
    }
}
