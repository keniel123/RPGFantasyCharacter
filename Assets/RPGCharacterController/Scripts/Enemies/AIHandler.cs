using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class AIHandler : MonoBehaviour
    {
        public AIAttacks[] ai_attacks;
        public EnemyStates enemyState;

        public StateManager states;
        public Transform target;
        
        public float sight; //Closest you can be at target
        public float FOVAngle;

        public int closeCount = 10;
        int _close;

        public int frameCount = 30;
        int _frame;


        public int attackCount = 30;    //Check for every 30 frames if the enemy can attack
        int _attackCount;

        float distanceFromTarget;
        float angleToTarget;
        Vector3 directionToTarget;

        float delta;

        private void Start()
        {
            if (enemyState == null)
            {
                enemyState = GetComponent<EnemyStates>();
            }

            enemyState.Init();
            InitDamageColldiers();
        }

        void InitDamageColldiers() {
            for (int i = 0; i < ai_attacks.Length; i++)
            {
                for (int k = 0; k < ai_attacks[i].damageColliders.Length; k++)
                {
                    DamageCollider damageCol = ai_attacks[i].damageColliders[k].GetComponent<DamageCollider>();
                    damageCol.InitEnemy(enemyState);
                }
            }

            for (int i = 0; i < enemyState.defaultDamageColliders.Length; i++)
            {
                DamageCollider damageCol = enemyState.defaultDamageColliders[i].GetComponent<DamageCollider>();
                damageCol.InitEnemy(enemyState);
            }
        }

        public AIState aiState;
        public enum AIState {
            FAR, CLOSE, INSIGHT, ATTACKING
        }

        private void Update()
        {
            delta = Time.deltaTime;

            distanceFromTarget = GetDistanceFromTarget();
            angleToTarget = GetAngleToTarget();
            if (target)
                directionToTarget = target.position - transform.position;
            enemyState.directionToTarget = this.directionToTarget;

            switch (aiState)
            {
                case AIState.FAR:
                    HandleFarSight();
                    break;
                case AIState.CLOSE:
                    HandleCloseSight();
                    break;
                case AIState.INSIGHT:
                    InSight();
                    break;
                case AIState.ATTACKING:
                    if (enemyState.canMove)
                    {
                        aiState = AIState.INSIGHT;
                        enemyState.rotateToTarget = true;
                        //enemyState.navMeshAgent.enabled = true;
                    }
                    break;
                default:
                    break;
            }

            enemyState.Tick(delta);
        }

        void HandleCloseSight() {

            _close++;
            if (_close > closeCount)
            {
                _close = 0;

                if (distanceFromTarget > sight || angleToTarget > FOVAngle)
                {
                    aiState = AIState.FAR;
                    return;
                }
            }

            RaycastToTarget();
        }

        void GoToTarget() {
            enemyState.hasDestination = false;
            enemyState.SetDestination(target.position);
        }

        void InSight() {

            HandleCoolDowns();
            
            float distanceToDestination = Vector3.Distance(enemyState.targetDestination, target.position);
            if (distanceToDestination > 2 || distanceFromTarget > sight * 0.5)
                GoToTarget();

            if (distanceFromTarget < 2)
                enemyState.navMeshAgent.isStopped = true;

            if (_attackCount > 0)
            {
                _attackCount--;
                return;
            }

            _attackCount = attackCount;

            AIAttacks attack = WillAttack();
            enemyState.SetCurrentAttack(attack);

            if (attack != null)
            {
                enemyState.animator.Play(attack.targetAnim);
                enemyState.animator.SetBool(StaticStrings.animParam_OnEmpty, false);
                enemyState.canMove = false;
                attack._cool = attack.coolDown;
                enemyState.navMeshAgent.isStopped = true;
                enemyState.rotateToTarget = false;
                return;
            }

        }
        
        void HandleFarSight()
        {
            if (target == null)
            {
                return;
            }

            _frame++;
            if (_frame > frameCount)
            {
                _frame = 0;
                //Do checks for target
                if (distanceFromTarget < sight)
                {
                    if (angleToTarget < FOVAngle)
                    {
                        aiState = AIState.CLOSE;
                    }
                }

            }
        }

        void HandleCoolDowns() {

            for (int i = 0; i < ai_attacks.Length; i++)
            {
                AIAttacks a = ai_attacks[i];
                if (a._cool > 0)
                {
                    a._cool -= delta;
                    if (a._cool < 0)
                        a._cool = 0;
                }
            }

        }

        void LookTowardsTarget() {
            Vector3 direction = directionToTarget;
            direction.y = 0;
            if (direction == Vector3.zero)
            {
                direction = transform.forward;
            }

            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, delta * 5);
        }

        public AIAttacks WillAttack()
        {
            int w = 0;
            List<AIAttacks> list = new List<AIAttacks>();
            for (int i = 0; i < ai_attacks.Length; i++)
            {
                AIAttacks a = ai_attacks[i];
                if (a._cool > 0)
                {
                    continue;
                }

                if (distanceFromTarget > a.minDistance)
                {
                    continue;
                }

                if (angleToTarget < a.minAngle)
                {
                    continue;
                }

                if (angleToTarget > a.maxAngle)
                {
                    continue;
                }

                if (a.weight == 0)
                {
                    continue;
                }

                w += a.weight;
                list.Add(a);
            }

            if (list.Count == 0)
            {
                return null;
            }

            int rand = Random.Range(0, w + 1);
            int c_w = 0;

            for (int i = 0; i < list.Count; i++)
            {
                c_w += list[i].weight;
                if (c_w > rand)
                {
                    return list[i];
                    //Do ATTACK!
                }
            }

            return null;
        }

        void RaycastToTarget() {
            RaycastHit hit;
            Vector3 origin = transform.position;
            origin.y += 0.5f;
            Vector3 direction = directionToTarget;
            direction.y += 0.5f;    //Avoid shooting raycast too low e.g. target's feet
            if (Physics.Raycast(origin, direction, out hit, sight, enemyState.ignoreLayers))
            {
                StateManager st = hit.transform.GetComponentInParent<StateManager>();
                Debug.Log("StateManager: " + (st != null));
                if (st != null)
                {
                    enemyState.rotateToTarget = true;
                    aiState = AIState.INSIGHT;
                    enemyState.SetDestination(target.position);
                }
            }
        }

        private float GetDistanceFromTarget()
        {
            if (target == null)
            {
                return 100;
            }

            return Vector3.Distance(target.position, transform.position);

        }

        private float GetAngleToTarget()
        {
            float a = 180;  //180, in case of the target is behind
            if (target)
            {
                Vector3 direction = directionToTarget;
                a = Vector3.Angle(direction, transform.forward);
            }

            return a;

        }

    }


    [System.Serializable]
    public class AIAttacks
    {
        public int weight;
        public float minDistance;
        public float minAngle;
        public float maxAngle;

        public float coolDown = 2;
        public float _cool;
        public string targetAnim;
        public bool hasReactAnim;
        public string reactAnimation;

        public bool isDefaultDamageCollider;
        public GameObject[] damageColliders;
    }
}