using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class LevelManager : MonoBehaviour
    {
        public Transform spawnPosition;
        public WorldInteraction[] worldInteractions;

        public static LevelManager Instance;

        private void Awake()
        {
            Instance = this;
        }
    }
}