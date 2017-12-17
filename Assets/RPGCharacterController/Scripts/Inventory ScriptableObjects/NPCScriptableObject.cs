using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{

    public class NPCScriptableObject : ScriptableObject
    {
        public NPCDialogue[] NPCList;
    }

    [System.Serializable]
    public class NPCDialogue {
        public string NPC_Id;
        public Dialogue[] dialogue;
    }

    [System.Serializable]
    public class Dialogue {
        public string[] dialogueText;
        public string specialEvent;
        public bool increaseIndex;
        public string targetAnim;
        public bool playAnim;
    }
}