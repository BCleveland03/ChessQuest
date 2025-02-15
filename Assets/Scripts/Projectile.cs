using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TopDownGame
{
    public class Projectile : MonoBehaviour
    {
        // Outlets
        Rigidbody2D proj;
        public GameObject display;
        public SpriteRenderer sprite;

        // Configuration
        public float projSpeed;

        public LayerMask enemyMask;
        public LayerMask playerMask;
        public LayerMask enemyAndPlayerMask;

        // State Tracking
        public bool belongsToPlayer;

        // Methods
        void Start()
        {
            proj = GetComponent<Rigidbody2D>();
            proj.velocity = transform.right * projSpeed;
            display.transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        void Update()
        {
            // Checks if the distance from the player exceeds the distance between the target's position from the player (with some extra room)
            if (Vector2.Distance(transform.position, PlayerController.instance.transform.position) 
                > Vector2.Distance(PlayerController.instance.projectileTargetTile.transform.position, PlayerController.instance.transform.position) + 2)
            {
                proj.velocity = Vector2.zero;
                Destroy(gameObject);
            }

            // Update enemy layer
            sprite.sortingOrder = (int)(transform.position.y * -10 -1);
        }

        // Destroys when it comes into contact with something
        void OnCollisionEnter2D(Collision2D other)
        {
            if (other.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                Collider2D hit = Physics2D.OverlapCircle(transform.position, 0.75f, enemyMask);
                
                EnemyMasterController enemy = hit.GetComponent<EnemyMasterController>();
                enemy.EnemyTakeDamage("main");
            }

            // Switch to fireball collision animation
            proj.velocity = Vector2.zero;
            Destroy(gameObject, 0.1f);
        }
    }
}
