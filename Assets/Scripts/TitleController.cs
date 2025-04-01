using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace TopDownGame
{
    public class TitleController : MonoBehaviour
    {
        [Header("Title Outlets")]
        public RectTransform logoRibbon;
        public RectTransform buttonContainer;
        public GameObject optionsContainer;

        [Header("Screenspace Outlets")]
        public Grid foregroundGrid;
        public Grid midgroundGrid;
        public Grid backgroundGrid;

        public Image loadingScreen;
        public TMP_Text loadingText;

        [Header("Grid Speeds")]
        private float foregroundSpeed;
        private float midgroundSpeed;
        private float backgroundSpeed;

        private float modifier = 1.2f;
        
        // Start is called before the first frame update
        void Start()
        {
            optionsContainer.SetActive(false);

            foregroundSpeed = 0;
            midgroundSpeed = 0;
            backgroundSpeed = 0;

            StartCoroutine(SpeedIncrease());
            StartCoroutine(BackgroundScroll());
            StartCoroutine(ButtonsDelay());

            print(PlayerPrefs.GetString("GameState_ContinueLevel"));
        }

        // Update is called once per frame
        void Update()
        {
            /*if (foregroundSpeed < 0.01)
            {
                foregroundSpeed += 0.00001f;
            }*/
        }

        public void InitiatePlay()
        {
            StartCoroutine(FadeLoadingScreen(false, "LevelT-1"));
        }

        public void InitateContinue()
        {
            StartCoroutine(FadeLoadingScreen(false, PlayerPrefs.GetString("GameState_ContinueLevel")));
        }

        public void InitiateOptions()
        {
            StartCoroutine(ButtonsSlideUp(false));
            StartCoroutine(LogoSlideUp(true));
        }

        public void InitiateOptionsReturn()
        {
            StartCoroutine(ButtonsSlideUp(true));
            StartCoroutine(LogoSlideUp(false));
        }

        IEnumerator FadeLoadingScreen(bool fadeaway, string sceneName)
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
                SceneManager.LoadScene(sceneName);
            }
        }

        IEnumerator SpeedIncrease()
        {
            WaitForSeconds speedIncreaseDelay = new WaitForSeconds(0.1f);

            // Will complete when foreground reaches 0.02, midground reaches 0.015, background reaches 0.01
            while (foregroundSpeed < 0.02)
            {
                foregroundSpeed += 0.0005f;
                midgroundSpeed += 0.000375f;
                backgroundSpeed += 0.00025f;
                yield return speedIncreaseDelay;
            }
        }

        IEnumerator BackgroundScroll()
        {
            while (10 < 100)
            {
                foregroundGrid.transform.position -= new Vector3(foregroundSpeed * modifier, 0, 0);
                if (foregroundGrid.transform.position.x <= -132)
                {
                    foregroundGrid.transform.position = new Vector3(foregroundGrid.transform.position.x + 132, 0, 0);
                }

                midgroundGrid.transform.position -= new Vector3(midgroundSpeed * modifier, 0, 0);
                if (midgroundGrid.transform.position.x <= -149.5f)
                {
                    midgroundGrid.transform.position = new Vector3(midgroundGrid.transform.position.x + 149.5f, 0.5f, 0);
                }

                backgroundGrid.transform.position -= new Vector3(backgroundSpeed * modifier, 0, 0);
                if (backgroundGrid.transform.position.x <= -139.25f)
                {
                    backgroundGrid.transform.position = new Vector3(backgroundGrid.transform.position.x + 139.25f, 0.75f, 0);
                }

                yield return null;
            }
        }

        IEnumerator ButtonsDelay()
        {
            yield return new WaitForSeconds(0.6f);
            StartCoroutine(ButtonsSlideUp(true));
        }

        IEnumerator ButtonsSlideUp(bool slideUp)
        {
            WaitForSeconds movementDelay = new WaitForSeconds(0.015f);

            if (slideUp)
            {
                for (float i = -20; i < 2.5f; i += 0.5f)
                {
                    buttonContainer.anchoredPosition = new Vector3(0, (0.1f * Mathf.Pow(i, 3)) + (Mathf.Pow(i, 2)) - i - 5, 0);
                    yield return movementDelay;
                }

                buttonContainer.anchoredPosition = Vector3.zero;
            }
            else
            {
                for (float i = 2.5f; i > -20; i -= 0.5f)
                {
                    buttonContainer.anchoredPosition = new Vector3(0, (0.1f * Mathf.Pow(i, 3)) + (Mathf.Pow(i, 2)) - i - 5, 0);
                    yield return movementDelay;
                }
            }
        }

        IEnumerator LogoSlideUp(bool slideUp)
        {
            WaitForSeconds movementDelay = new WaitForSeconds(0.02f);

            if (slideUp)
            {
                for (int i = 0; i < 24; i++)
                {
                    logoRibbon.anchoredPosition = new Vector3(0, 0.05f * Mathf.Pow(i, 3), 0);
                    yield return movementDelay;
                }

                yield return new WaitForSeconds(0.2f);

                //StartCoroutine(ShowOptionsMenu(true));
                optionsContainer.SetActive(true);
                MenuController.instance.pauseState = 2;
                MenuController.instance.LoadSettingPrefs();
            }
            else
            {
                //StartCoroutine(ShowOptionsMenu(false));
                MenuController.instance.pauseState = -1;
                MenuController.instance.SaveSettingPrefs();
                optionsContainer.SetActive(false);

                for (int i = 24; i > 0; i--)
                {
                    logoRibbon.anchoredPosition = new Vector3(0, 0.05f * Mathf.Pow(i, 3), 0);
                    yield return movementDelay;
                }

                logoRibbon.anchoredPosition = Vector3.zero;
            }
        }

        /*IEnumerator ShowOptionsMenu(bool show)
        {
            WaitForSeconds fadeDelay = new WaitForSeconds(0.01f);

            if (show)
            {
                for (int i = 0; i < 10; i++)
                {

                }
            }
            yield return null;
        }*/
    }
}
