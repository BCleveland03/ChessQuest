using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace TopDownGame
{
    public class SceneCompletionController : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag == "Player")
            {
                // Stop time and revoke control for end-of-level sequence
                GameController.instance.levelEnded = true;
                print("Level complete!");
                print("Insert transition out of level.");

                GameController.instance.InitiateFade(false);
            }
        }

        void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.tag == "Player")
            {
                GameController.instance.levelEnded = false;
                print("Left level completion zone. This won't be an intended thing you can do later.");
            }
        }
    }
}
