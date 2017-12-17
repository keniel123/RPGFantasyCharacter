using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class OnTriggerLoadScene : MonoBehaviour
    {
        public string loadLevel;
        public string unloadLevel;

        private void OnTriggerEnter(Collider other)
        {
            InputHandler inp = other.GetComponent<InputHandler>();
            if (inp != null)
            {
                if (!string.IsNullOrEmpty(loadLevel))
                {
                    Debug.Log("Loading scene... " + loadLevel);
                    GameSceneManager.Instance.LoadScene(loadLevel);
                }
                else
                {
                    Debug.Log("No scene name to load!");
                }

                if (!string.IsNullOrEmpty(unloadLevel))
                {
                    Debug.Log("Unloading scene... " + unloadLevel);
                    GameSceneManager.Instance.UnloadScene(unloadLevel);
                }
            }
        }
    }
}