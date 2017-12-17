using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class ParryCollider : MonoBehaviour
    {
        StateManager stateManager;
        EnemyStates enemyState;
        public float maxTimer = 0.6f;
        float timer = 0;

        public void InitPlayer(StateManager st)
        {
            stateManager = st;
        }

        private void Update()
        {
            if (stateManager)
            {
                timer += stateManager.delta;
                if (timer > maxTimer)
                {
                    timer = 0;
                    gameObject.SetActive(false);
                }
            }
        }

        public void InitEnemy(EnemyStates eStates)
        {
            enemyState = eStates;
        }

        private void OnTriggerEnter(Collider other)
        {
            //Making parry very hard if you want to
            //    DamageCollider dc = other.GetComponent<DamageCollider>();
            //    if (dc == null)
            //    {
            //        return;
            //    }

            //If it's the player's character, assign enemy target to parry
            if (stateManager)
            {
                EnemyStates enemyStates = other.transform.GetComponentInParent<EnemyStates>();

                if (enemyStates != null)
                {
                    if (transform.root == null)
                    {
                        Debug.Log("Transform root is null: " + transform.root.name);
                    }
                    enemyStates.CheckForParry(transform.root, stateManager);
                }
            }

            //If the character is an enemy, check for player
            if (enemyState)
            {

            }

        }
    }
}