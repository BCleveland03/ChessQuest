using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TopDownGame
{
    public class ParticleEffects : MonoBehaviour
    {
        // Outlets
        [Header("Display Connections")]
        public GameObject particleDisplay;
        public GameObject pivotOffset;

        [Header("Physics Connections")]
        Rigidbody2D particlePhys;

        // Configuration
        [Header("Configuration")]
        public float particleProjSpeed;
        public int layerOffset = 6;
        public int inMomentSelectedCharacter;
        public int playerFacingDirection;
        public int intToBooloolSidesOrTops;
        public bool isTops;

        public LayerMask enemyMask;
        public LayerMask wallMask;

        // Set up particle based on which it is
        void Start()
        {
            inMomentSelectedCharacter = PlayerController.instance.selectedCharacter;
            print(inMomentSelectedCharacter);
            playerFacingDirection = PlayerController.instance.facingDirection;


            if (inMomentSelectedCharacter == 0)
            {
                // Disables collision and spawn offset for deflect
                GetComponent<CircleCollider2D>().enabled = false;
                pivotOffset.GetComponent<Transform>().localPosition = Vector3.zero;
                
                // Convert the player's facing direction into a boolean, where left/right is false and up/down is true
                intToBooloolSidesOrTops = Mathf.RoundToInt(playerFacingDirection % 180 / 90);
                isTops = intToBooloolSidesOrTops == 1;
                if (intToBooloolSidesOrTops == 1)
                {
                    isTops = true;
                }
                else
                {
                    isTops = false;
                }
                Debug.Log(isTops);

                particleDisplay.GetComponent<Animator>().SetBool("Tops?", isTops);
                transform.rotation = Quaternion.Euler(0, 0, 0);

                // Determine if the side the animation shows is already on the native side, if not, flip axis
                if (isTops)
                {
                    if (playerFacingDirection == 270)
                    {
                        particleDisplay.GetComponent<SpriteRenderer>().flipY = true;
                    }
                    else if (playerFacingDirection == 90)
                    {
                        layerOffset = 0;
                    }
                }
                else if (!isTops && playerFacingDirection == 0)
                {
                    particleDisplay.GetComponent<SpriteRenderer>().flipX = true;
                }
            }
            else if (inMomentSelectedCharacter == 2)
            {
                particlePhys = GetComponent<Rigidbody2D>();
                particlePhys.velocity = transform.right * particleProjSpeed;
            }

            // Set the selected character in animator and sorting order in game
            particleDisplay.GetComponent<Animator>().SetInteger("SelectedCharacter", inMomentSelectedCharacter);
            particleDisplay.GetComponent<Animator>().SetTrigger("SpawnParticle");
            particleDisplay.GetComponent<SpriteRenderer>().sortingOrder = (int)(transform.position.y * -10 - 1);
        }

        void Update()
        {
            if (inMomentSelectedCharacter == 0)
            {
                Destroy(gameObject, 1.3f);
            }
            else if (inMomentSelectedCharacter == 2)
            {
                // Checks if the distance from the player nears its two-tile distance limit
                if (Vector2.Distance(transform.position, PlayerController.instance.transform.position) > 0.05)
                {
                    //particlePhys.velocity = Vector2.zero;
                    particleDisplay.GetComponent<Animator>().SetTrigger("Hit");
                    Destroy(gameObject, 0.24f);
                }
            }
            
            // Update the sorting order
            particleDisplay.GetComponent<SpriteRenderer>().sortingOrder = (int)(transform.position.y * -10 + layerOffset);
        }

        // End slash prematurely if it collides with a wall
        void OnCollisionEnter2D()
        {
            Collider2D[] collisions = Physics2D.OverlapCircleAll(transform.position, 0.5f, wallMask);

            for (int i = 0; i < collisions.Length; i++)
            {
                if (collisions[i].gameObject.layer == LayerMask.NameToLayer("Wall"))
                {
                    // End slash animation early
                    particlePhys.velocity = Vector2.zero;
                    particleDisplay.GetComponent<Animator>().SetTrigger("Hit");
                    Destroy(gameObject, 0.24f);
                }
            }
        }
    }
}
