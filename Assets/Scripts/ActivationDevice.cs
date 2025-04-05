using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TopDownGame
{
    public class ActivationDevice : MonoBehaviour
    {
        [Header("Outlets")]
        public Animator animator;

        public GameObject outputObject;
        public GameObject componentObject;
        //public List<GameObject> outputAppearance = new List<GameObject>();
        private BoxCollider2D outputCollider;
        private Animator outputAnimator;
        private BoxCollider2D triggerZone;

        [Header("State Configuration")]
        public int activationProximitySizeX;
        public int activationProximitySizeY;
        public bool activationState;
        public bool canBeReactivated;
        public bool needsToBeRightClicked;
        private int activations = 0;

        public enum ActivatorType
        {
            Button,
            Lever
        }
        public ActivatorType activator;
        private bool inRange;

        // Start is called before the first frame update
        void Start()
        {
            // Establishes activation zone size / boundaries
            triggerZone = GetComponent<BoxCollider2D>();
            triggerZone.size = new Vector2(activationProximitySizeX, activationProximitySizeY);

            animator.GetComponent<Animator>();
            if (activationState == true)
            {
                print("Activated");
                activationState = true;
                animator.SetBool("Activated?", true);
            }

            // Establishes collider and animator control for object
            outputCollider = outputObject.GetComponent<BoxCollider2D>();
            outputAnimator = outputObject.GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.tag == "Player" && (canBeReactivated || (!canBeReactivated && activations < 1)))
            {
                print("Entered range");
                inRange = true;
                StartCoroutine(ActivationDetection());
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.tag == "Player")
            {
                print("Left range");
                inRange = false;
            }
        }

        IEnumerator ActivationDetection()
        {            
            while (inRange)
            {
                if (needsToBeRightClicked && Input.GetMouseButtonDown(1) && (canBeReactivated || (!canBeReactivated && activations < 1)))
                {
                    if (activationState == false)
                    {
                        print("Activated");
                        activationState = true;
                        animator.SetBool("Activated?", true);
                        activations++;
                        ToggleObject();
                    }
                    else
                    {
                        print("Deactivated");
                        activationState = false;
                        animator.SetBool("Activated?", false);
                        ToggleObject();
                    }
                }

                yield return null;
            }
        }

        private void ToggleObject()
        {
            outputCollider.enabled = !outputCollider.isActiveAndEnabled;
            componentObject.SetActive(!componentObject.activeSelf);
            outputAnimator.SetBool("Activated?", !outputAnimator.GetBool("Activated?"));
        }
    }
}
