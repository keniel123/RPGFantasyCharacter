using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class HandleIK : MonoBehaviour
    {
        Animator animator;

        Transform handHelper;
        Transform bodyHelper;
        Transform headHelper;
        Transform shoulderHelper;
        Transform animShoulder;
        Transform headTrans;
        
        public float targetWeight;

        public IKSnapShot[] ikSnapshots;
        public Vector3 defaultHeadPos;

        IKSnapShot GetSnapShot(IKSnaphotType type) {

            for (int i = 0; i < ikSnapshots.Length; i++)
            {
                if (ikSnapshots[i].type ==type)
                {
                    return ikSnapshots[i];
                }
            }

            return null;
        }

        public void Init(Animator a)
        {
            animator = a;

            headHelper = new GameObject().transform;
            headHelper.name = "IK Head Helper";

            handHelper = new GameObject().transform;
            handHelper.name = "IK Hand Helper";

            bodyHelper = new GameObject().transform;
            bodyHelper.name = "IK Body Helper";

            shoulderHelper = new GameObject().transform;
            shoulderHelper.name = "IK Shoulder Helper";

            shoulderHelper.parent = transform.parent;
            shoulderHelper.localPosition = Vector3.zero;
            shoulderHelper.localRotation = Quaternion.identity;
            headHelper.parent = shoulderHelper;
            bodyHelper.parent = shoulderHelper;
            handHelper.parent = shoulderHelper;

            headTrans = animator.GetBoneTransform(HumanBodyBones.Head);
        }

        public void UpdateIKTargets(IKSnaphotType type, bool isLeft) {
            Debug.Log("UpdateIKTarget for: " + type);
            IKSnapShot snapShot = GetSnapShot(type);

            Vector3 targetBodyPos = snapShot.bodyPos;
            if (isLeft)
            {
                targetBodyPos.x = -targetBodyPos.x;
            }

            bodyHelper.localPosition = targetBodyPos;

            handHelper.localPosition = snapShot.handPos;
            handHelper.localEulerAngles = snapShot.handEulers;

            if (snapShot.overrideHeadPos)
                headHelper.localPosition = snapShot.headPos;
            else
                headHelper.localPosition = defaultHeadPos;
            
        }

        public void OnAnimatorMoveTick(bool isLeft) {

            Transform shoulder = animator.GetBoneTransform((isLeft) ? HumanBodyBones.LeftShoulder : HumanBodyBones.RightShoulder);
            shoulderHelper.transform.position = shoulder.position;
        }

        public void IKTick(AvatarIKGoal IKGoal, float weight) {

            targetWeight = Mathf.Lerp(targetWeight, weight, Time.deltaTime * 5);

            animator.SetIKPositionWeight(IKGoal, targetWeight);
            animator.SetIKRotationWeight(IKGoal, targetWeight);

            animator.SetIKPosition(IKGoal, handHelper.position);
            animator.SetIKRotation(IKGoal, handHelper.rotation);

            animator.SetLookAtWeight(targetWeight, 0.8f, 1, 1, 1);
            animator.SetLookAtPosition(bodyHelper.position);
        }

        public void LateTick() {

            if (headTrans == null || headHelper == null)
            {
                return;
            }

            Vector3 direction = headHelper.position - headTrans.position;
            if (direction == Vector3.zero)
            {
                direction = headTrans.forward;
            }

            Quaternion targetRot = Quaternion.LookRotation(direction);
            Quaternion currentRotation = Quaternion.Slerp(headTrans.rotation, targetRot, targetWeight);
            headTrans.rotation = currentRotation;
            
        }
        
    }

    public enum IKSnaphotType {
        Breath, Shield_RightHand, Shield_LeftHand
    }

    [System.Serializable]
    public class IKSnapShot {
        public IKSnaphotType type;

        public Vector3 handPos;
        public Vector3 handEulers;
        public Vector3 bodyPos;

        public bool overrideHeadPos;
        public Vector3 headPos;
    }
}