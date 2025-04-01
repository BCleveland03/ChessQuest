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

        [Header ("Scene List")]
        public List<string> sceneList = new List<string>();
        public int currentScene;

        [Header("UI")]
        public Canvas activeCanvas;
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
            activeCanvas.gameObject.SetActive(true);
        }

        void Start()
        {
            UpdatePlayerStats();
            MenuController.instance.LoadSettingPrefs();

            isPaused = false;

            loadingScreen.gameObject.SetActive(true);
            StartCoroutine(FadeAwayLoadingScreen(true));
            StartCoroutine(StartTimer());

            // Check to get active scene in list
            for (int i = 0; i < sceneList.Count; i++)
            {
                if (sceneList[i] == SceneManager.GetActiveScene().name)
                {
                    currentScene = i;
                    break;
                }
            }
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
            if (Input.GetKeyDown(keyPause) && MenuController.instance.pauseState != 3)
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

        public void InitiateFade(bool fadeaway, bool fadeToTitle)
        {
            if (fadeToTitle)
            {
                StartCoroutine(ReturnToTitle());
            }
            else
            {
                StartCoroutine(FadeAwayLoadingScreen(fadeaway));
            }
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
                // Fade out // disappear
                for (int i = 10; i > 0; i--)
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
                // Fade in / appear
                for (int i = 0; i < 10; i++)
                {
                    loadingScreen.color = new Color(1, 1, 1, i / 10f);
                    loadingText.color = new Color(1, 1, 1, i / 10f);
                    yield return fadeSpeed;
                }

                loadingScreen.color = new Color(1, 1, 1, 1);
                loadingText.color = new Color(1, 1, 1, 1);

                // Reaching the end will advance you to the next scene, or back to the first scene in the list if there is none
                // This system is temp; will eventually use 0 - Title, 1 - Level Select, 2-4 - prev. to next level
                SceneManager.LoadScene(sceneList[(currentScene + 1) % sceneList.Count]);
            }
        }

        IEnumerator ReturnToTitle()
        {
            WaitForSecondsRealtime fadeSpeed = new WaitForSecondsRealtime(0.025f);

            // Fade in / appear
            for (int i = 0; i < 10; i++)
            {
                loadingScreen.color = new Color(1, 1, 1, i / 10f);
                loadingText.color = new Color(1, 1, 1, i / 10f);
                yield return fadeSpeed;
            }

            loadingScreen.color = new Color(1, 1, 1, 1);
            loadingText.color = new Color(1, 1, 1, 1);

            SceneManager.LoadScene("Title");
        }
    }
}
