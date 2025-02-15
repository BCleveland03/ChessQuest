using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TopDownGame {
    public enum PlayerDirections
    {
        North = 0,
        Northeast = 1,
        East = 2,
        Southeast = 3,
        South = 4,
        Southwest = 5,
        West = 6,
        Northwest = 7
    }
    
    public class PlayerController : MonoBehaviour
    {
        // Outlet
        [Header("Outlets")]
        public static PlayerController instance;
        //public static bool queriesHitTriggers = true;
        public GameObject selectableTiles;
        public GameObject nodeParent;

        public GameObject actionCooldownBarBG;
        public GameObject actionCooldownBar;
        public GameObject projectileCooldownBarBG;
        public GameObject projectileCooldownBar;

        public SpriteRenderer spr;
        public Animator animator;

        [SerializeField]
        Sprite[] appearance;

        public GameObject rotationalPivot;
        public GameObject rookShield;
        public GameObject bishopProjectile;
        public GameObject projectileTargetTile;

        [Header("Health UI")]
        public Image frontHealthBar;
        public Image leftHealthBar;
        public Image rightHealthBar;

        // Configuration
        [Header("Detection")]
        public LayerMask wallMask;
        public LayerMask combinedMask;
        public LayerMask enemyMask;

        [Header ("Miscellaneous")]
        public Sprite[] sprites;

        private float moveDuration;
        public float moveModifier = 0.08f;
        public float mouseDirection;

        // State Tracking
        public int selectedCharacter = 0;
        public float currentScroll;
        public bool activeScrolling = true;
        public float TEMPScrollVelocity;
        public bool movementCoroutineIsActive = false;
        public bool attackCoroutineIsActive = false;
        private bool TEMPcharSwap = false;
        private int prevSelectedChar;

        private Vector3 mousePositionInWorld;
        public Vector2 targetedPos;
        public float moveSpeed;

        [Header("Health and Status Tracking")]
        public int rookHealthMax = 100;
        public int bishopHealthMax = 100;
        public int knightHealthMax = 100;

        public int knockedOutCharacters;

        public float[] characterHealths;
        public float[] characterHealthsMax;

        [Header("Action Tracking")]
        public int facingDirection;
        public int[] damageOutput;
        public float[] actionDelays;

        public bool rookShielded;
        public bool bishopEligibleTarget;
        public int bishopAttackRange;



        void Awake()
        {
            instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            // Deactivates attached attack hitboxes
            ResetAttackStates();

            // Establishes the max and sets each characters' health to their max
            characterHealthsMax[0] = rookHealthMax;
            characterHealthsMax[1] = bishopHealthMax;
            characterHealthsMax[2] = knightHealthMax;

            for (int i = 0; i < 3; i++)
            {
                characterHealths[i] = characterHealthsMax[i];
            }

            currentScroll = GameController.instance.scrollSensitivity / 2;
            prevSelectedChar = selectedCharacter;
            animator.SetInteger("SelectedCharacter", selectedCharacter);
            targetedPos = targetedPos = new Vector3(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
            gameObject.SetActive(true);
            spawnSelectableTiles();
            NodeController.instance.ReinstantiateNodePoints();
        }

        // Update is called once per frame
        void Update()
        {
            // OLD SCROLLING METHOD //
            //selectedCharacter = Mathf.FloorToInt(nfmod((currentScroll += Input.mouseScrollDelta.y) / GameController.instance.scrollSensitivity, 3f));
            
            // Scroll character swapping
            if (Input.mouseScrollDelta.y != 0 && !movementCoroutineIsActive && !attackCoroutineIsActive && !GameController.instance.isPaused)
            {
                currentScroll = currentScroll += Input.mouseScrollDelta.y * invertScrollConversion(GameController.instance.invertScroll);
                if (currentScroll <= -GameController.instance.scrollSensitivity)
                {
                    if (knockedOutCharacters < 2)
                    {
                        // Cycles back one
                        selectedCharacterCycle(-1);
                        updateHealthUI();
                        currentScroll = 0;
                        TEMPcharSwap = true;
                        clearExistingTiles();
                        spawnSelectableTiles();
                    }
                } 
                else if (currentScroll >= GameController.instance.scrollSensitivity)
                {
                    if (knockedOutCharacters < 2)
                    {
                        // Cycles forward one
                        selectedCharacterCycle(1);
                        updateHealthUI();
                        currentScroll = 0;
                        TEMPcharSwap = true;
                        clearExistingTiles();
                        spawnSelectableTiles();
                    }
                }

                if (selectedCharacter == 1)
                {
                    projectileTargetTile.SetActive(true);
                }
                else
                {
                    projectileTargetTile.SetActive(false);
                }

                activeScrolling = true;

                animator.SetInteger("SelectedCharacter", selectedCharacter);
            }

            // Manual character swapping
            if (!movementCoroutineIsActive && !attackCoroutineIsActive && !GameController.instance.isPaused)
            {
                // Rook (1 to select by default)
                if (Input.GetKeyDown(GameController.instance.keyChar0) && characterHealths[0] > 0)
                {
                    selectedCharacter = 0;
                    animator.SetInteger("SelectedCharacter", selectedCharacter);
                    updateHealthUI();
                    currentScroll = 0;
                    TEMPcharSwap = true;
                    clearExistingTiles();
                    spawnSelectableTiles();
                    projectileTargetTile.SetActive(false);
                }

                // Bishop (2 to select by default)
                if (Input.GetKeyDown(GameController.instance.keyChar1) && characterHealths[1] > 0f)
                {
                    selectedCharacter = 1;
                    animator.SetInteger("SelectedCharacter", selectedCharacter);
                    updateHealthUI();
                    currentScroll = 0;
                    TEMPcharSwap = true;
                    clearExistingTiles();
                    spawnSelectableTiles();
                    projectileTargetTile.SetActive(true);
                }
                
                // Knight (3 to select by default)
                if (Input.GetKeyDown(GameController.instance.keyChar2) && characterHealths[2] > 0f)
                {
                    selectedCharacter = 2;
                    animator.SetInteger("SelectedCharacter", selectedCharacter);
                    updateHealthUI();
                    currentScroll = 0;
                    TEMPcharSwap = true;
                    clearExistingTiles();
                    spawnSelectableTiles();
                    projectileTargetTile.SetActive(false);
                }
            }

            // Presentation of character selected
            spr.sprite = appearance[selectedCharacter];

            // Detects for a left click on a selectable tile; if true, then move character, update moves, and clear preexisting tiles
            if(Input.GetMouseButtonDown(0) && GameController.instance.selectedTile != null)
            {
                StartCoroutine(MovePlayer(GameController.instance.selectedTile.transform.position, moveSpeed));
                clearExistingTiles();
            }



            // Checks direction the mouse is in and detects for right click to perform attack
            Vector3 mousePosition = Input.mousePosition;
            mousePositionInWorld = GameController.instance.mainCam.ScreenToWorldPoint(mousePosition);
            Vector3 directionFromPlayerToMouse = transform.position - mousePositionInWorld;

            float radiansTowardMouse = Mathf.Atan2(directionFromPlayerToMouse.y, directionFromPlayerToMouse.x);
            mouseDirection = radiansTowardMouse * Mathf.Rad2Deg + 180;

            // Snap to East
            if (mouseDirection <= 45 || mouseDirection > 315)
            {
                facingDirection = 0;
            }
            // Snap to North
            else if (mouseDirection <= 135 && mouseDirection > 45)
            {
                facingDirection = 90;
            }
            // Snap to South
            else if (mouseDirection <= 315 && mouseDirection > 225)
            {
                facingDirection = 270;
            }
            // Snap to West
            else
            {
                facingDirection = 180;
            }

            animator.SetInteger("DirectionFacing", facingDirection);




            // Update target tile location to mouse pointer
            projectileTargetTile.transform.position = new Vector2(Mathf.Round(mousePositionInWorld.x / 2) * 2, Mathf.Round(mousePositionInWorld.y / 2) * 2);

            // Updates target icon to show eligible tiles for attack
            if (transform.position != projectileTargetTile.transform.position && Vector2.Distance(transform.position, projectileTargetTile.transform.position) <= bishopAttackRange && !attackCoroutineIsActive)
            {
                projectileTargetTile.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.2f);
                bishopEligibleTarget = true;
            }
            else
            {
                projectileTargetTile.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.075f);
                bishopEligibleTarget = false;
            }

            // Check to see if attack can be made available
            if (Input.GetMouseButtonDown(1) && !movementCoroutineIsActive && !attackCoroutineIsActive)
            {
                // Check specifically if bishop is selected, so it can then check if the target is an elegible target
                if (selectedCharacter == 1)
                {
                    // Check to make sure bishop's projectile attack doesn't land on self or target is too far from player
                    if (bishopEligibleTarget)
                    {
                        StartCoroutine(PerformAttack());
                        print("Performed attack toward " + rotationalPivot.transform.eulerAngles.z + "*");
                    }
                }
                else
                {
                    StartCoroutine(PerformAttack());
                    print("Performed attack toward " + rotationalPivot.transform.eulerAngles.z + "*");
                }
            }

            // Update enemy layer
            spr.sortingOrder = (int)(transform.position.y * -10 + 5);
        }

        public void clearExistingTiles()
        {
            GameObject[] allObjects = GameObject.FindGameObjectsWithTag("SelectableTiles");
            foreach(GameObject obj in allObjects)
            {
                Destroy(obj);
            }
        }
        
        public void spawnSelectableTiles()
        {
            // Prefap variables for tracking where raycasts are casting from and what they'll be hitting
            Vector2 tempHitPos;
            RaycastHit2D hitObstacle;

            // Assigning vector directions and their distances (for debug purposes)
            Vector2 up = transform.TransformDirection(new Vector2(0, 2));
            Vector2 right = transform.TransformDirection(new Vector2(2, 0));
            Vector2 down = transform.TransformDirection(new Vector2(0, -2));
            Vector2 left = transform.TransformDirection(new Vector2(-2, 0));

            Vector2 upright = transform.TransformDirection(new Vector2(2, 2));
            Vector2 downright = transform.TransformDirection(new Vector2(2, -2));
            Vector2 downleft = transform.TransformDirection(new Vector2(-2, -2));
            Vector2 upleft = transform.TransformDirection(new Vector2(-2, 2));

            Vector2 noDirNecessary = transform.TransformDirection(new Vector2(0, 0));



            if (selectedCharacter == 0)
            {
                // North
                tempHitPos = transform.position;

                for (int i = 1; i < 4; i++)
                {
                    hitObstacle = Physics2D.Raycast(tempHitPos, up, 2, combinedMask);
                    Debug.DrawRay(tempHitPos, up, Color.white, 0.5f);

                    if (hitObstacle.collider == null)
                    {
                        tempHitPos += up;

                        GameObject spawnedTile = Instantiate(selectableTiles, transform.position, Quaternion.identity);
                        spawnedTile.transform.position = tempHitPos;
                    }
                    else
                    {
                        break;
                    }
                }

                // East
                tempHitPos = transform.position;

                for (int i = 1; i < 4; i++)
                {
                    hitObstacle = Physics2D.Raycast(tempHitPos, right, 2, combinedMask);
                    Debug.DrawRay(tempHitPos, right, Color.white, 0.5f);

                    if (hitObstacle.collider == null)
                    {
                        tempHitPos += right;

                        GameObject spawnedTile = Instantiate(selectableTiles, transform.position, Quaternion.identity);
                        spawnedTile.transform.position = tempHitPos;
                    }
                    else
                    {
                        break;
                    }
                }

                // South
                tempHitPos = transform.position;

                for (int i = 1; i < 4; i++)
                {
                    hitObstacle = Physics2D.Raycast(tempHitPos, down, 2, combinedMask);
                    Debug.DrawRay(tempHitPos, down, Color.white, 0.5f);

                    if (hitObstacle.collider == null)
                    {
                        tempHitPos += down;

                        GameObject spawnedTile = Instantiate(selectableTiles, transform.position, Quaternion.identity);
                        spawnedTile.transform.position = tempHitPos;
                    }
                    else
                    {
                        break;
                    }
                }

                // West
                tempHitPos = transform.position;

                for (int i = 1; i < 4; i++)
                {
                    hitObstacle = Physics2D.Raycast(tempHitPos, left, 2, combinedMask);
                    Debug.DrawRay(tempHitPos, left, Color.white, 0.5f);

                    if (hitObstacle.collider == null)
                    {
                        tempHitPos += left;

                        GameObject spawnedTile = Instantiate(selectableTiles, transform.position, Quaternion.identity);
                        spawnedTile.transform.position = tempHitPos;
                    }
                    else
                    {
                        break;
                    }
                }
            }



            if (selectedCharacter == 1)
            {
                // Northeast
                tempHitPos = transform.position;

                for (int i = 1; i < 4; i++)
                {
                    hitObstacle = Physics2D.Raycast(tempHitPos, upright, 2, combinedMask);
                    Debug.DrawRay(tempHitPos, upright, Color.white, 0.5f);

                    if (hitObstacle.collider == null)
                    {
                        tempHitPos += upright;

                        GameObject spawnedTile = Instantiate(selectableTiles, transform.position, Quaternion.identity);
                        spawnedTile.transform.position = tempHitPos;
                    }
                    else
                    {
                        break;
                    }
                }

                // Southeast
                tempHitPos = transform.position;

                for (int i = 1; i < 4; i++)
                {
                    hitObstacle = Physics2D.Raycast(tempHitPos, downright, 2, combinedMask);
                    Debug.DrawRay(tempHitPos, downright, Color.white, 0.5f);

                    if (hitObstacle.collider == null)
                    {
                        tempHitPos += downright;

                        GameObject spawnedTile = Instantiate(selectableTiles, transform.position, Quaternion.identity);
                        spawnedTile.transform.position = tempHitPos;
                    }
                    else
                    {
                        break;
                    }
                }

                // Southwest
                tempHitPos = transform.position;

                for (int i = 1; i < 4; i++)
                {
                    hitObstacle = Physics2D.Raycast(tempHitPos, downleft, 2, combinedMask);
                    Debug.DrawRay(tempHitPos, downleft, Color.white, 0.5f);

                    if (hitObstacle.collider == null)
                    {
                        tempHitPos += downleft;

                        GameObject spawnedTile = Instantiate(selectableTiles, transform.position, Quaternion.identity);
                        spawnedTile.transform.position = tempHitPos;
                    }
                    else
                    {
                        break;
                    }
                }

                // Northwest
                tempHitPos = transform.position;

                for (int i = 1; i < 4; i++)
                {
                    hitObstacle = Physics2D.Raycast(tempHitPos, upleft, 2, combinedMask);
                    Debug.DrawRay(tempHitPos, upleft, Color.white, 0.5f);

                    if (hitObstacle.collider == null)
                    {
                        tempHitPos += upleft;

                        GameObject spawnedTile = Instantiate(selectableTiles, transform.position, Quaternion.identity);
                        spawnedTile.transform.position = tempHitPos;
                    }
                    else
                    {
                        break;
                    }
                }
            }



            if (selectedCharacter == 2)
            {
                // North North East
                tempHitPos = transform.position;

                hitObstacle = Physics2D.Raycast(tempHitPos, up, 2, wallMask);
                Debug.DrawRay(tempHitPos, up, Color.white, 0.5f);

                if (hitObstacle.collider == null)
                {
                    tempHitPos += up;

                    hitObstacle = Physics2D.Raycast(tempHitPos, right, 2, wallMask);
                    Debug.DrawRay(tempHitPos, right, Color.white, 0.5f);

                    if (hitObstacle.collider == null)
                    {
                        tempHitPos += right;

                        hitObstacle = Physics2D.Raycast(tempHitPos, up, 2, wallMask);
                        Debug.DrawRay(tempHitPos, up, Color.white, 0.5f);

                        if (hitObstacle.collider == null)
                        {
                            tempHitPos += up;

                            hitObstacle = Physics2D.Raycast(tempHitPos, noDirNecessary, 0, combinedMask);
                            //Debug.DrawRay(tempHitPos, noDirNecessary, Color.yellow, 0.5f);

                            if (hitObstacle.collider == null)
                            {
                                GameObject spawnedTile = Instantiate(selectableTiles, transform.position, Quaternion.identity);
                                spawnedTile.transform.position = tempHitPos;
                            }
                        }
                    }
                }

                // North East East
                tempHitPos = transform.position;

                hitObstacle = Physics2D.Raycast(tempHitPos, right, 2, wallMask);
                Debug.DrawRay(tempHitPos, right, Color.white, 0.5f);

                if (hitObstacle.collider == null)
                {
                    tempHitPos += right;

                    hitObstacle = Physics2D.Raycast(tempHitPos, up, 2, wallMask);
                    Debug.DrawRay(tempHitPos, up, Color.white, 0.5f);

                    if (hitObstacle.collider == null)
                    {
                        tempHitPos += up;

                        hitObstacle = Physics2D.Raycast(tempHitPos, right, 2, wallMask);
                        Debug.DrawRay(tempHitPos, right, Color.white, 0.5f);

                        if (hitObstacle.collider == null)
                        {
                            tempHitPos += right;

                            hitObstacle = Physics2D.Raycast(tempHitPos, noDirNecessary, 0, combinedMask);
                            //Debug.DrawRay(tempHitPos, noDirNecessary, Color.yellow, 0.5f);

                            if (hitObstacle.collider == null)
                            {
                                GameObject spawnedTile = Instantiate(selectableTiles, transform.position, Quaternion.identity);
                                spawnedTile.transform.position = tempHitPos;
                            }
                        }
                    }
                }

                // South East East
                tempHitPos = transform.position;

                hitObstacle = Physics2D.Raycast(tempHitPos, right, 2, wallMask);
                Debug.DrawRay(tempHitPos, right, Color.white, 0.5f);

                if (hitObstacle.collider == null)
                {
                    tempHitPos += right;

                    hitObstacle = Physics2D.Raycast(tempHitPos, down, 2, wallMask);
                    Debug.DrawRay(tempHitPos, down, Color.white, 0.5f);

                    if (hitObstacle.collider == null)
                    {
                        tempHitPos += down;

                        hitObstacle = Physics2D.Raycast(tempHitPos, right, 2, wallMask);
                        Debug.DrawRay(tempHitPos, right, Color.white, 0.5f);

                        if (hitObstacle.collider == null)
                        {
                            tempHitPos += right;

                            hitObstacle = Physics2D.Raycast(tempHitPos, noDirNecessary, 0, combinedMask);
                            //Debug.DrawRay(tempHitPos, noDirNecessary, Color.yellow, 0.5f);

                            if (hitObstacle.collider == null)
                            {
                                GameObject spawnedTile = Instantiate(selectableTiles, transform.position, Quaternion.identity);
                                spawnedTile.transform.position = tempHitPos;
                            }
                        }
                    }
                }

                // South South East
                tempHitPos = transform.position;

                hitObstacle = Physics2D.Raycast(tempHitPos, down, 2, wallMask);
                Debug.DrawRay(tempHitPos, down, Color.white, 0.5f);

                if (hitObstacle.collider == null)
                {
                    tempHitPos += down;

                    hitObstacle = Physics2D.Raycast(tempHitPos, right, 2, wallMask);
                    Debug.DrawRay(tempHitPos, right, Color.white, 0.5f);

                    if (hitObstacle.collider == null)
                    {
                        tempHitPos += right;

                        hitObstacle = Physics2D.Raycast(tempHitPos, down, 2, wallMask);
                        Debug.DrawRay(tempHitPos, down, Color.white, 0.5f);

                        if (hitObstacle.collider == null)
                        {
                            tempHitPos += down;

                            hitObstacle = Physics2D.Raycast(tempHitPos, noDirNecessary, 0, combinedMask);
                            //Debug.DrawRay(tempHitPos, noDirNecessary, Color.yellow, 0.5f);

                            if (hitObstacle.collider == null)
                            {
                                GameObject spawnedTile = Instantiate(selectableTiles, transform.position, Quaternion.identity);
                                spawnedTile.transform.position = tempHitPos;
                            }
                        }
                    }
                }

                // South South West
                tempHitPos = transform.position;

                hitObstacle = Physics2D.Raycast(tempHitPos, down, 2, wallMask);
                Debug.DrawRay(tempHitPos, down, Color.white, 0.5f);

                if (hitObstacle.collider == null)
                {
                    tempHitPos += down;

                    hitObstacle = Physics2D.Raycast(tempHitPos, left, 2, wallMask);
                    Debug.DrawRay(tempHitPos, left, Color.white, 0.5f);

                    if (hitObstacle.collider == null)
                    {
                        tempHitPos += left;

                        hitObstacle = Physics2D.Raycast(tempHitPos, down, 2, wallMask);
                        Debug.DrawRay(tempHitPos, down, Color.white, 0.5f);

                        if (hitObstacle.collider == null)
                        {
                            tempHitPos += down;

                            hitObstacle = Physics2D.Raycast(tempHitPos, noDirNecessary, 0, combinedMask);
                            //Debug.DrawRay(tempHitPos, noDirNecessary, Color.yellow, 0.5f);

                            if (hitObstacle.collider == null)
                            {
                                GameObject spawnedTile = Instantiate(selectableTiles, transform.position, Quaternion.identity);
                                spawnedTile.transform.position = tempHitPos;
                            }
                        }
                    }
                }

                // South West West
                tempHitPos = transform.position;

                hitObstacle = Physics2D.Raycast(tempHitPos, left, 2, wallMask);
                Debug.DrawRay(tempHitPos, left, Color.white, 0.5f);

                if (hitObstacle.collider == null)
                {
                    tempHitPos += left;

                    hitObstacle = Physics2D.Raycast(tempHitPos, down, 2, wallMask);
                    Debug.DrawRay(tempHitPos, down, Color.white, 0.5f);

                    if (hitObstacle.collider == null)
                    {
                        tempHitPos += down;

                        hitObstacle = Physics2D.Raycast(tempHitPos, left, 2, wallMask);
                        Debug.DrawRay(tempHitPos, left, Color.white, 0.5f);

                        if (hitObstacle.collider == null)
                        {
                            tempHitPos += left;

                            hitObstacle = Physics2D.Raycast(tempHitPos, noDirNecessary, 0, combinedMask);
                            //Debug.DrawRay(tempHitPos, noDirNecessary, Color.yellow, 0.5f);

                            if (hitObstacle.collider == null)
                            {
                                GameObject spawnedTile = Instantiate(selectableTiles, transform.position, Quaternion.identity);
                                spawnedTile.transform.position = tempHitPos;
                            }
                        }
                    }
                }

                // North West West
                tempHitPos = transform.position;

                hitObstacle = Physics2D.Raycast(tempHitPos, left, 2, wallMask);
                Debug.DrawRay(tempHitPos, left, Color.white, 0.5f);

                if (hitObstacle.collider == null)
                {
                    tempHitPos += left;

                    hitObstacle = Physics2D.Raycast(tempHitPos, up, 2, wallMask);
                    Debug.DrawRay(tempHitPos, up, Color.white, 0.5f);

                    if (hitObstacle.collider == null)
                    {
                        tempHitPos += up;

                        hitObstacle = Physics2D.Raycast(tempHitPos, left, 2, wallMask);
                        Debug.DrawRay(tempHitPos, left, Color.white, 0.5f);

                        if (hitObstacle.collider == null)
                        {
                            tempHitPos += left;

                            hitObstacle = Physics2D.Raycast(tempHitPos, noDirNecessary, 0, combinedMask);
                            //Debug.DrawRay(tempHitPos, noDirNecessary, Color.yellow, 0.5f);

                            if (hitObstacle.collider == null)
                            {
                                GameObject spawnedTile = Instantiate(selectableTiles, transform.position, Quaternion.identity);
                                spawnedTile.transform.position = tempHitPos;
                            }
                        }
                    }
                }

                // North North West
                tempHitPos = transform.position;

                hitObstacle = Physics2D.Raycast(tempHitPos, up, 2, wallMask);
                Debug.DrawRay(tempHitPos, up, Color.white, 0.5f);

                if (hitObstacle.collider == null)
                {
                    tempHitPos += up;

                    hitObstacle = Physics2D.Raycast(tempHitPos, left, 2, wallMask);
                    Debug.DrawRay(tempHitPos, left, Color.white, 0.5f);

                    if (hitObstacle.collider == null)
                    {
                        tempHitPos += left;

                        hitObstacle = Physics2D.Raycast(tempHitPos, up, 2, wallMask);
                        Debug.DrawRay(tempHitPos, up, Color.white, 0.5f);

                        if (hitObstacle.collider == null)
                        {
                            tempHitPos += up;

                            hitObstacle = Physics2D.Raycast(tempHitPos, noDirNecessary, 0, combinedMask);
                            //Debug.DrawRay(tempHitPos, noDirNecessary, Color.yellow, 0.5f);

                            if (hitObstacle.collider == null)
                            {
                                GameObject spawnedTile = Instantiate(selectableTiles, transform.position, Quaternion.identity);
                                spawnedTile.transform.position = tempHitPos;
                            }
                        }
                    }
                }
            }
        }

        void selectedCharacterCycle(int dir)
        {
            // 1 = right, -1 = left
            if (knockedOutCharacters == 0)
            {
                selectedCharacter = Mathf.FloorToInt(nfmod(selectedCharacter + dir, 3));
            }
            else if (knockedOutCharacters == 1)
            {
                if (characterHealths[0] <= 0)
                {
                    selectedCharacter = Mathf.FloorToInt(nfmod(selectedCharacter, 2)) + 1;
                }
                else if (characterHealths[1] <= 0)
                {
                    selectedCharacter = Mathf.FloorToInt(nfmod(selectedCharacter / 2 + 1, 2)) * 2;
                }
                else if (characterHealths[2] <= 0)
                {
                    selectedCharacter = Mathf.FloorToInt(nfmod(selectedCharacter + 1, 2));
                }
            }
        }

        void updateHealthUI()
        {
            // Search for any knocked out characters (those at or below 0 hp)
            knockedOutCharacters = 0;
            
            for (int i = 0; i < 3; i++)
            {
                if (characterHealths[i] <= 0)
                {
                    knockedOutCharacters++;
                }
            }

            // Change character if currently selected character is dead
            if (characterHealths[selectedCharacter] <= 0)
            {
                if (knockedOutCharacters == 1)
                {
                    selectedCharacter = nfmod(selectedCharacter + 1, 3);
                    animator.SetInteger("SelectedCharacter", selectedCharacter);
                    clearExistingTiles();
                    spawnSelectableTiles();
                }
                else if (knockedOutCharacters == 2)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (characterHealths[i] > 0)
                        {
                            selectedCharacter = i;
                            animator.SetInteger("SelectedCharacter", selectedCharacter);
                            clearExistingTiles();
                            spawnSelectableTiles();
                        }
                    }
                }
                else if (knockedOutCharacters == 3)
                {
                    gameObject.SetActive(false);
                    selectedCharacter = -1;
                }

                // If switching to bishop, enable her target icon
                if (selectedCharacter == 1)
                {
                    projectileTargetTile.SetActive(true);
                }
                else
                {
                    projectileTargetTile.SetActive(false);
                }
            }

            // Update UI accordingly
            if (knockedOutCharacters == 0)
            {
                frontHealthBar.fillAmount = Mathf.Ceil( characterHealths[selectedCharacter] / characterHealthsMax[selectedCharacter] * 10 ) / 10;
                //print(Mathf.Ceil(characterHealths[selectedCharacter] / characterHealthsMax[selectedCharacter] * 10) / 10);
                leftHealthBar.fillAmount = Mathf.Ceil( characterHealths[Mathf.FloorToInt(nfmod(selectedCharacter - 1, 3))] / characterHealthsMax[Mathf.FloorToInt(nfmod(selectedCharacter - 1, 3))] * 10 ) / 10;
                rightHealthBar.fillAmount = Mathf.Ceil( characterHealths[Mathf.FloorToInt(nfmod(selectedCharacter + 1, 3))] / characterHealthsMax[Mathf.FloorToInt(nfmod(selectedCharacter + 1, 3))] * 10 ) / 10;
            }
            else if (knockedOutCharacters == 1)
            {
                frontHealthBar.fillAmount = Mathf.Ceil( characterHealths[selectedCharacter] / characterHealthsMax[selectedCharacter] * 10 ) / 10;
                
                // Assess which character is knocked out so the left side bar shows the other remaining character's health
                if (characterHealths[0] <= 0)
                {
                    leftHealthBar.fillAmount = Mathf.Ceil( characterHealths[Mathf.FloorToInt(nfmod(selectedCharacter, 2)) + 1] / characterHealthsMax[Mathf.FloorToInt(nfmod(selectedCharacter, 2)) + 1] * 10 ) / 10;
                }
                else if (characterHealths[1] <= 0)
                {
                    leftHealthBar.fillAmount = Mathf.Ceil( characterHealths[Mathf.FloorToInt(nfmod(selectedCharacter / 2 + 1, 2)) * 2] / characterHealthsMax[Mathf.FloorToInt(nfmod(selectedCharacter / 2 + 1, 2)) * 2] * 10 ) / 10;
                }
                else if (characterHealths[2] <= 0)
                {
                    leftHealthBar.fillAmount = Mathf.Ceil( characterHealths[Mathf.FloorToInt(nfmod(selectedCharacter + 1, 2))] / characterHealthsMax[Mathf.FloorToInt(nfmod(selectedCharacter + 1, 2))] * 10 ) / 10;
                }


                rightHealthBar.fillAmount = 0;
            }
            else if (knockedOutCharacters == 2)
            {
                frontHealthBar.fillAmount = Mathf.Ceil( characterHealths[selectedCharacter] / characterHealthsMax[selectedCharacter] * 10 ) / 10;


                leftHealthBar.fillAmount = 0;
                rightHealthBar.fillAmount = 0;
            }
            else // knockedOutCharacters == 3
            {
                frontHealthBar.fillAmount = 0;
                leftHealthBar.fillAmount = 0;
                rightHealthBar.fillAmount = 0;
            }
        }

        public void takeDamage(int amount)
        {
            characterHealths[selectedCharacter] -= amount;
            if (characterHealths[selectedCharacter] < 0)
            {
                characterHealths[selectedCharacter] = 0;
            }
            else
            {
                StartCoroutine(DamageFlash());
                animator.SetBool("HasBeenDamaged", true);
            }

            updateHealthUI();
        }

        int nfmod(int a,int b)
        {
            return ((a % b) + b) % b;
        }

        float invertScrollConversion(float input)
        {
            return ((input * 2) - 1);
        }

        IEnumerator MovePlayer(Vector2 targetPos, float speedMult)
        {
            movementCoroutineIsActive = true;
            animator.SetBool("IsMoving", true);

            Vector2 startPos = transform.position;
            targetedPos = new Vector3((Mathf.RoundToInt(targetPos.x / 2)) * 2, (Mathf.RoundToInt(targetPos.y / 2)) * 2);
            moveDuration = Vector2.Distance(startPos, targetedPos) * moveModifier;
            float timeElapsed = 0;

            StartCoroutine(ActivateActionCooldownBar(new Color(1, 1, 1, 0.4f), moveDuration));

            if (TEMPcharSwap && prevSelectedChar != selectedCharacter)
            {
                prevSelectedChar = selectedCharacter;
                GameController.instance.charSwaps++;
                TEMPcharSwap = false;
            }

            GameController.instance.totalMoves++;

            GameController.instance.UpdatePlayerStats();

            // Perform the movement
            while (timeElapsed < (moveDuration / speedMult))
            {
                // Move player to desired destination over a fixed period of time
                transform.position = Vector2.Lerp(startPos, targetedPos, timeElapsed / (moveDuration / speedMult));

                timeElapsed += Time.deltaTime * speedMult;
                yield return null;
            }

            transform.position = targetedPos;

            CheckLanding();

            movementCoroutineIsActive = false;
            animator.SetBool("IsMoving", false);
            spawnSelectableTiles();
            NodeController.instance.ReinstantiateNodePoints();
        }

        public void CheckLanding()
        {
            RaycastHit2D hitDetect;
            Collider2D[] hits;
            Vector2 noDirNecessary = transform.TransformDirection(new Vector2(0, 0));

            hitDetect = Physics2D.Raycast(transform.position, noDirNecessary, 0, wallMask);
            Debug.DrawRay(transform.position, noDirNecessary, Color.yellow, 0.5f);

            // If no wall is detected in this spot, then check that spot again specifically for enemies using its coordinates
            if (hitDetect.collider == null)
            {
                // Enemy check
                hits = Physics2D.OverlapCircleAll(transform.position, 0.9f, enemyMask);

                // Handle each hit target in that spot, should there be more than one
                foreach (Collider2D hit in hits)
                {
                    EnemyMasterController enemy = hit.GetComponent<EnemyMasterController>();

                    if (enemy)
                    {
                        print("Landed on enemy");
                        enemy.EnemyTakeDamage("landing");
                    }
                }
            }
        }

        IEnumerator PerformAttack()
        {
            //Vector2 tempHitPos;
            Vector2 tempHitDir;
            //float radianAvg;
            Vector2 tempEndPos;
            RaycastHit2D hitDetect;
            Collider2D[] hits;

            Vector2 noDirNecessary = transform.TransformDirection(new Vector2(0, 0));

            rotationalPivot.transform.eulerAngles = new Vector3(0, 0, facingDirection);



            // Character's individual delay after performing an attack
            float actionDelay = actionDelays[selectedCharacter];

            // Time alloted for character animations
            float animationTime = 0.4f;
            WaitForSeconds delayTime = new WaitForSeconds(animationTime + actionDelay);

            attackCoroutineIsActive = true;
            clearExistingTiles();

            StartCoroutine(ActivateActionCooldownBar(new Color(0.9f, 0.55f, 0.25f, 0.4f), actionDelay + animationTime));

            if (selectedCharacter == 0)
            {
                rookShield.SetActive(true);
                rookShielded = true;
                animator.SetBool("IsAttacking", true);

                for (int i = -1; i < 2; i++)
                {
                    // First check for wall: converts angs to rads for Vector2 dir, gets avg of Vector2 vals to then be used for distance
                    tempHitDir = new Vector2(Mathf.Cos(-i * Mathf.PI / 4 + facingDirection * Mathf.Deg2Rad), Mathf.Sin(-i * Mathf.PI / 4 + facingDirection * Mathf.Deg2Rad));
                    tempEndPos = new Vector2(Mathf.Round(tempHitDir.x) * 2 + transform.position.x, Mathf.Round(tempHitDir.y) * 2 + transform.position.y);

                    hitDetect = Physics2D.Linecast(transform.position, tempEndPos, wallMask);
                    Debug.DrawLine(transform.position, tempEndPos, Color.yellow, 0.5f);

                    // If no wall is detected in this spot, then check that spot again specifically for enemies using its coordinates
                    if (hitDetect.collider == null)
                    {
                        // Enemy check
                        hits = Physics2D.OverlapCircleAll(tempEndPos, 0.9f, enemyMask);

                        // Handle each hit target in that spot, should there be more than one
                        foreach (Collider2D hit in hits)
                        {
                            EnemyMasterController enemy = hit.GetComponent<EnemyMasterController>();

                            if (enemy)
                            {
                                enemy.EnemyTakeDamage("main");
                            }
                        }
                    }
                }
            }
            else if (selectedCharacter == 1)
            {
                animator.SetBool("IsAttacking", true);

                Vector3 directionFromPlayerToTarget = transform.position - projectileTargetTile.transform.position;

                // Derives the radian angle and converts it to degrees for projectile's target direction
                float radiansTowardTarget = Mathf.Atan2(directionFromPlayerToTarget.y, directionFromPlayerToTarget.x);
                float angleTowardTarget = radiansTowardTarget * Mathf.Rad2Deg + 180;

                StartCoroutine(FireballDelay(angleTowardTarget));
            }
            else if (selectedCharacter == 2)
            {
                animator.SetBool("IsAttacking", true);

                // First check for wall: converts dir to rads for Vector2 dir, converts rad to extend the full 2 units
                tempHitDir = new Vector2(Mathf.Cos(facingDirection * Mathf.Deg2Rad), Mathf.Sin(facingDirection * Mathf.Deg2Rad));
                tempEndPos = new Vector2(Mathf.Round(tempHitDir.x) * 2 + transform.position.x, Mathf.Round(tempHitDir.y) * 2 + transform.position.y);

                hitDetect = Physics2D.Linecast(transform.position, tempEndPos, wallMask);
                Debug.DrawLine(transform.position, tempEndPos, Color.yellow, 0.5f);

                // If no wall is detected in this spot, then check that spot again specifically for enemies using its coordinates
                if (hitDetect.collider == null)
                {
                    // Enemy check on this tile
                    hits = Physics2D.OverlapCircleAll(tempEndPos, 0.9f, enemyMask);

                    // Handle each hit target in that spot, should there be more than one
                    foreach (Collider2D hit in hits)
                    {
                        EnemyMasterController enemy = hit.GetComponent<EnemyMasterController>();

                        if (enemy)
                        {
                            enemy.EnemyTakeDamage("main");
                        }
                    }

                    // Cast to the next tile
                    tempEndPos = new Vector2(Mathf.Round(tempHitDir.x) * 2 + tempEndPos.x, Mathf.Round(tempHitDir.y) * 2 + tempEndPos.y);

                    hitDetect = Physics2D.Linecast(transform.position, tempEndPos, wallMask);
                    Debug.DrawLine(transform.position, tempEndPos, Color.yellow, 0.5f);

                    // If no wall is detected in this second spot, then check that spot again specifically for enemies using its coordinates
                    if (hitDetect.collider == null)
                    {
                        // Enemy check on this tile
                        hits = Physics2D.OverlapCircleAll(tempEndPos, 0.9f, enemyMask);

                        // Handle each hit target in that spot, should there be more than one
                        foreach (Collider2D hit in hits)
                        {
                            EnemyMasterController enemy = hit.GetComponent<EnemyMasterController>();

                            if (enemy)
                            {
                                enemy.EnemyTakeDamage("main");
                            }
                        }
                    }
                }
            }

            yield return delayTime;

            animator.SetBool("IsAttacking", false);
            ResetAttackStates();
            spawnSelectableTiles();
            attackCoroutineIsActive = false;
        }

        IEnumerator FireballDelay(float angle)
        {
            // Small delay to match animation
            yield return new WaitForSeconds(0.25f);

            // Spawns in projectile
            GameObject newProjectile = Instantiate(bishopProjectile);
            newProjectile.transform.position = transform.position;
            newProjectile.transform.rotation = Quaternion.Euler(0, 0, angle);
        }






        IEnumerator ActivateActionCooldownBar(Color color, float durationPeriod)
        {
            if (GameController.instance.showCooldown)
            {
                actionCooldownBar.transform.localScale = new Vector3(0f, 1, 1);
                actionCooldownBar.GetComponent<SpriteRenderer>().color = color;
                actionCooldownBarBG.SetActive(true);
                float timeElapsed = 0;

                while (timeElapsed < durationPeriod)
                {
                    // Show duration before next action can be performed
                    actionCooldownBar.transform.localScale = new Vector3(timeElapsed / durationPeriod, 1, 1);

                    timeElapsed += Time.deltaTime;
                    yield return null;
                }

                actionCooldownBarBG.SetActive(false);
            }
            else
            {
                actionCooldownBarBG.SetActive(false);
            }
        }

        void ResetAttackStates()
        {
            rookShield.SetActive(false);
            rookShielded = false;
            //targetTile.SetActive(false);
        }

        IEnumerator DamageFlash()
        {
            WaitForSeconds damagedAnimationTime = new WaitForSeconds(0.5f);

            yield return damagedAnimationTime;
            animator.SetBool("HasBeenDamaged", false);
            
            /* Outdated code
             * spr.color = new Color(1, 0.6f, 0.6f);
            yield return new WaitForSeconds(0.1f);
            spr.color = new Color(1, 1, 1);
            yield return null;*/
        }

        /*IEnumerator IneligibleTarget()
        {
            projectileTargetTile.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.075f);
            yield return new WaitForSeconds(0.1f);
            projectileTargetTile.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.2f);
            yield return null;
        }*/

    }
}


