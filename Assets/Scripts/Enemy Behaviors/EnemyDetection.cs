using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TopDownGame
{
    public class EnemyDetection : MonoBehaviour
    {
        
        public bool detectsPlayer;

        public void OnTriggerEnter2D(Collider2D collision)
        {
            // Player is within detection range of enemy
            if (collision.gameObject.tag == "Player")
            {
                detectsPlayer = true;
                print("In range");
            }

            // Player's attack intersects with enemy
            if (collision.gameObject.tag == "Hitbox")
            {
                //EnemyTakeDamage();
            }
        }

        public void OnTriggerExit2D(Collider2D collision)
        {
            // Player leaves enemy's detection range
            if (collision.gameObject.tag == "Player")
            {
                detectsPlayer = false;
                print("Out of range");
            }
        }
    }
}
