using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class DamageCollider : MonoBehaviour
    {
        StateManager states;
        EnemyStates eStates;

        public void InitPlayer(StateManager st)
        {
            states = st;
            gameObject.layer = LayerMask.NameToLayer("DamageColliders");
            gameObject.SetActive(false);

        }

        public void InitEnemy(EnemyStates enemy)
        {
            eStates = enemy;
            gameObject.layer = LayerMask.NameToLayer("DamageColliders");
            gameObject.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (states)
            {
                EnemyStates enemyStates = other.transform.GetComponentInParent<EnemyStates>();
                if (enemyStates != null)
                {
                    enemyStates.DoDamage(states.currentAction,
                        states.inventoryManager.GetCurrentWeapon(states.currentAction.mirror)); ;
                }

                return;
            }

            if (eStates)
            {
                StateManager player = other.transform.root.GetComponent<StateManager>();
                if (player != null)
                {
                    player.DoDamage(eStates.GetCurrentAttack());
                }
            }
        }
    }
}