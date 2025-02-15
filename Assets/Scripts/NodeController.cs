using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TopDownGame
{
    public class NodeController : MonoBehaviour
    {
        // Outlets
        public static NodeController instance;
        public GameObject nodeDetector;
        public GameObject nodePoint;

        // State Tracking
        /*public List<int> nodeIdentification;*/
        public Dictionary<int, string> nodeIdentification = new Dictionary<int, string>();
        public int nodeGridCount = 0;
        private int tempID;

        // Configuration
        // Half the side length of the square grid; will create an odd sized square, with the center being reserved for the player
        public int nodeGridDimensions;

        [Header("Detection")]
        public LayerMask wallMask;
        public LayerMask combinedMask;

        // Methods
        private void Awake()
        {
            instance = this;

            nodeDetector.SetActive(false);
        }

        public void ReinstantiateNodePoints()
        {
            // Clear existing nodes
            nodeIdentification.Clear();
            nodeGridCount = 0;

            GameObject[] allObjects = GameObject.FindGameObjectsWithTag("NodePoint");
            foreach (GameObject obj in allObjects)
            {
                Destroy(obj);
            }

            PlayerController.instance.nodeParent.transform.position = PlayerController.instance.transform.position;
            nodeDetector.SetActive(true);

            // Goes across the rows
            for (int r = -nodeGridDimensions; r < nodeGridDimensions + 1; r++)
            {
                // Goes down the columns
                for (int c = -nodeGridDimensions; c < nodeGridDimensions + 1; c++)
                {
                    nodeDetector.transform.position = new Vector3(2 * c + transform.position.x, -2 * r + transform.position.y, 0);
                    tempID = (r + nodeGridDimensions + 1) * 100 + (c + nodeGridDimensions + 1);

                    RaycastHit2D hit = Physics2D.Raycast(nodeDetector.transform.position, Vector2.zero, 0, combinedMask);
                    if (hit.collider == null)
                    {
                        // Adds each node to a list with a specified ID
                        nodeIdentification.Add(tempID, "" + tempID);

                        // Spawns 
                        Node spawnedNode = Instantiate(nodePoint, PlayerController.instance.nodeParent.transform, false).GetComponent<Node>();
                        spawnedNode.transform.position = nodeDetector.transform.position;
                        spawnedNode.name = nodeIdentification[tempID];

                        SpriteRenderer nodeSprite = spawnedNode.GetComponent<SpriteRenderer>();
                        float tileDist = Vector2.Distance(spawnedNode.transform.position, PlayerController.instance.transform.position);

                        if (tileDist < 7f)
                        {
                            nodeSprite.color = new Color(1, 1, 1, Mathf.Clamp(1 / tileDist - 0.0625f, 0f, 0.4375f));
                        }
                        else
                        {
                            nodeSprite.color = new Color(1, 1, 1, 0);
                        }

                        nodeGridCount++;
                    }
                }
            }

            nodeDetector.SetActive(false);
            //print(nodeIdentification.Count);
        }
    }
}
