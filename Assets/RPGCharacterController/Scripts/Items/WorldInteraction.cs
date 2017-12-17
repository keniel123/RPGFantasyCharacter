using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class WorldInteraction : MonoBehaviour
    {
        public string interactionId;
        public UIManager.UIActionType actionType;

        public virtual void InteractActual() {

        }
    }
}