using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class BreathCollider : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            EnemyStates enemy = other.GetComponent<EnemyStates>();

            if (enemy != null)
            {
                enemy.DoDamage_();
                SpellEffectManager.Instance.UseSpellEffect("OnFire", null, enemy);
            }
        }
    }
}