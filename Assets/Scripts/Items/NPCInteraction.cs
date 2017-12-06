using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class NPCInteraction : WorldInteraction
    {
        public string NPC_Id;

        public override void InteractActual()
        {
            DialogueManager.Instance.InitDialogue(this.transform, NPC_Id);
        }
    }
}