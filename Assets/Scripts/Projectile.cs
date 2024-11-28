using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TopDownGame
{
    public class Projectile : MonoBehaviour
    {
        // Outlets
        Rigidbody2D proj;
        public GameObject sprite;

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
            sprite.transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        void OnCollisionEnter2D(Collision2D other)
        {
            if (other.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                Collider2D hit = Physics2D.OverlapCircle(transform.position, 0.75f, enemyMask);
                
                EnemyMasterController enemy = hit.GetComponent<EnemyMasterController>();
                enemy.EnemyTakeDamage();
            }

            Destroy(gameObject);
        }
    }
}
