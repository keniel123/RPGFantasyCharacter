using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance;
        public List<EnemyTarget> enemyTargets = new List<EnemyTarget>();

        public EnemyTarget GetEnemy(Vector3 from) {

            EnemyTarget target = null;

            //Get the closest enemy to player
            float minDistance = float.MaxValue;
            for (int i = 0; i < enemyTargets.Count; i++)
            {
                float distance = Vector3.Distance(from, enemyTargets[i].GetTarget().position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    target = enemyTargets[i];
                }
            }

            return target;
        }

        private void Awake()
        {
            Instance = this;
        }

    }
}