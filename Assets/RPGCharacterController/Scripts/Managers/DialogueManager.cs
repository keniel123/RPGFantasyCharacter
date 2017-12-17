using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RPGController
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance;

        public Text dialogueText;
        public GameObject textObj;
        Transform origin;
        NPCDialogue npcDialogue;
        NPCStates npcState;
        public bool dialogueActive;
        bool updateDialogue;
        int lineIndex;
        public Transform player;
        
        void Awake()
        {
            Instance = this;
            textObj.SetActive(false);
        }

        public void Init(Transform playerTransform) {
            player = playerTransform;
        }

        //Origin; transform of the NPC
        public void InitDialogue(Transform npcOrigin, string NPCId) {
            origin = npcOrigin;
            npcDialogue = ResourcesManager.Instance.GetNPCDialogue(NPCId);
            npcState = SessionManager.Instance.GetNPCState(NPCId);
            dialogueActive = true;
            textObj.SetActive(true);
            updateDialogue = false;
            lineIndex = 0;
        }

        public void Tick(bool input)
        {
            if (!dialogueActive)
                return;

            if (origin == null)
            {
                return;
            }

            float distance = Vector3.Distance(player.transform.position, origin.transform.position);
            if (distance > 6)
            {
                CloseDialogue();
            }

            if (!updateDialogue) {

                updateDialogue = true;
                dialogueText.text = npcDialogue.dialogue[npcState.dialogueIndex].dialogueText[lineIndex];

                if (npcDialogue.dialogue[npcState.dialogueIndex].playAnim)
                {
                    Animator animator = origin.GetComponentInChildren<Animator>();
                    animator.CrossFade(npcDialogue.dialogue[npcState.dialogueIndex].targetAnim,0.2f);
                }
            }

            if (input)
            {
                input = false;
                updateDialogue = false;
                lineIndex++;

                //If you have reached the end of conversation
                if (lineIndex > npcDialogue.dialogue[npcState.dialogueIndex].dialogueText.Length - 1)
                {
                    if (npcDialogue.dialogue[npcState.dialogueIndex].increaseIndex)
                    {
                        npcState.dialogueIndex++;

                        if (npcState.dialogueIndex > npcDialogue.dialogue.Length -1)
                        {
                            npcState.dialogueIndex = npcDialogue.dialogue.Length - 1;
                        }
                    }

                    CloseDialogue();
                }
            }
        }

        void CloseDialogue() {
            dialogueActive = false;
            textObj.SetActive(false);
        }
    }
}