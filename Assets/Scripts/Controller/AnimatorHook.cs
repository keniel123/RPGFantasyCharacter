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

        public Transform IKTarget;
        public Transform bodyTargetIK;
        public Transform headTarget;

        //Ik References for shield
        public Transform IKTargetShield;
        public Transform IKTargetBodyShield;

        HandleIK IK_Handler;
        public bool useIK;
        public AvatarIKGoal currentHand;

        public bool killDelta;

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
            if (eStates != null)
            {
                //Debug.Log("Initialized enemy");
                animator = eStates.animator;
                rigid = eStates.rigid;
                delta = eStates.delta;
            }

            IK_Handler = gameObject.GetComponent<HandleIK>();
            if (IK_Handler != null)
            {
                IK_Handler.Init(animator);
            }
            else
            {
                Debug.LogWarning("Handle IK component not found on: " + name);
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

            if (IK_Handler != null)
            {
                IK_Handler.OnAnimatorMoveTick(currentHand == AvatarIKGoal.LeftHand);
            }

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
                if (states.onEmpty)
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

                if (killDelta)
                {
                    killDelta = false;
                    deltaPos = Vector3.zero;
                }

                //Whereas delta stands for time
                Vector3 v = (deltaPos * rootMotionMultiplier) / delta;

                //More time in the air, will result higher gravity and faster falling
                if (!states.onGround)
                {
                    v += Physics.gravity;
                }

                rigid.velocity = v;
            }
            //If the character is rolling, manipulate the animation curve
            else
            {
                Debug.Log("Is root motion: " + animator.hasRootMotion);
                //Depending on the animation curve we've, this give the relative pos
                roll_t += delta / 0.2f;

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
                if (!states.onGround)
                {
                    v2 += Physics.gravity;
                }

                rigid.velocity = v2;
            }
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (IK_Handler == null)
            {
                return;
            }

            if (!useIK)
            {
                if (IK_Handler.targetWeight > 0)
                {
                    IK_Handler.IKTick(currentHand, 0);
                }
                else
                {
                    IK_Handler.targetWeight = 0;
                }
            }
            else
            {
                IK_Handler.IKTick(currentHand, 1);
            }
        }

        private void LateUpdate()
        {
            if (IK_Handler != null)
            {
                IK_Handler.LateTick();
            }
        }

        public void OpenCanMove() {
            if (states)
            {
                states.canMove = true;
            }
        }

        public void OpenAttack() {
            if (states)
            {
                states.canAttack = true;
            }
        }

        public void OpenDamageColliders() {
            if (states)
            {
                states.damageIsOn = true;
                states.inventoryManager.OpenAllDamageColliders();
            }

            OpenParryFlag();
        }

        public void CloseDamageColliders() {
            if (states)
            {
                states.damageIsOn = false;
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

        public void CloseParticle() {
            if (states)
            {
                if(states.inventoryManager.currentSpell.currentParticle != null)
                states.inventoryManager.currentSpell.currentParticle.SetActive(false);
            }
        }

        public void InitiateThrowForProjectile()
        {
            if (states)
            {
                states.ThrowProjectile();
            }

        }

        public void InitIKForShield(bool isLeft) {

            //We use if check for isLeft and pass as parameter,
            //because positions might be same when mirrored, but the rotations can be different
            IK_Handler.UpdateIKTargets((isLeft) ? IKSnaphotType.Shield_LeftHand : IKSnaphotType.Shield_RightHand, isLeft);
        }

        public void InitIKForBreathSpell(bool isLeft) {
            IK_Handler.UpdateIKTargets(IKSnaphotType.Breath, isLeft);
        }

        public void OpenRotationControl() {
            if (states)
            {
                states.canRotate = true;
            }
        }

        public void CloseRotationControl()
        {
            if (states)
            {
                states.canRotate = false;
            }
        }

        public void ConsumeCurrentItem() {
            if (states)
            {
                if (states.inventoryManager.currentConsumable)
                {
                    states.inventoryManager.currentConsumable.itemCount--;
                    ItemEffectsManager.Instance.CastEffect(states.inventoryManager.currentConsumable.Instance.consumableEffect, states);
                }
            }
        }
    }

}
