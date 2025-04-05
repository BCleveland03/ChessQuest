using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TopDownGame
{
    public class Projectile : MonoBehaviour
    {
        // Outlets
        [Header("Display Connections")]
        public Transform spritePivot;
        public GameObject spriteDisplay;

        [Header("Physics Connections")]
        Rigidbody2D proj;


        // Configuration
        [Header("Configuration")]
        public float projSpeed;

        public LayerMask enemyMask;
        public LayerMask playerMask;
        public LayerMask enemyAndPlayerMask;

        // State Tracking
        public bool belongsToPlayer;
        bool fireballSpent = false;

        // Methods
        void Start()
        {
            proj = GetComponent<Rigidbody2D>();
            proj.velocity = transform.right * projSpeed;
            //GetComponentInChildren<Transform>().eulerAngles = new Vector3(0, 0, transform.parent.rotation.z + (transform.parent.rotation.z * -1));
            spritePivot.rotation = Quaternion.Euler(0, 0, transform.rotation.z + (transform.rotation.z * -1));

            // Spawn at correct layer
            spriteDisplay.GetComponent<SpriteRenderer>().sortingOrder = (int)(transform.position.y * -10 - 1);
        }

        void Update()
        {
            // Checks if the distance from the player exceeds the distance between the target's position from the player (with some extra room)
            if (Vector2.Distance(transform.position, PlayerController.instance.transform.position) 
                > Vector2.Distance(PlayerController.instance.projectileTargetTile.transform.position, PlayerController.instance.transform.position) + 2)
            {
                proj.velocity = Vector2.zero;
                spriteDisplay.GetComponent<Animator>().SetTrigger("Explode");
                Destroy(gameObject, 0.34f);
            }

            // Update projectile layer
            spriteDisplay.GetComponent<SpriteRenderer>().sortingOrder = (int)(transform.position.y * -10 + 6);
        }

        // Destroys when it comes into contact with something
        void OnCollisionEnter2D()
        {
            if (!fireballSpent)
            {
                fireballSpent = true;
                Collider2D[] collisions = Physics2D.OverlapCircleAll(transform.position, 0.75f, enemyMask);

                for (int i = 0; i < collisions.Length; i++)
                {
                    if (collisions[i].gameObject.layer == LayerMask.NameToLayer("Enemy"))
                    {
                        //Collider2D hit = Physics2D.OverlapCircle(transform.position, 0.75f, enemyMask);
                        //print("Dealt damage to enemy");
                        //print(collisions[i].gameObject.name);

                        EnemyMasterController enemy = collisions[i].GetComponent<EnemyMasterController>();
                        enemy.EnemyTakeDamage("main", 1); // collisions.Length);
                    }
                    
                    if (collisions[i].gameObject.layer == LayerMask.NameToLayer("Destructable"))
                    {
                        DestructableObject destructableObj = collisions[i].GetComponent<DestructableObject>();
                        destructableObj.DamageObject();
                        break;
                    }
                }
                //print(collisions.Length);

                // Switch to fireball collision animation
                proj.velocity = Vector2.zero;
                spriteDisplay.GetComponent<Animator>().SetTrigger("Explode");
                Destroy(gameObject, 0.34f);
            }
        }
    }
}
