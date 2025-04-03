using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TopDownGame
{
    public class Node : MonoBehaviour
    {
        //Outlets
        public Node cameFrom;
        /* Needs to be public List<Node> connections */
        public List<Node> fourDirectionConnections;
        public List<Node> eightDirectionConnections;

        // State Tracking
        public int nameToInt;
        public int myRow;
        public int myColumn;

        public float gScore;
        public float hScore;

        // Configuration

        // Methods
        void Start()
        {
            StartCoroutine(EstablishNodeConnections());
        }

        IEnumerator EstablishNodeConnections()
        {
            int gridSize = (2 * NodeController.instance.nodeGridDimensions + 1) ^ 2;

            yield return new WaitForEndOfFrame();

            // Converts name/ID into two separate identifiers used to find adjacent nodes
            nameToInt = int.Parse(NodeController.instance.nodeIdentification[int.Parse(name)]);
            myRow = Mathf.FloorToInt(nameToInt / 100);
            myColumn = nameToInt - (myRow * 100);

            // North adjacent node check
            if (NodeController.instance.nodeIdentification.ContainsKey(nameToInt - 100))
            {
                fourDirectionConnections.Add(NodeController.instance.transform.Find("" + (nameToInt - 100)).GetComponent<Node>());
                eightDirectionConnections.Add(NodeController.instance.transform.Find("" + (nameToInt - 100)).GetComponent<Node>());
            }

            // Northeast adjacent node check (8-dir only)
            if (NodeController.instance.nodeIdentification.ContainsKey(nameToInt - 99))
            {
                eightDirectionConnections.Add(NodeController.instance.transform.Find("" + (nameToInt - 99)).GetComponent<Node>());
            }



            // East adjacent node check
            if (NodeController.instance.nodeIdentification.ContainsKey(nameToInt + 1))
            {
                fourDirectionConnections.Add(NodeController.instance.transform.Find("" + (nameToInt + 1)).GetComponent<Node>());
                eightDirectionConnections.Add(NodeController.instance.transform.Find("" + (nameToInt + 1)).GetComponent<Node>());
            }

            // Southeast adjacent node check (8-dir only)
            if (NodeController.instance.nodeIdentification.ContainsKey(nameToInt + 101))
            {
                eightDirectionConnections.Add(NodeController.instance.transform.Find("" + (nameToInt + 101)).GetComponent<Node>());
            }



            // South adjacent node check
            if (NodeController.instance.nodeIdentification.ContainsKey(nameToInt + 100))
            {
                fourDirectionConnections.Add(NodeController.instance.transform.Find("" + (nameToInt + 100)).GetComponent<Node>());
                eightDirectionConnections.Add(NodeController.instance.transform.Find("" + (nameToInt + 100)).GetComponent<Node>());
            }

            // Southwest adjacent node check
            if (NodeController.instance.nodeIdentification.ContainsKey(nameToInt + 99))
            {
                eightDirectionConnections.Add(NodeController.instance.transform.Find("" + (nameToInt + 99)).GetComponent<Node>());
            }



            // West adjacent node check
            if (NodeController.instance.nodeIdentification.ContainsKey(nameToInt - 1))
            {
                fourDirectionConnections.Add(NodeController.instance.transform.Find("" + (nameToInt - 1)).GetComponent<Node>());
                eightDirectionConnections.Add(NodeController.instance.transform.Find("" + (nameToInt - 1)).GetComponent<Node>());
            }

            // Northwest adjacent node check
            if (NodeController.instance.nodeIdentification.ContainsKey(nameToInt - 101))
            {
                eightDirectionConnections.Add(NodeController.instance.transform.Find("" + (nameToInt - 101)).GetComponent<Node>());
            }

            yield break;
        }

        public float FScore()
        {
            return gScore + hScore;
        }

        private void OnDrawGizmos()
        {
            if (fourDirectionConnections.Count > 0)
            {
                Gizmos.color = Color.yellow;

                for (int i = 0; i < eightDirectionConnections.Count; i++)
                {
                    Gizmos.DrawLine(transform.position, eightDirectionConnections[i].transform.position);
                }

                Gizmos.color = Color.blue;

                for (int i = 0; i < fourDirectionConnections.Count; i++)
                {
                    Gizmos.DrawLine(transform.position, fourDirectionConnections[i].transform.position);
                }
            }
        }
    }
}