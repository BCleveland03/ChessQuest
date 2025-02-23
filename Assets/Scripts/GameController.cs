using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace TopDownGame {
    public class GameController : MonoBehaviour
    {
        // Outlets
        [Header ("Outlets")]
        public static GameController instance;
        public Camera mainCam;
        public GameObject selectedTile;
        public LayerMask tileMask;
        public GameObject endPoint;

        [Header("UI")]
        public Image loadingScreen;
        public TMP_Text loadingText;
        public TMP_Text charSwapCounter;
        public TMP_Text moveCounter;
        public TMP_Text distanceCounter;
        public TMP_Text globalTimerDisplay;

        // State Tracking
        [Header ("Character Info")]
        public int charSwaps = 0;
        public int totalMoves = 0;
        public float distanceFromEnd;
        public bool levelEnded = false;
        public int enemyDespawnDistance;
        
        [Header ("User Settings")]
        public float scrollSensitivity;
        public int invertScroll;
        public bool showCooldown;
        public float sfxVolume;

        [Header("Time and Pause State")]
        public bool firstActionMade = false;
        bool startedCounter = false;
        float unroundedGlobalTimer;
        public float globalTimer = 0;
        public float previousTimeScale = 1;
        public bool isPaused;

        // Configuration
        [Header ("Custom Key Inputs")]
        public KeyCode keyChar0;
        public KeyCode keyChar1;
        public KeyCode keyChar2;

        public KeyCode keyPause;

        // Methods
        void Awake()
        {
            instance = this;

            mainCam = Camera.main;
        }

        void Start()
        {
            UpdatePlayerStats();

            isPaused = false;
            invertScroll = 0;
            showCooldown = true;

            StartCoroutine(FadeAwayLoadingScreen(true));
            StartCoroutine(StartTimer());
        }

        void Update()
        {
            if (startedCounter && !levelEnded)
            {
                // Global time control
                unroundedGlobalTimer += Time.deltaTime;
                globalTimer = Mathf.Ceil(unroundedGlobalTimer * 10) / 10;
            }

            // Distance from player to end value
            distanceFromEnd = Mathf.Round(Vector2.Distance(PlayerController.instance.transform.position, endPoint.transform.position) * 5) / 10;

            // Pause toggle
            if (Input.GetKeyDown(keyPause))
            {
                MenuController.instance.TogglePause();
            }

            // TEMP scene reset
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene("LevelT-2");
            }

            UpdateWorldStats();
            CheckForSelectableTileMouseHover();
        }

        // Checks if this is an integer
        public bool IsThisInteger(float floatVal)
        {
            return Mathf.Approximately(floatVal, Mathf.RoundToInt(floatVal));
        }

        // Detection for when mouse is hovering over selectable tiles; set to trigger hover animation

        public void CheckForSelectableTileMouseHover()
        {
            RaycastHit2D[] rayCheck = Physics2D.GetRayIntersectionAll(mainCam.ScreenPointToRay(Mouse.current.position.ReadValue()), tileMask);
            for (int i = 0; i < rayCheck.Length; i++)
            {
                if (!rayCheck[i].collider) continue;

                if (rayCheck[i].collider.gameObject.tag == "SelectableTiles")
                {

                }
            }
        }

        // Left click detection for selectable tiles
        public void OnLeftClick(InputAction.CallbackContext context)
        {
            if (!context.started) return;

            RaycastHit2D[] rayHit = Physics2D.GetRayIntersectionAll(mainCam.ScreenPointToRay(Mouse.current.position.ReadValue()), tileMask);
            for(int i = 0; i < rayHit.Length; i++)
            {
                if (!rayHit[i].collider) continue;

                if (rayHit[i].collider.gameObject.tag == "SelectableTiles")
                {
                    selectedTile = rayHit[i].collider.gameObject;
                }
            }
        }

        public void UpdateWorldStats()
        {
            // Timer display
            if (globalTimer < 999.9)
            {
                if (IsThisInteger(globalTimer))
                {
                    globalTimerDisplay.text = globalTimer + ".0";
                }
                else
                {
                    globalTimerDisplay.text = globalTimer + "";
                }
            }
            else
            {
                globalTimerDisplay.text = "999.9";
            }

            // End of level tracker
            if (distanceFromEnd < 999.9)
            {
                distanceCounter.text = "Distance: " + distanceFromEnd;
            }
            else
            {
                distanceCounter.text = "Distance: 999.9";
            }
        }

        // Updates moves and swaps with each action
        public void UpdatePlayerStats()
        {
            if (charSwaps < 1000)
            {
                charSwapCounter.text = "Swaps: " + charSwaps.ToString();
            }
            else
            {
                charSwapCounter.text = "Swaps: 999+";
            }

            if (totalMoves < 1000)
            {
                moveCounter.text = "Moves: " + totalMoves.ToString();
            }
            else
            {
                moveCounter.text = "Moves: 999+";
            }
        }

        public void InitiateFade(bool fadeaway)
        {
            StartCoroutine(FadeAwayLoadingScreen(fadeaway));
        }

        IEnumerator StartTimer()
        {
            while (!firstActionMade)
            {
                yield return null;
            }

            startedCounter = true;
        }

        IEnumerator FadeAwayLoadingScreen(bool fadeaway)
        {
            WaitForSeconds fadeSpeed = new WaitForSeconds(0.025f);
            
            if (fadeaway)
            {
                for (int i = 10; i > 0; i --)
                {
                    loadingScreen.color = new Color(1, 1, 1, i / 10f);
                    loadingText.color = new Color(1, 1, 1, i / 10f);
                    yield return fadeSpeed;
                }

                loadingScreen.color = new Color(1, 1, 1, 0);
                loadingText.color = new Color(1, 1, 1, 0);
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    loadingScreen.color = new Color(1, 1, 1, i / 10f);
                    loadingText.color = new Color(1, 1, 1, i / 10f);
                    yield return fadeSpeed;
                }

                loadingScreen.color = new Color(1, 1, 1, 1);
                loadingText.color = new Color(1, 1, 1, 1);

                // Temp set up; will reset scene
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
}
