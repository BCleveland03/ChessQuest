using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TopDownGame
{
    public class EnemyMasterController : MonoBehaviour
    {
        // Outlets
        public Vector3 playerPos;
        //public CircleCollider2D detectionComponent;
        public GameObject healthDisplay;
        public GameObject healthDisplayBG;
        public Transform[] actionZones;

        SpriteRenderer sprite;

        // State Tracking
        [Header ("Preconfiguration")]
        public int enemyTypeID;
        public int currentHealth;
        public int currentHealthMax;
        public int enemyDamage;
        public float individualActionInterval;
        public float enemyDetectionRadius;
        public bool canEnemyEvade;
        private bool evadableState;

        [Header ("Enemy Functionality")]
        public float previousWaitCounter = -1;
        //private bool actionPerformed = false;
        //private bool withinRangeOfPlayer = false;

        [Header("Enemy AI")]
        public List<Vector2> allDirections = new List<Vector2>();
        public List<Vector2> availableDirections = new List<Vector2>();
        public bool playerDetectionRadius;
        public Vector3 hypotenuseOfEnemyToPlayer;
        public bool detectsPlayer = false;
        public bool seesPlayer = false;
        public Node previousNode;
        public Node currentNode;
        //private GameObject[] allObjects;
        public List<Node> path/* = new List<Node>()*/;
        public Vector3 targetPos;

        public enum StateMachine
        {
            Patrol,
            Engage,
            Evade
        }
        public StateMachine currentState;

        // Configuration
        [Header ("Detection")]
        public LayerMask combinedPlayerAndObstacleMask;
        public LayerMask combinedPlayerAndWallMask;
        public LayerMask obstacleMask;
        public LayerMask playerAndShieldMask;
        //public LayerMask playerMask;
        //public LayerMask wallMask;
        //public LayerMask obstacleMask;

        // Methods
        private void Start()
        {
            sprite = GetComponent<SpriteRenderer>();

            currentHealth = currentHealthMax;

            currentState = StateMachine.Patrol;
            StartCoroutine(InitiateEnemy(individualActionInterval));
        }

        IEnumerator InitiateEnemy(float interval)
        {
            // Waits until global timer is aligned with enemy's personal movement time interval
            while (!GameController.instance.IsThisInteger(GameController.instance.globalTimer / individualActionInterval))
            {
                yield return null;
            }

            previousWaitCounter = GameController.instance.globalTimer;
        }



        private void Update()
        {
            // Occurs once every specified interval
            if (GameController.instance.globalTimer - previousWaitCounter == individualActionInterval)
            {
                // Sets the counter to the current time so it can wait again
                previousWaitCounter = GameController.instance.globalTimer;

                // Finds the player position for future reference
                playerPos = new Vector2(PlayerController.instance.targetedPos.x, PlayerController.instance.targetedPos.y);

                // Player detection for engage state
                if (Vector2.Distance(transform.position, playerPos) < enemyDetectionRadius * 2 + 1 - 0.1f)
                {
                    hypotenuseOfEnemyToPlayer = playerPos - transform.position;

                    float radiansTowardPlayer = Mathf.Atan2(hypotenuseOfEnemyToPlayer.y, hypotenuseOfEnemyToPlayer.x);
                    float angleTowardPlayer = radiansTowardPlayer * Mathf.Rad2Deg;

                    // Detect for walls
                    RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, hypotenuseOfEnemyToPlayer, enemyDetectionRadius * 2 + 1, combinedPlayerAndWallMask);
                    Debug.DrawRay(transform.position, hypotenuseOfEnemyToPlayer, Color.red, 0.5f);

                    for (int i = 0; i < hits.Length; i++)
                    {
                        RaycastHit2D hit = hits[i];
                        /*Debug.Log("---------------");
                        Debug.Log(hits.Length);
                        Debug.Log(hit.collider.gameObject);
                        Debug.Log("===============");*/

                        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
                        {
                            seesPlayer = true;
                            break;
                        }
                        else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
                        {
                            seesPlayer = false;
                            break;
                        }
                    }
                }
                else
                {
                    seesPlayer = false;
                }



                // Active node update
                GameObject[] allObjects = GameObject.FindGameObjectsWithTag("NodePoint");
                foreach (GameObject obj in allObjects)
                {
                    Node n = obj.GetComponent<Node>();
                    if (Mathf.Round(transform.position.x) == n.transform.position.x && Mathf.Round(transform.position.y) == n.transform.position.y)
                    {
                        currentNode = n;
                        break;
                    }
                }

                // Update state
                if (!seesPlayer && currentState != StateMachine.Patrol && currentHealth > (currentHealthMax * 10) / 100)
                {
                    currentState = StateMachine.Patrol;
                    path.Clear();
                    print("Patrolling");
                }
                else if (seesPlayer && currentState != StateMachine.Engage && currentHealth > (currentHealthMax * 10) / 100)
                {
                    currentState = StateMachine.Engage;
                    path.Clear();
                    print("Engaged");
                }
                else if (currentState != StateMachine.Evade && currentHealth <= (currentHealthMax * 10) / 100)
                {
                    currentState = StateMachine.Evade;
                    path.Clear();
                    print("Running");
                }

                switch (currentState)
                {
                    case StateMachine.Patrol:
                        Patrol();
                        break;
                    case StateMachine.Engage:
                        Engage();
                        break;
                    case StateMachine.Evade:
                        Evade();
                        break;
                }
            }
        }

        public void EnemyTakeDamage()
        {
            currentHealth -= PlayerController.instance.damageOutput[PlayerController.instance.selectedCharacter];
            healthDisplay.transform.localScale = new Vector3((float)currentHealth / currentHealthMax, 1, 1);
            StartCoroutine(DamageFlash());

            if (currentHealth < currentHealthMax)
            {
                //healthDisplay.SetActive(true);
                healthDisplayBG.SetActive(true);
            }

            if (currentHealth <= 0)
            {
                Destroy(gameObject);
            }
        }

        // STATE MACHINE STATES //
        void Patrol()
        {
            RaycastHit2D hitObstacle;

            availableDirections.Clear();

            // Establishing available directions
            for (int i = 0; i < allDirections.Count; i++)
            {
                // Check this direction
                hitObstacle = Physics2D.Raycast(transform.position, allDirections[i], 2, combinedPlayerAndObstacleMask);
                Debug.DrawRay(transform.position, allDirections[i], Color.white, 0.5f);
                if (hitObstacle.collider != null)
                {
                    if (hitObstacle.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
                    {
                        print("Attack");
                        break;
                    }
                }
                else
                {
                    availableDirections.Add(allDirections[i]);
                }
            }

            // Movement
            if (availableDirections.Count > 0)
            {
                targetPos = new Vector2(Mathf.Round(transform.position.x / 2) * 2, Mathf.Round(transform.position.y / 2) * 2) + availableDirections[Random.Range(0, availableDirections.Count)];
                StartCoroutine(MoveEnemy());
            }

            /* Patrol script goes here, is not using node system
            if (path.Count == 0)
            {
                path = AStarManager.instance.GeneratePath(currentNode, AStarManager.instance.NodesInScene()[Random.Range(0, AStarManager.instance.NodesInScene().Length)]);
            }*/
        }

        void Engage()
        {
            path.Clear();
            path = AStarManager.instance.GeneratePath(currentNode, AStarManager.instance.FindNearestNode(playerPos));

            /*for (int i = 1; i < path.Count; i++)
            {
                path.RemoveAt(i);
            }*/

            targetPos = path[1].transform.position;
            if(targetPos == playerPos)
            {
                if (PlayerController.instance.selectedCharacter == 0 && PlayerController.instance.rookShielded)
                {
                    hypotenuseOfEnemyToPlayer = playerPos - transform.position;

                    // Detect for shield blocking the attack
                    RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, hypotenuseOfEnemyToPlayer, 2, playerAndShieldMask);
                    Debug.DrawRay(transform.position, hypotenuseOfEnemyToPlayer, Color.magenta, 0.5f);

                    for (int i = 0; i < hits.Length; i++)
                    {
                        RaycastHit2D hit = hits[i];

                        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
                        {
                            PlayerController.instance.takeDamage(enemyDamage);
                            print("Perform attack");
                            break;
                        }
                        else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Shield"))
                        {
                            print("Deflected!");
                            break;
                        }
                    }
                }
                else
                {
                    PlayerController.instance.takeDamage(enemyDamage);
                    print("Perform attack");
                }
            }
            else
            {
                StartCoroutine(MoveEnemy());
            }
        }

        void Evade()
        {
            /*if (path.Count == 0)
            {
                path = AStarManager.instance.GeneratePath(currentNode, AStarManager.instance.FindFurthestNode(playerPos));
            }*/
        }

        void CreatePath()
        {
            if (path.Count > 0)
            {
                int x = 0;
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(path[x].transform.position.x, path[x].transform.position.y, 0),
                    2 * Time.deltaTime);

                if (Vector2.Distance(transform.position, path[x].transform.position) < 0.1f)
                {
                    currentNode = path[x];
                    path.RemoveAt(x);
                }
            }
        }

        IEnumerator MoveEnemy()
        {
            Vector2 startPos = transform.position;
            float moveDuration = Vector2.Distance(startPos, targetPos) * individualActionInterval / 8;
            float timeElapsed = 0;

            while (timeElapsed < moveDuration)
            {
                transform.position = Vector2.Lerp(startPos, targetPos, timeElapsed / moveDuration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
        }

        IEnumerator DamageFlash()
        {
            sprite.color = new Color(1, 0.6f, 0.6f);
            yield return new WaitForSeconds(0.1f);
            sprite.color = new Color(1, 1, 1);
            yield return null;
        }
    }
}
