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
        public List<Node> connections;

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
                /* Need to convert from a GameObject so that these can be put into the List */
                //connections.Add(NodeController.instance.transform.Find("" + (nameToInt - 100)).gameObject);
                connections.Add(NodeController.instance.transform.Find("" + (nameToInt - 100)).GetComponent<Node>());
            }

            // East adjacent node check
            if (NodeController.instance.nodeIdentification.ContainsKey(nameToInt + 1))
            {
                connections.Add(NodeController.instance.transform.Find("" + (nameToInt + 1)).GetComponent<Node>());
            }

            // South adjacent node check
            if (NodeController.instance.nodeIdentification.ContainsKey(nameToInt + 100))
            {
                connections.Add(NodeController.instance.transform.Find("" + (nameToInt + 100)).GetComponent<Node>());
            }

            // West adjacent node check
            if (NodeController.instance.nodeIdentification.ContainsKey(nameToInt - 1))
            {
                connections.Add(NodeController.instance.transform.Find("" + (nameToInt - 1)).GetComponent<Node>());
            }

            yield break;
        }

        public float FScore()
        {
            return gScore + hScore;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;

            if (connections.Count > 0)
            {
                for (int i = 0; i < connections.Count; i++)
                {
                    Gizmos.DrawLine(transform.position, connections[i].transform.position);
                }
            }
        }
    }
}