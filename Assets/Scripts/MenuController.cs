using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace TopDownGame
{
    public class MenuController : MonoBehaviour
    {
        [Header("Outlets")]
        public static MenuController instance;
        public Image pauseOverlay;
        public GameObject menuPanel;
        public int pauseState = -1;
        public bool returnToTitle = false;

        [Header("User Settings")]
        public int scrollSensitivity;
        public int invertScroll;
        public bool showCooldown;
        public float sfxVolume;

        [Header ("Pause Menu")]
        public GameObject pauseContainer;
        public GameObject pauseLabel;
        public GameObject resumeButton;
        public GameObject optionsButton;
        public GameObject menuButton;

        [Header("Options Menu")]
        public GameObject optionsContainer;
        public GameObject optionsBackButton;
        public GameObject optionsLabel;
        public GameObject scrollSensLabel;
        public Slider scrollSensSlider;
        public TMP_Text scrollSensValue;
        public GameObject invertScrollLabel;
        public Toggle invertScrollToggle;
        public GameObject showCooldownBarLabel;
        public Toggle showCooldownBarToggle;

        [Header("Level Completion Menu")]
        public GameObject completionContainer;

        // State Tracking
        bool pauseAnimationCoroutineIsActive;
        private int invertScrollCounter = 0;

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            if (SceneManager.GetActiveScene().name != "Title")
            {
                menuPanel.SetActive(false);
                pauseContainer.SetActive(false);
                optionsContainer.SetActive(false);
                completionContainer.SetActive(false);
                pauseAnimationCoroutineIsActive = false;

                PlayerPrefs.SetString("GameState_ContinueLevel", "" + SceneManager.GetActiveScene().name);
            }
        }

        void Update()
        {
            // Only runs when options is pulled up
            if (pauseState == 2)
            {
                scrollSensValue.text = "" + scrollSensSlider.value;
                scrollSensitivity = (int)(scrollSensSlider.value);
            }
        }

        // Pause toggle
        public void TogglePause()
        {
            if (Time.timeScale > 0 && !pauseAnimationCoroutineIsActive)
            {
                print("Paused");
                PauseGame();
            }
            else if (Time.timeScale == 0 && !pauseAnimationCoroutineIsActive)
            {
                print("Unpaused");
                UnpauseGame();
            }
        }

        public void PauseGame()
        {
            // Resets sub-pause UI elements
            optionsContainer.SetActive(false);

            GameController.instance.previousTimeScale = Time.timeScale;
            Time.timeScale = 0;
            AudioListener.pause = true;
            pauseState = 1;
            StartCoroutine(ShowMenu());

            GameController.instance.isPaused = true;
        }

        public void UnpauseGame()
        {
            Time.timeScale = GameController.instance.previousTimeScale;
            AudioListener.pause = false;
            pauseState = -1;
            StartCoroutine(HideMenu());

            GameController.instance.isPaused = false;
        }

        public void OpenOptions()
        {
            pauseState = 2;
            StartCoroutine(GoToOptionsMenu());
        }

        public void CloseOptions()
        {
            pauseState = 1;
            StartCoroutine(ReturnFromOptionsMenu());

            SaveSettingPrefs();
        }

        public void LevelCompletion()
        {
            GameController.instance.previousTimeScale = Time.timeScale;
            //Time.timeScale = 0;
            AudioListener.pause = true;
            pauseState = 3;
            StartCoroutine(ShowMenu());

            GameController.instance.isPaused = true;
        }

        public void ContinueToNextLevel()
        {
            print("Insert transition out of level.");
            GameController.instance.InitiateFade(false, false);
            PlayerPrefs.SetInt("LevelStat_SelectedCharacter", PlayerController.instance.selectedCharacter);
        }

        public void ReturnToTitle()
        {
            Time.timeScale = GameController.instance.previousTimeScale;
            AudioListener.pause = false;

            // Fix this; needs to save the next scene in list for continue
            if (pauseState == 3)
            {
                if(GameController.instance.sceneList[(GameController.instance.currentScene + 1) % GameController.instance.sceneList.Count] == "Title")
                {
                    PlayerPrefs.SetString("GameState_ContinueLevel", "LevelT-1");
                    print("Scene Name: LevelT-1");
                }
                else
                {
                    PlayerPrefs.SetString("GameState_ContinueLevel", "" + GameController.instance.sceneList[(GameController.instance.currentScene + 1) % GameController.instance.sceneList.Count]);
                    print("Scene Name: " + GameController.instance.sceneList[(GameController.instance.currentScene + 1) % GameController.instance.sceneList.Count]);
                }
            }
            else
            {
                PlayerPrefs.SetString("GameState_ContinueLevel", "" + SceneManager.GetActiveScene().name);
            }
            //print("" + SceneManager.GetActiveScene());

            pauseState = -1;
            GameController.instance.InitiateFade(false, true);
        }

        public void SaveSettingPrefs()
        {
            PlayerPrefs.SetInt("Setting_ScrollSensitivity", scrollSensitivity);
            PlayerPrefs.SetInt("Setting_InvertScroll", invertScroll);

            if (showCooldown)
            {
                PlayerPrefs.SetInt("Setting_ShowCooldown", 1);
            }
            else
            {
                PlayerPrefs.SetInt("Setting_ShowCooldown", 0);
            }

            // Saves all established settings above
            PlayerPrefs.Save();
        }

        public void LoadSettingPrefs()
        {
            /*print(PlayerPrefs.GetInt("Setting_ScrollSensitivity"));
            print(PlayerPrefs.GetInt("Setting_InvertScroll"));
            print(PlayerPrefs.GetInt("Setting_ShowCooldown"));*/

            scrollSensitivity = PlayerPrefs.GetInt("Setting_ScrollSensitivity", 5);
            scrollSensSlider.value = scrollSensitivity;
            
            //GameController.instance.invertScroll = PlayerPrefs.GetInt("Setting_ScrollSensitivity", 0);

            if (PlayerPrefs.HasKey("Setting_InvertScroll"))
            {
                int getToggleGraphic = PlayerPrefs.GetInt("Setting_InvertScroll");

                if (getToggleGraphic == 1)
                {
                    invertScrollToggle.isOn = true;
                    invertScrollCounter = 1;
                    invertScroll = 1;
                }
                else if (getToggleGraphic == 0)
                {
                    invertScrollToggle.isOn = false;
                    invertScrollCounter = 0;
                    invertScroll = 0;
                }
            }
            else
            {
                // Default invert scroll state
                invertScrollToggle.isOn = false;
                invertScrollCounter = 0;
                invertScroll = 0;
            }

            if (PlayerPrefs.HasKey("Setting_ShowCooldown"))
            {
                int fetchedShowCooldown = PlayerPrefs.GetInt("Setting_ShowCooldown");

                if (fetchedShowCooldown == 1)
                {
                    showCooldownBarToggle.isOn = true;
                    showCooldown = true;
                }
                else if (fetchedShowCooldown == 0)
                {
                    showCooldownBarToggle.isOn = false;
                    showCooldown = false;
                }
            }
            else
            {
                // Default show cooldown state
                showCooldownBarToggle.isOn = true;
                showCooldown = true;
            }
        }

        IEnumerator ShowMenu()
        {
            // Returns size to default
            RectTransform pausePanelTransform = menuPanel.GetComponent<RectTransform>();
            if (pauseState == 1)
            {
                pausePanelTransform.sizeDelta = new Vector2(410, 490);
            }
            else if (pauseState == 3)
            {
                pausePanelTransform.sizeDelta = new Vector2(480, 520);
            }

            // Declares coroutine activation and revokes player input
            pauseAnimationCoroutineIsActive = true;
            GameController.instance.isPaused = true;

            if (!PlayerController.instance.movementCoroutineIsActive)
            {
                PlayerController.instance.clearExistingTiles();
            }

            // Darken background and enable pause panel
            WaitForSecondsRealtime alphaAnimationPeriod = new WaitForSecondsRealtime(0.005f);

            for (float i = 0f; i < 0.55f; i += 0.05f)
            {
                pauseOverlay.color = new Color(0, 0, 0, i);
                yield return alphaAnimationPeriod;
            }

            menuPanel.SetActive(true);
            if (pauseState == 1)
            {
                pauseContainer.SetActive(true);
            }
            else if (pauseState == 3)
            {
                completionContainer.SetActive(true);
            }

            // Declares coroutine conclusion
            pauseAnimationCoroutineIsActive = false;
            yield break;
        }

        IEnumerator HideMenu()
        {
            // Declares coroutine activation
            pauseAnimationCoroutineIsActive = true;

            // Lighten background and disable pause panel
            WaitForSecondsRealtime alphaAnimationPeriod = new WaitForSecondsRealtime(0.005f);

            menuPanel.SetActive(false);
            pauseContainer.SetActive(false);
            optionsContainer.SetActive(false);

            EventSystem.current.SetSelectedGameObject(null);

            for (float i = 0.5f; i > -0.05f; i -= 0.05f)
            {
                pauseOverlay.color = new Color(0, 0, 0, i);
                yield return alphaAnimationPeriod;
            }

            // Declares coroutine conclusion and restores player functionality
            if (!PlayerController.instance.movementCoroutineIsActive)
            {
                PlayerController.instance.spawnSelectableTiles();
            }

            GameController.instance.isPaused = false;
            pauseAnimationCoroutineIsActive = false;
            yield break;
        }

        IEnumerator GoToOptionsMenu()
        {
            RectTransform pausePanelTransform = menuPanel.GetComponent<RectTransform>();
            WaitForSecondsRealtime resizeAnimationPeriod = new WaitForSecondsRealtime(0.005f);


            // Disables main pause interface
            pauseContainer.SetActive(false);

            // Resizes panel to be larger
            for (int i = 0; i < 30; i++)
            {
                pausePanelTransform.sizeDelta = new Vector2(410 + i * 2.6f, 490 + i * 2.8f);
                yield return resizeAnimationPeriod;
            }

            // Enables options interface
            optionsContainer.SetActive(true);

            yield break;
        }

        IEnumerator ReturnFromOptionsMenu()
        {
            RectTransform pausePanelTransform = menuPanel.GetComponent<RectTransform>();
            WaitForSecondsRealtime resizeAnimationPeriod = new WaitForSecondsRealtime(0.005f);


            // Disables options interface
            optionsContainer.SetActive(false);

            // Resizes panel to be regular size
            for (int i = 0; i < 30; i++)
            {
                pausePanelTransform.sizeDelta = new Vector2(488 - i * 2.6f, 574 - i * 2.4f);
                yield return resizeAnimationPeriod;
            }

            // Enables main pause interface
            pauseContainer.SetActive(true);

            yield break;
        }

        public void InvertScrollToggle()
        {
            invertScrollCounter++;
            invertScroll = invertScrollCounter % 2;
        }

        public void ShowCooldownBarToggle()
        {
            showCooldown = !showCooldown;
        }
    }
}
