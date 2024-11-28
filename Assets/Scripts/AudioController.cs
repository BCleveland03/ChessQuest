using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TopDownGame {
    public class AudioController : MonoBehaviour
    {
        public static AudioController instance;

        // Outlets


        // Methods
        private void Awake()
        {
            instance = this;
        }
    }
}
