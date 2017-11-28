using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class InteractionsScriptableObject : ScriptableObject
    {
        public Interaction[] interactions;
    }

    [System.Serializable]
    public class Interaction {
        public string interactionID;
        public string targetAnim;
        public bool oneShot;
        public string specialEvent;
    }
}