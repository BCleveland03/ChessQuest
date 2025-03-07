using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace TopDownGame
{
    public class MenuController : MonoBehaviour
    {
        // Outlets
        public static MenuController instance;
        public Image pauseOverlay;
        public GameObject menuPanel;

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

        // State Tracking
        bool pauseAnimationCoroutineIsActive;
        private int invertScrollCounter = 0;
        private int pauseState;

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            menuPanel.SetActive(false);
            pauseContainer.SetActive(false);
            optionsContainer.SetActive(false);
            pauseAnimationCoroutineIsActive = false;
        }

        void Update()
        {
            // Only runs when options is pulled up
            if (pauseState == 2)
            {
                scrollSensValue.text = "" + scrollSensSlider.value;
                GameController.instance.scrollSensitivity = (int)(scrollSensSlider.value);
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
            pauseState = 0;
            StartCoroutine(ShowPauseMenu());

            GameController.instance.isPaused = true;
        }

        public void UnpauseGame()
        {
            Time.timeScale = GameController.instance.previousTimeScale;
            AudioListener.pause = false;
            pauseState = -1;
            StartCoroutine(HidePauseMenu());

            SaveSettingPrefs();

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
        }

        public void SaveSettingPrefs()
        {
            PlayerPrefs.SetInt("Setting_ScrollSensitivity", GameController.instance.scrollSensitivity);
            PlayerPrefs.SetInt("Setting_InvertScroll", GameController.instance.invertScroll);

            if (GameController.instance.showCooldown)
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
            print(PlayerPrefs.GetInt("Setting_ScrollSensitivity"));
            print(PlayerPrefs.GetInt("Setting_InvertScroll"));
            print(PlayerPrefs.GetInt("Setting_ShowCooldown"));

            GameController.instance.scrollSensitivity = PlayerPrefs.GetInt("Setting_ScrollSensitivity", 5);
            scrollSensSlider.value = GameController.instance.scrollSensitivity;
            
            //GameController.instance.invertScroll = PlayerPrefs.GetInt("Setting_ScrollSensitivity", 0);

            if (PlayerPrefs.HasKey("Setting_InvertScroll"))
            {
                int getToggleGraphic = PlayerPrefs.GetInt("Setting_InvertScroll");

                if (getToggleGraphic == 1)
                {
                    invertScrollToggle.isOn = true;
                    invertScrollCounter = 1;
                    GameController.instance.invertScroll = 1;
                }
                else if (getToggleGraphic == 0)
                {
                    invertScrollToggle.isOn = false;
                    invertScrollCounter = 0;
                    GameController.instance.invertScroll = 0;
                }
            }
            else
            {
                // Default invert scroll state
                invertScrollToggle.isOn = false;
                invertScrollCounter = 0;
                GameController.instance.invertScroll = 0;
            }

            if (PlayerPrefs.HasKey("Setting_ShowCooldown"))
            {
                int fetchedShowCooldown = PlayerPrefs.GetInt("Setting_ShowCooldown");

                if (fetchedShowCooldown == 1)
                {
                    showCooldownBarToggle.isOn = true;
                    GameController.instance.showCooldown = true;
                }
                else if (fetchedShowCooldown == 0)
                {
                    showCooldownBarToggle.isOn = false;
                    GameController.instance.showCooldown = false;
                }
            }
            else
            {
                // Default show cooldown state
                showCooldownBarToggle.isOn = true;
                GameController.instance.showCooldown = true;
            }
        }

        IEnumerator ShowPauseMenu()
        {
            // Returns size to default
            RectTransform pausePanelTransform = menuPanel.GetComponent<RectTransform>();
            pausePanelTransform.sizeDelta = new Vector2(410, 490);

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
            pauseContainer.SetActive(true);

            // Declares coroutine conclusion
            pauseAnimationCoroutineIsActive = false;
            yield break;
        }

        IEnumerator HidePauseMenu()
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
            GameController.instance.invertScroll = invertScrollCounter % 2;
        }

        public void ShowCooldownBarToggle()
        {
            GameController.instance.showCooldown = !GameController.instance.showCooldown;
        }
    }
}
