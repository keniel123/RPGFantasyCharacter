using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class AnimatorHook : MonoBehaviour
    {
        Animator animator;
        StateManager states;

        public float rootMotionMultiplier;
        bool rolling;
        float roll_t;

        public void Init(StateManager stateManager)
        {
            states = stateManager;
            animator = stateManager.animator;
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

            if (states.canMove)
            {
                return;
            }

            //If the character is moving, set drag to 0, since the character is moving with root motion and physics together
            states.rigid.drag = 0;

            if (rootMotionMultiplier == 0)
            {
                rootMotionMultiplier = 1;
            }

            if (!rolling)
            {
                Vector3 delta = animator.deltaPosition;
                delta.y = 0;

                Vector3 v = (delta * rootMotionMultiplier) / states.delta;
                states.rigid.velocity = v;
            }
            //If the character is rolling, manipulate the animation curve
            else
            {
                Debug.Log("Is root motion: " + animator.hasRootMotion);
                //Depending on the animation curve we've, this give the relative pos
                roll_t += states.delta / 0.65f;

                //Debug.Log("roll_t: " + roll_t);
                if (roll_t > 1)
                {
                    roll_t = 1;
                }

                float zValueAnim = states.rollAnimCurve.Evaluate(roll_t);
                Vector3 v1 = Vector3.forward * zValueAnim;
                Vector3 relative = transform.TransformDirection(v1);
                Vector3 v2 = (relative * rootMotionMultiplier);

                states.rigid.velocity = v2;
            }
        }

    }
}
