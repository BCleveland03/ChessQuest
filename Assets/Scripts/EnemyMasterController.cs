using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TopDownGame
{
    public class EnemyMasterController : MonoBehaviour
    {
        // Outlets
        public Vector2 playerPos;
        //public CircleCollider2D detectionComponent;
        public GameObject healthDisplay;
        public GameObject healthDisplayBG;
        public GameObject starSwirl;
        //public Transform[] actionZones;
        SpriteRenderer sprite;
        CircleCollider2D circCollider;
        Animator animator;

        // State Tracking
        [Header ("Preconfiguration")]
        public int enemyTypeID;
        public int currentHealth;
        public int currentHealthMax;
        public int enemyDamage;
        public float enemyDetectionRadius;
        private bool evadableState;
        public float travelDirection;
        public bool withinRangeToPerformAction = false;

        [Header ("Enemy Functionality")]
        public float previousWaitCounter = -1;
        //private bool actionPerformed = false;
        //private bool withinRangeOfPlayer = false;

        [Header("Enemy AI/Behavior")]
        public List<Vector2> allSlimeDirections = new List<Vector2>();
        public List<Vector2> allBatDirections = new List<Vector2>();
        public List<Vector2> allGuardDirections = new List<Vector2>();
        public List<Vector2> availableDirections = new List<Vector2>();
        public bool playerDetectionRadius;
        private Vector3 hypotenuseOfEnemyToPlayer;
        private float angleTowardPlayer;
        public bool detectsPlayer = false;
        public bool canTrackPlayer = false;
        public bool eightDirectionalMovement;

        public enum EnemyType
        {
            ForestSlime,
            CommonBat,
            OnyxGuard
        }
        public EnemyType selectedEnemy;

        //public bool canMove = true;
        public bool collidesWithGaps;
        public bool canBeLandedOn;
        public bool isFlyingLayer;
        private int flyingInt;
        public bool protectionFromLandedOn;
        public bool canEnemyEvade;
        public float patrolMoveSpeed;
        public float patrolActionInterval;
        public float engagedMoveSpeed;
        public float engageActionInterval;
        private float individualActionInterval;

        // For guard's idle animations
        [SerializeField]
        private int idleCounter = 3;
        [SerializeField]
        private int maskFlipBlocker = 1;

        [Header("Node Info")]
        public Node previousNode;
        public Node currentNode;
        //private GameObject[] allObjects;
        public List<Node> path/* = new List<Node>()*/;
        public Vector2 targetPos;

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
        public LayerMask wallMask;
        public LayerMask playerAndShieldMask;
        //public LayerMask playerMask;
        //public LayerMask wallMask;
        //public LayerMask obstacleMask;

        // Methods
        private void Start()
        {
            sprite = GetComponentInChildren<SpriteRenderer>();
            circCollider = GetComponent<CircleCollider2D>();
            animator = GetComponentInChildren<Animator>();

            currentHealth = currentHealthMax;
            animator.SetInteger("Health", currentHealth);
            animator.SetFloat("DirectionAngle", 180);

            currentState = StateMachine.Patrol;
            StartCoroutine(InitiateEnemy(patrolActionInterval));
        }

        IEnumerator InitiateEnemy(float interval)
        {
            // Waits until global timer is aligned with enemy's personal movement time interval
            while (!GameController.instance.IsThisInteger(GameController.instance.globalTimer / patrolActionInterval))
            {
                yield return null;
            }

            previousWaitCounter = GameController.instance.globalTimer;
        }



        private void Update()
        {
            // Reset path
            if (path != null)
            {
                path.Clear();
            }

            if (currentState == StateMachine.Patrol)
            {
                individualActionInterval = patrolActionInterval;
            }
            else if (currentState == StateMachine.Engage)
            {
                individualActionInterval = engageActionInterval;
            }

            // Action occurs once every specified interval
            if (GameController.instance.globalTimer - previousWaitCounter >= individualActionInterval && currentHealth > 0)
            {                
                // Sets the counter to the current time so it can wait again
                previousWaitCounter = GameController.instance.globalTimer;

                // Finds the player position for future reference
                playerPos = new Vector2(PlayerController.instance.targetedPos.x, PlayerController.instance.targetedPos.y);

                // Determines if any actions should be performed
                if (Vector2.Distance(transform.position, playerPos) < 20f)
                {
                    withinRangeToPerformAction = true;

                    // Player detection for engage state
                    if (Vector2.Distance(transform.position, playerPos) < enemyDetectionRadius * 2 + 1 - 0.1f /*&&
                    Vector2.Distance(transform.position, playerPos) > 0.1f*/)
                    {
                        hypotenuseOfEnemyToPlayer = playerPos - new Vector2(transform.position.x, transform.position.y);

                        float radiansTowardPlayer = Mathf.Atan2(hypotenuseOfEnemyToPlayer.y, hypotenuseOfEnemyToPlayer.x);
                        angleTowardPlayer = radiansTowardPlayer * Mathf.Rad2Deg;

                        // Detect for walls
                        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, hypotenuseOfEnemyToPlayer, enemyDetectionRadius * 2 + 1, combinedPlayerAndWallMask);
                        Debug.DrawRay(transform.position, hypotenuseOfEnemyToPlayer, Color.red, 0.5f);

                        for (int i = 0; i < hits.Length; i++)
                        {
                            RaycastHit2D hit = hits[i];
                            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
                            {
                                // Incorporates same-tile check here since Bat can occupy same tile and remain engaged
                                if ((selectedEnemy == EnemyType.ForestSlime || selectedEnemy == EnemyType.OnyxGuard) && Vector2.Distance(transform.position, playerPos) > 0.1f)
                                {
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

                                    // Generate path to check for possible reach
                                    /* If enemy has eightDirectionalMovement enabled, the enemy will use the eight direction node connection list;
                                     * otherwise will use four direction node connection list
                                     */
                                    path = AStarManager.instance.GeneratePath(currentNode, AStarManager.instance.FindNearestNode(playerPos), eightDirectionalMovement);

                                    /*if (eightDirectionalMovement)
                                    {
                                        path = AStarManager.instance.GeneratePath(currentNode, AStarManager.instance.FindNearestNode(playerPos), true);
                                    }
                                    else
                                    {
                                        path = AStarManager.instance.GeneratePath(currentNode, AStarManager.instance.FindNearestNode(playerPos), false);
                                    }*/

                                    if (path != null)
                                    {
                                        canTrackPlayer = true;
                                    }
                                    else
                                    {
                                        //path.Clear();
                                        canTrackPlayer = false;
                                    }

                                    break;
                                }
                                else if (selectedEnemy == EnemyType.CommonBat)
                                {
                                    canTrackPlayer = true;
                                    break;
                                }
                            }
                            else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
                            {
                                canTrackPlayer = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        canTrackPlayer = false;
                    }



                    // Update state
                    // Evade won't be included in show build
                    if (!canTrackPlayer && currentState != StateMachine.Patrol && currentHealth > 1)//(currentHealthMax * 10) / 100)
                    {
                        currentState = StateMachine.Patrol;
                        //path.Clear();
                        print("Patrolling");
                    }
                    else if (canTrackPlayer && currentState != StateMachine.Engage && currentHealth > 1)//(currentHealthMax * 10) / 100)
                    {
                        currentState = StateMachine.Engage;
                        //path.Clear();
                        print("Engaged");
                    }
                    else if (currentState != StateMachine.Evade && currentHealth <= 1)//(currentHealthMax * 10) / 100)
                    {
                        currentState = StateMachine.Evade;
                        //path.Clear();
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
                else
                {
                    // If not within distance of player, prevent any movement actions to help preserve initial spawn locations
                    withinRangeToPerformAction = false;
                }
            }

            flyingInt = isFlyingLayer ? 1 : 0;

            // Update enemy layer
            sprite.sortingOrder = (int)(transform.position.y * -10 + (flyingInt * 10));
        }

        public void EnemyTakeDamage(string TypeOfAttack, float mult)
        {
            if (TypeOfAttack == "main")
            {
                currentHealth -= (int)(PlayerController.instance.damageOutput[PlayerController.instance.selectedCharacter] * mult);
                animator.SetInteger("Health", currentHealth);
            }
            else if (TypeOfAttack == "landing")
            {
                if (canBeLandedOn)
                {
                    // Inflict damage from being landed on
                    currentHealth -= PlayerController.instance.damageOutput[3];
                    animator.SetInteger("Health", currentHealth);

                    if (currentHealth <= 0)
                    {
                        StartCoroutine(DeathAnimation());
                    }
                    else
                    {
                        CreateAvailableDirections(obstacleMask);

                        // Movement
                        if (availableDirections.Count > 0)
                        {
                            targetPos = new Vector2(Mathf.Round(transform.position.x / 2) * 2, Mathf.Round(transform.position.y / 2) * 2) + availableDirections[Random.Range(0, availableDirections.Count)];
                            StartCoroutine(MoveEnemy(PlayerController.instance.moveSpeed * 1.2f));
                        }
                        else
                        {
                            StartCoroutine(DeathAnimation());
                        }
                    }
                }
                else
                {
                    // IMPLEMENT PUSH BACK WHEN LANDING; IF ENEMY CANT BE LANDED ON, CALL FUNCTION IN PLAYER SCRIPT THAT PUSHES THEM BACK A TILE

                }
            }

            healthDisplay.transform.localScale = new Vector3((float)currentHealth / currentHealthMax, 1, 1);
            StartCoroutine(DamageFlash());

            if (currentHealth < currentHealthMax)
            {
                //healthDisplay.SetActive(true);
                healthDisplayBG.SetActive(true);
            }

            if (currentHealth <= 0)
            {
                StartCoroutine(DeathAnimation());
            }
        }





        // STATE MACHINE STATES //
        void Patrol()
        {
            LayerMask gapCheckMask;
            
            if (collidesWithGaps)
            {
                gapCheckMask = combinedPlayerAndObstacleMask;
            }
            else
            {
                gapCheckMask = combinedPlayerAndWallMask;
            }
            
            // Creates available directions for enemy to move
            CreateAvailableDirections(gapCheckMask);

            // Movement
            if (availableDirections.Count > 0)
            {
                // Random idle actions
                if (selectedEnemy == EnemyType.OnyxGuard)
                {
                    // 1 in 3/4/5/... chance of idleing
                    if (Random.Range(1, idleCounter) == 1)
                    {
                        print(idleCounter);
                        idleCounter++;

                        if (Random.Range(maskFlipBlocker, 4) == 1)
                        {
                            print("Mask flip");
                            animator.SetTrigger("MaskFlip");
                            maskFlipBlocker = 2;
                        }              
                    }
                    else
                    {
                        // Reset counters
                        idleCounter = 3;
                        maskFlipBlocker = 1;

                        targetPos = new Vector2(Mathf.Round(transform.position.x / 2) * 2, Mathf.Round(transform.position.y / 2) * 2) + availableDirections[Random.Range(0, availableDirections.Count)];
                        StartCoroutine(MoveEnemy(patrolMoveSpeed));
                    }
                }
                else
                {
                    targetPos = new Vector2(Mathf.Round(transform.position.x / 2) * 2, Mathf.Round(transform.position.y / 2) * 2) + availableDirections[Random.Range(0, availableDirections.Count)];
                    StartCoroutine(MoveEnemy(patrolMoveSpeed));
                }
            }
        }

        void Engage()
        {
            if (selectedEnemy == EnemyType.ForestSlime || selectedEnemy == EnemyType.OnyxGuard)
            {
                // Slime AI pathing
                targetPos = path[1].transform.position;

                if (targetPos == playerPos)
                {
                    if (PlayerController.instance.selectedCharacter == 0 && PlayerController.instance.rookShielded)
                    {
                        hypotenuseOfEnemyToPlayer = playerPos - new Vector2(transform.position.x, transform.position.y);

                        // Detect for shield blocking the attack
                        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, hypotenuseOfEnemyToPlayer, 2, playerAndShieldMask);
                        Debug.DrawRay(transform.position, hypotenuseOfEnemyToPlayer, Color.magenta, 0.5f);

                        for (int i = 0; i < hits.Length; i++)
                        {
                            RaycastHit2D hit = hits[i];

                            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
                            {
                                GetTravelDirection(transform.position);
                                animator.SetFloat("DirectionAngle", travelDirection);
                                animator.SetTrigger("DoesAttack");

                                PlayerController.instance.takeDamage(enemyDamage);
                                print("Perform attack");
                                break;
                            }
                            else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Shield"))
                            {
                                GetTravelDirection(transform.position);
                                animator.SetFloat("DirectionAngle", travelDirection);
                                animator.SetTrigger("DoesAttack");

                                print("Deflected!");
                                PlayerController.instance.SpawnParticle(PlayerController.instance.facingDirection);
                                break;
                            }
                        }
                    }
                    else
                    {
                        GetTravelDirection(transform.position);
                        animator.SetFloat("DirectionAngle", travelDirection);
                        animator.SetTrigger("DoesAttack");

                        PlayerController.instance.takeDamage(enemyDamage);
                        print("Perform attack");
                    }
                }
                else
                {
                    StartCoroutine(MoveEnemy(engagedMoveSpeed));
                }
            }
            else if (selectedEnemy == EnemyType.CommonBat)
            {
                // Bat pathing, similar to patrol movement, but direction is targeted
                // Does not use pathfinding system
                RaycastHit2D hitObstacle;
                Vector2 batLocation = transform.position;
                int readyToDeleteItemNumber = 0;
                float distanceBetweenPlayerandTarget;
                Vector2 closestTargetToPlayer = new Vector2();
                float previousMinimumDistance;
                List<Vector2> reorganizedBatDirections = new List<Vector2>();
                List<Vector2> tempBatDirections = new List<Vector2>();

                availableDirections.Clear();
                reorganizedBatDirections.Clear();
                tempBatDirections.Clear();

                // Check if enemy is already overlapping player to attack
                if (batLocation == playerPos)
                {
                    animator.SetTrigger("DoesAttack");

                    PlayerController.instance.takeDamage(enemyDamage);
                    print("Perform attack");
                }
                else
                {
                    // Add all bat directions to the temporary list for process of deletion
                    for (int i = 0; i < allBatDirections.Count; i++)
                    {
                        tempBatDirections.Add(allBatDirections[i]);
                    }

                    for (int i = 0; i < allBatDirections.Count; i++)
                    {
                        previousMinimumDistance = 20;

                        for (int j = 0; j < tempBatDirections.Count; j++)
                        {
                            distanceBetweenPlayerandTarget = Vector2.Distance(playerPos, batLocation + tempBatDirections[j]);

                            if (previousMinimumDistance > distanceBetweenPlayerandTarget)
                            {
                                previousMinimumDistance = distanceBetweenPlayerandTarget;
                                closestTargetToPlayer = tempBatDirections[j];
                                readyToDeleteItemNumber = j;
                            }
                            //print(distanceBetweenPlayerandTarget);
                        }

                        reorganizedBatDirections.Add(closestTargetToPlayer);
                        tempBatDirections.Remove(tempBatDirections[readyToDeleteItemNumber]);

                        if (tempBatDirections.Count == 0)
                        {
                            break;
                        }
                    }

                    // Go down the new list of directions based on closest direction to desired direction
                    // Goes in opposite direction (for whatever reason), reverse to create evade movement
                    for (int i = reorganizedBatDirections.Count - 1; i >= 0; i--)
                    {
                        // Check this direction
                        hitObstacle = Physics2D.Raycast(transform.position, reorganizedBatDirections[i],
                            Mathf.Sqrt(Mathf.Pow(reorganizedBatDirections[i].x, 2) + Mathf.Pow(reorganizedBatDirections[i].y, 2)),
                            wallMask);
                        Debug.DrawRay(transform.position, reorganizedBatDirections[i], Color.white, 0.5f);
                        if (hitObstacle.collider == null)
                        {
                            //availableDirections.Add(reorganizedBatDirections[i]);
                            targetPos = new Vector2(Mathf.Round(transform.position.x / 2) * 2, Mathf.Round(transform.position.y / 2) * 2) + reorganizedBatDirections[i];
                            StartCoroutine(MoveEnemy(engagedMoveSpeed));
                        }
                    }
                }
            }
        }

        void Evade()
        {
            /*if (path.Count == 0)
            {
                path = AStarManager.instance.GeneratePath(currentNode, AStarManager.instance.FindFurthestNode(playerPos), false);
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

        // 0 does NOT include player in mask, 1 DOES include player in mask
        void CreateAvailableDirections(LayerMask maskCheck)
        {
            RaycastHit2D hitObstacle;
            List<Vector2> specifiedEnemyTypeDirections = new List<Vector2>();

            availableDirections.Clear();

            // Direction determination based on which enemy is which
            if (selectedEnemy == EnemyType.ForestSlime)
            {
                specifiedEnemyTypeDirections = allSlimeDirections;
            }
            else if (selectedEnemy == EnemyType.CommonBat)
            {
                specifiedEnemyTypeDirections = allBatDirections;
            }
            else if (selectedEnemy == EnemyType.OnyxGuard)
            {
                specifiedEnemyTypeDirections = allGuardDirections;
            }

            // Establishing available directions based on enemy type
            for (int i = 0; i < specifiedEnemyTypeDirections.Count; i++)
            {
                // Check this direction
                hitObstacle = Physics2D.Raycast(transform.position, specifiedEnemyTypeDirections[i],
                    Mathf.Sqrt(Mathf.Pow(specifiedEnemyTypeDirections[i].x, 2) + Mathf.Pow(specifiedEnemyTypeDirections[i].y, 2)), 
                    maskCheck);
                Debug.DrawRay(transform.position, specifiedEnemyTypeDirections[i], Color.white, 0.5f);
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
                    availableDirections.Add(specifiedEnemyTypeDirections[i]);
                }
            }
        }

        IEnumerator MoveEnemy(float speedMult)
        {
            if (withinRangeToPerformAction)
            {
                Vector2 startPos = transform.position;
                GetTravelDirection(startPos);
                animator.SetFloat("DirectionAngle", travelDirection);

                float moveDuration = Vector2.Distance(startPos, targetPos) * individualActionInterval / 8;
                float timeElapsed = 0;
                animator.SetBool("IsMoving", true);

                // Find direction of movement

                while (timeElapsed < (moveDuration / speedMult))
                {
                    transform.position = Vector2.Lerp(startPos, targetPos, timeElapsed / (moveDuration / speedMult));
                    timeElapsed += Time.deltaTime * speedMult;
                    yield return null;
                }

                //transform.position = targetPos;
                animator.SetBool("IsMoving", false);
            }
            else
            {
                yield return null;
            }
        }

        IEnumerator DamageFlash()
        {
            WaitForSeconds delayFlash = new WaitForSeconds(0.1f);
            
            for (int i = 0; i < 2; i++)
            {
                sprite.color = new Color(1, 0.6f, 0.6f);
                yield return delayFlash;
                sprite.color = new Color(1, 1, 1);
                yield return delayFlash;
                yield return null;
            }
        }

        IEnumerator DeathAnimation()
        {
            WaitForSeconds fadeAway = new WaitForSeconds(0.01f);
            
            gameObject.layer = 0;
            healthDisplayBG.SetActive(false);
            circCollider.enabled = false;

            if (selectedEnemy == EnemyType.OnyxGuard)
            {
                for (float i = 0f; i < 1f; i += 0.05f)
                {
                    starSwirl.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, i);
                    yield return fadeAway;
                }
            }

            while (Vector2.Distance(transform.position, PlayerController.instance.transform.position)
                < GameController.instance.enemyDespawnDistance)
            {
                yield return null;
            }

            for (float i = 1f; i > 0f; i -= 0.05f)
            {
                sprite.color = new Color(1, 1, 1, i);
                yield return fadeAway;
            }

            Destroy(gameObject);
        }

        void GetTravelDirection(Vector2 startingPosition)
        {
            Vector2 directionFromStartToTarget = startingPosition - targetPos;
            float radianTravelDirection = Mathf.Atan2(directionFromStartToTarget.y, directionFromStartToTarget.x);
            travelDirection = radianTravelDirection * Mathf.Rad2Deg;
        }
    }
}
