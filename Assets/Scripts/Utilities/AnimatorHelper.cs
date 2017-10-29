using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SA
{
    public class AnimatorHelper : MonoBehaviour
    {
        [Range(-1, 1)]
        public float vertical;

        [Range(-1, 1)]
        public float horizontal;


        public bool playAnim;
        public bool isTwoHanded;

        public string[] OneHandedAttackAnims;
        public string[] TwoHandedAttackAnims;

        public bool enableRootMotion;
        public bool useItem;
        public bool interacting;
        public bool lockon;

        Animator m_animator;
        static string param_Vertical = "Vertical";
        static string param_IsTwoHanded = "IsTwoHanded";


        // Use this for initialization
        void Start()
        {
            m_animator = GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {
            enableRootMotion = !m_animator.GetBool("CanMove");

            m_animator.applyRootMotion = enableRootMotion;

            interacting = m_animator.GetBool("Interacting");

            if (!lockon)
            {
                horizontal = 0;
                vertical = Mathf.Clamp01(vertical);
            }

            m_animator.SetBool("Lock On", lockon);

            //Cant attack until its finished
            if (enableRootMotion)
            {
                return;
            }
            
            if (useItem)
            {
                m_animator.Play("use_item");
                useItem = false;
            }

            if (interacting)
            {
                playAnim = false;

                //If you are using an item, you can't run
                vertical = Mathf.Clamp(vertical, 0, 0.5f);
            }

            m_animator.SetBool(param_IsTwoHanded, isTwoHanded);

            if (playAnim)
            {
                string targetAnim = null;

                if (isTwoHanded)
                {
                    int r = Random.Range(0, TwoHandedAttackAnims.Length);
                    targetAnim = TwoHandedAttackAnims[r];
                }
                else
                {
                    int r = Random.Range(0, OneHandedAttackAnims.Length);
                    targetAnim = OneHandedAttackAnims[r];

                    //If you are running
                    if (vertical > 0.5f)
                    {
                        targetAnim = "oh_attack_3";
                    }
                }

                //If you are running
                if (vertical > 0.5f)
                {
                    targetAnim = "oh_attack_3";
                }

                vertical = 0;

                m_animator.CrossFade(targetAnim, 0.2f);
                //m_animator.SetBool("CanMove", false);
                //enableRootMotion = true;
                playAnim = false;
            }

            m_animator.SetFloat(param_Vertical, vertical);
            m_animator.SetFloat("Horizontal", horizontal);
        }
    }
}