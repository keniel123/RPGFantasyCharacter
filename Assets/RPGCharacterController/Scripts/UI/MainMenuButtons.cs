using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class MainMenuButtons : MonoBehaviour
    {
        public void Press() {
            GameSceneManager.Instance.PressStartGame();
        }
    }
}