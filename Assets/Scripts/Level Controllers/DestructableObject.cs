using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TopDownGame
{
    public class DestructableObject : MonoBehaviour
    {
        [Header("Outlets")]
        private Animator animator;
        public Collider2D endPointCollision;

        [Header("Configuration")]
        public int health;
        public bool triggerEndOnDestroy;

        void Start()
        {
            animator = GetComponent<Animator>();
            endPointCollision.enabled = false;
        }

        public void DamageObject()
        {
            health--;
            animator.SetInteger("HitsLeft", health);
            if (health < 1 && triggerEndOnDestroy)
            {
                StartCoroutine(EnableEndingPoint());
            }
        }

        IEnumerator EnableEndingPoint()
        {
            yield return new WaitForSeconds(1.5f);

            endPointCollision.enabled = true;
        }
    }
}
