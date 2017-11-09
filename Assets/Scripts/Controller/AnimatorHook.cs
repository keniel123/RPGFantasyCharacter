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

        public void Init(StateManager st) {
            states = st;
            animator = st.animator;
        }

        void OnAnimatorMove() {

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

            Vector3 delta = animator.deltaPosition;
            delta.y = 0;

            Vector3 v = (delta * rootMotionMultiplier) / states.delta;
            states.rigid.velocity = v;
        }

    }
}
