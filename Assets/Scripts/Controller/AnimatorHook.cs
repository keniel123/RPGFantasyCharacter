using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class AnimatorHook : MonoBehaviour
    {
        Animator animator;
        StateManager states;
        EnemyStates eStates;
        Rigidbody rigid;

        public float rootMotionMultiplier;
        bool rolling;
        float roll_t;
        float delta;
        AnimationCurve rollCurve;

        public void Init(StateManager stateManager, EnemyStates eSt)
        {
            states = stateManager;
            eStates = eSt;

            if (stateManager != null)
            {
                animator = stateManager.animator;
                rigid = stateManager.rigid;
                rollCurve = states.rollAnimCurve;
                delta = states.delta;
            }
            if (this.eStates != null)
            {
                Debug.Log("Initialized enemy state");
                animator = this.eStates.animator;
                rigid = this.eStates.rigid;
                delta = this.eStates.delta;
            }
        }

        public void InitForRoll() {
            rolling = true;
            roll_t = 0;
        }

        public void CloseRoll() {

            if (!rolling)
            {
                return;
            }

            rootMotionMultiplier = 1;
            roll_t = 0;
            rolling = false;
        }

        void OnAnimatorMove()
        {
            if (states == null && eStates == null)
            {
                return;
            }

            if (rigid == null)
            {
                return;
            }
            
            if (states!=null)
            {
                if (states.canMove == false)
                {
                    return;
                }

                delta = states.delta;
            }

            if (eStates != null)
            {
                if (eStates.canMove)
                {
                    return;
                }

                delta = eStates.delta;
            }

            //If the character is moving, set drag to 0, since the character is moving with root motion and physics together
            rigid.drag = 0;

            if (rootMotionMultiplier == 0)
            {
                rootMotionMultiplier = 1;
            }

            if (!rolling)
            {
                //deltaPos stands for position
                Vector3 deltaPos = animator.deltaPosition;
                deltaPos.y = 0;

                //Whereas delta stands for time
                Vector3 v = (deltaPos * rootMotionMultiplier) / delta;
                rigid.velocity = v;
            }
            //If the character is rolling, manipulate the animation curve
            else
            {
                Debug.Log("Is root motion: " + animator.hasRootMotion);
                //Depending on the animation curve we've, this give the relative pos
                roll_t += delta / 0.6f;

                //Debug.Log("roll_t: " + roll_t);
                if (roll_t > 1)
                {
                    roll_t = 1;
                }

                if (states == null)
                {
                    return;
                }

                float zValueAnim = rollCurve.Evaluate(roll_t);
                Vector3 v1 = Vector3.forward * zValueAnim;
                Vector3 relative = transform.TransformDirection(v1);
                Vector3 v2 = (relative * rootMotionMultiplier);

                rigid.velocity = v2;
            }
        }

        public void OpenDamageColliders() {
            if (states)
            {
                states.inventoryManager.OpenAllDamageColliders();
            }

            OpenParryFlag();
        }

        public void CloseDamageColliders() {
            if (states)
            {
                states.inventoryManager.CloseAllDamageColliders();
            }

            CloseParryFlag();
        }

        public void OpenParryCollider() {
            if (states == null)
            {
                return;
            }

            states.inventoryManager.OpenParryCollider();
        }

        public void CloseParryCollider()
        {
            if (states == null)
            {
                return;
            }

            states.inventoryManager.CloseParryCollider();
        }

        public void OpenParryFlag() {
            if (states)
            {
                states.isParryOn = true;
            }

            if (eStates)
            {
                eStates.isParryOn = true;
            }
        }

        public void CloseParryFlag()
        {
            if (states)
            {
                states.isParryOn = false;
            }

            if (eStates)
            {
                eStates.isParryOn = false;
            }
        }
    }

}
