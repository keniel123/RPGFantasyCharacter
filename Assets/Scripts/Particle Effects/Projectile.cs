using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class Projectile : MonoBehaviour
    {
        Rigidbody rigid;

        public float horizontalSpeed = 10;
        public float verticalSpeed = 2;

        //Projectile Target
        public Transform target;

        public GameObject explosionPrefab;

        public void Init()
        {
            rigid = GetComponent<Rigidbody>();

            Vector3 targetForce = transform.forward * horizontalSpeed;
            targetForce += transform.up * verticalSpeed;
            rigid.AddForce(targetForce, ForceMode.Impulse);
        }

        private void OnTriggerEnter(Collider other)
        {
            EnemyStates enemyStates = other.GetComponent<EnemyStates>();
            if (enemyStates != null)
            {
                enemyStates.health -= 40;
                enemyStates.DoDamage_();
                SpellEffectManager.Instance.UseSpellEffect("OnFire", null, enemyStates);
            }

            GameObject go = Instantiate(explosionPrefab, transform.position, transform.rotation) as GameObject;

            Destroy(this.gameObject);
        }

    }
}