// Unused Knight Movements
/*for (int i = 1; i < 3; i++)
{
    // UPDATE
    RaycastHit2D hitObstacle = Physics2D.Raycast(tempHitPos, up, 2);
    Debug.DrawRay(tempHitPos, up, Color.yellow, 0.5f);

    if(hitObstacle.collider == null)
    {
        // UPDATE
        tempHitPos += up;
        Debug.Log(hitObstacle.point);
        Debug.Log(hitObstacle.collider);

        if (i == 2)
        {
            Vector2 right = transform.TransformDirection(Vector2.right) * 2;

            hitObstacle = Physics2D.Raycast(tempHitPos, right);
            Debug.DrawRay(tempHitPos, right, Color.yellow, 0.5f);

            if (hitObstacle.collider == null)
            {
                GameObject spawnedTile = Instantiate(selectableTiles, transform.position, Quaternion.identity);
                spawnedTile.transform.position = hitObstacle.point;
                knightCaseCounter = true;
            }
            else
            {
                break;
            }
        }
    } 
    else
    {
        break;
    }
}*/

/*if (knightCaseCounter != true)
{
    Vector2 right = transform.TransformDirection(Vector2.right);

    RaycastHit2D hitObstacle = Physics2D.Raycast(transform.position, right);
    Debug.DrawRay(transform.position, right, Color.yellow, 0.5f);

    if (hitObstacle.collider != null)
    {
        Vector2 tempHitPos = new Vector2(transform.position.x + 2, transform.position.y);

        for (int j = 2; j < 6; j += 2)
        {
            Vector2 up = transform.TransformDirection(Vector2.up) * j;

            hitObstacle = Physics2D.Raycast(tempHitPos, up, j);
            Debug.DrawRay(tempHitPos, up, Color.yellow, 0.5f);

            if (hitObstacle.collider == null)
            {
                break;
            }

            if (hitObstacle.collider != null && j == 4)
            {
                GameObject spawnedTile = Instantiate(selectableTiles, transform.position, Quaternion.identity);
                spawnedTile.transform.position = new Vector2(this.transform.position.x + 2, this.transform.position.y + 4);
                knightCaseCounter = false;
            }
            else
            {
                break;
            }
        }
    }
}*/
