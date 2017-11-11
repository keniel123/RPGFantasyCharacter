using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class DamageCollider : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            EnemyStates enemyStates = other.transform.GetComponentInParent<EnemyStates>();
            if (enemyStates == null)
            {
                return;
            }

            enemyStates.DoDamage(50);
        }
    }
}