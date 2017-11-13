using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class EnemyTarget : MonoBehaviour
    {
        public int index;
        public List<Transform> targets = new List<Transform>();
        public List<HumanBodyBones> humanoidBones = new List<HumanBodyBones>();

        public EnemyStates eStates;

        Animator animator;

        public void Init(EnemyStates enemyStates)
        {
            eStates = enemyStates;
            animator = eStates.animator;
            if (animator.isHuman == false)
            {
                return;
            }
            //If it is human, add al the bones as lock on target transforms
            else
            {
                for (int i = 0; i < humanoidBones.Count; i++)
                {
                    targets.Add(animator.GetBoneTransform(humanoidBones[i]));
                }
            }

            EnemyManager.Instance.enemyTargets.Add(this);
        }

        public Transform GetTarget(bool negative = false)
        {
            if (targets.Count == 0)
            {
                return transform;
            }

            if (!negative)
            {
                if (index < targets.Count - 1)
                {
                    index++;
                }
                else
                {
                    index = 0;
                }

            }
            else
            {
                if (index <= 0)
                {
                    index = targets.Count - 1;
                }
                else
                {
                    index--;
                }
            }

            index = Mathf.Clamp(index, 0, targets.Count);
            return targets[index];
        }
    }
}