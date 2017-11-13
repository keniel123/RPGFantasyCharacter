using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class DamageCollider : MonoBehaviour
    {
        StateManager states;

        public void Init(StateManager st) {
            states = st;
        }

        private void OnTriggerEnter(Collider other)
        {
            EnemyStates enemyStates = other.transform.GetComponentInParent<EnemyStates>();
            if (enemyStates == null)
            {
                return;
            }

            enemyStates.DoDamage(states.currentAction);
        }
    }
}