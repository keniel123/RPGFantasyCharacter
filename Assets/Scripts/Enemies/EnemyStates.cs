using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace RPGController
{
    public class EnemyStates : MonoBehaviour
    {
        [Header ("Stats")]
        public int health;
        public CharacterStats characterStats;
        public float poiseDegrade = 2;
        public float airTimer;

        [Header("Values")]
        public float delta;
        public float horizontal;
        public float vertical;

        AIAttacks currentAttack;
        public void SetCurrentAttack(AIAttacks a) {
            currentAttack = a;
        }

        public AIAttacks GetCurrentAttack() {
            return currentAttack;
        }

        public GameObject[] defaultDamageColliders;

        [Header("States")]
        public bool canParried = true;
        public bool isParryOn = true;
        //public bool doParry = false;
        public bool isInvincible;
        public bool dontDoAnything;
        public bool canMove;
        public bool isDead;
        public bool hasDestination;
        public Vector3 targetDestination;
        public Vector3 directionToTarget;
        public bool rotateToTarget;

        public StateManager parriedBy;

        //References
        public Animator animator;
        public Rigidbody rigid;
        EnemyTarget enemyTarget;
        AnimatorHook animHook;
        public NavMeshAgent navMeshAgent;

        public LayerMask ignoreLayers;

        List<Rigidbody> ragdollRigids = new List<Rigidbody>();
        List<Collider> ragdollColliders = new List<Collider>();

        public delegate void SpellEffectLoop();
        public SpellEffectLoop spellEffectLoop;

        float timer;

        public void Init()
        {
            health = 10;

            animator = GetComponentInChildren<Animator>();
            enemyTarget = GetComponent<EnemyTarget>();
            enemyTarget.Init(this);

            rigid = GetComponent<Rigidbody>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            rigid.isKinematic = true;

            animHook = animator.GetComponent<AnimatorHook>();

            if (animHook == null)
            {
                animHook = animator.gameObject.AddComponent<AnimatorHook>();
            }

            animHook.Init(null, this);
            InitRagdoll();
            isParryOn = false;

            ignoreLayers = ~(1 << 9);
        }

        void InitRagdoll() {

            Rigidbody[] rigs = GetComponentsInChildren<Rigidbody>();
            for (int i = 0; i < rigs.Length; i++)
            {
                if (rigs[i] == rigid)
                {
                    continue;
                }

                rigs[i].gameObject.layer = LayerMask.NameToLayer("Ragdolls");
                ragdollRigids.Add(rigs[i]);
                rigs[i].isKinematic = true;

                Collider col = rigs[i].gameObject.GetComponent<Collider>();
                col.isTrigger = true;
                ragdollColliders.Add(col);
            }

        }

        public void EnableRagdoll() {
            
            for (int i = 0; i < ragdollRigids.Count; i++)
            {
                ragdollRigids[i].isKinematic = false;
                ragdollColliders[i].isTrigger = false;
            }

            Collider controllerCollider = rigid.gameObject.GetComponent<Collider>();
            controllerCollider.enabled = false;
            rigid.isKinematic = true;

            StartCoroutine("CloseAnimator");
        }

        IEnumerator CloseAnimator()
        {
            yield return new WaitForEndOfFrame();
            animator.enabled = false;
            this.enabled = false;
        }

        public void Tick(float d)
        {
            delta = d;
            
            canMove = animator.GetBool(StaticStrings.animParam_OnEmpty);

            if (spellEffectLoop != null)
            {
                spellEffectLoop();
            }

            if (dontDoAnything)
            {
                dontDoAnything = !canMove;
                return;
            }

            if (rotateToTarget)
            {
                LookTowardsTarget();
                Debug.Log("Rotate");
            }

            if (health <= 0)
            {
                isDead = true;
                EnableRagdoll();
            }

            if (isInvincible)
            {
                isInvincible = !canMove;
            }

            if (parriedBy != null && !isParryOn)
            {
                //parriedBy.parryTarget = null;
                parriedBy = null;
            }

            if (canMove)
            {
                isParryOn = false;
                animator.applyRootMotion = false;

                MovementAnimation();
            }
            else
            {
                if (!animator.applyRootMotion)
                {
                    animator.applyRootMotion = true;
                }
            }

            characterStats.poise -= delta * poiseDegrade;
            if (characterStats.poise < 0)
            {
                characterStats.poise = 0;
            }

        }

        public void MovementAnimation()
        {

            float square = navMeshAgent.desiredVelocity.sqrMagnitude;
            float vertical = Mathf.Clamp(square, 0, 0.5f);
            animator.SetFloat(StaticStrings.animParam_Vertical, vertical, 0.2f, delta);


            //Vector3 desiredVelocity = navMeshAgent.desiredVelocity;
            //Vector3 relative = transform.InverseTransformDirection(desiredVelocity);

            //float vertical = relative.z;
            //float hor = relative.x;

            //vertical = Mathf.Clamp(vertical, -0.5f, 0.5f);
            //hor = Mathf.Clamp(hor, -0.5f, 0.5f);

            //animator.SetFloat(StaticStrings.animParam_Horizontal, hor, 0.2f, delta);
            //animator.SetFloat(StaticStrings.animParam_Vertical, vertical, 0.2f, delta);

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

        public void SetDestination(Vector3 dest) {
            if (!hasDestination)
            {
                hasDestination = true;
                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(dest);
                targetDestination = dest;
            }

            //Check stop destination
        }

        void DoAction() {
            
            animator.Play("oh_attack_1");
            animator.applyRootMotion = true;
            animator.SetBool(StaticStrings.animParam_CanMove, false);

        }

        public void DoDamage(Action act, Weapon currentWeapon)
        {
            //return;
            if (isInvincible)
            {
                return;
            }

            //int damageTaken = StatsCalculations.CalculateBaseDamage(currentWeapon.weaponStats, characterStats);
            int damageTaken = 5;

            characterStats.poise += damageTaken;
            health -= damageTaken;
            
            Debug.Log("Damage is: " + damageTaken + ", Poise is: " + characterStats.poise);

            if (canMove /*|| characterStats.poise > 100*/)
            {
                if (act.overrideDamageAnimation)
                {
                    animator.Play(act.damageAnim);
                }
                else
                {
                    int rand = Random.Range(0, 100);
                    string targetAnim = (rand > 50) ? StaticStrings.damage1 : StaticStrings.damage2;
                    animator.Play(targetAnim);
                }
            }

            isInvincible = true;
            animator.applyRootMotion = true;
            //animator.SetBool(StaticStrings.animParam_CanMove,false);
        }

        public void DoDamage_() {
            //return;
            if (isInvincible)
            {
                return;
            }

            animator.Play("damage_3");

        }

        public void CheckForParry(Transform parryTarget, StateManager states) {
            if (!canParried || !isParryOn || isInvincible)
            {
                return;
            }

            Vector3 direction = transform.position - parryTarget.position;
            direction.Normalize();
            float dotProduct = Vector3.Dot(parryTarget.forward, direction);

            //If the enemy is behind you, but still lands in collider, don't parry
            if (dotProduct <0)
            {
                return;
            }

            isInvincible = true;
            animator.Play(StaticStrings.animState_AttackInterrupt);
            animator.applyRootMotion = true;
            animator.SetBool(StaticStrings.animParam_CanMove, false);
            //states.parryTarget = this;
            parriedBy = states;
            return;
        }

        public void IsGettingParried(Action act, Weapon currentWeapon)
        {
            //int damage = StatsCalculations.CalculateBaseDamage(currentWeapon.weaponStats, characterStats, act.parryMultiplier);
            //Debug.Log("Parry damage: " + damage);
            int damage = 5;

            health -= Mathf.RoundToInt(damage);
            dontDoAnything = true;
            animator.SetBool(StaticStrings.animParam_CanMove, false);
            animator.Play(StaticStrings.animState_ParryReceived);

        }

        public void IsGettingBackStabbed(Action act, Weapon currentWeapon)
        {
            //int damage = StatsCalculations.CalculateBaseDamage(currentWeapon.weaponStats, characterStats,act.backstabMultiplier);
            int damage = 5;

            //Debug.Log("Backstab damage: " + damage);

            health -= Mathf.RoundToInt(damage);
            dontDoAnything = true;
            animator.SetBool(StaticStrings.animParam_CanMove, false);
            animator.Play(StaticStrings.animState_BackStabbed);

        }

        public ParticleSystem fire_particle;
        float _timer;

        public void OnFire() {

            if (fire_particle == null)
            {
                return;
            }

            if (_timer < 3)
            {
                _timer += Time.deltaTime;
                fire_particle.Emit(1);
            }
            else
            {
                _timer = 0;
                spellEffectLoop = null;
            }
        }

        #region Handle Damage Colliders
        public void OpenDamageColliders()
        {
            if (currentAttack == null)
            {
                return;
            }

            Debug.Log("Opening damage colliders");
            if (currentAttack.isDefaultDamageCollider || currentAttack.damageColliders.Length == 0)
            {
                ObjectListStatus(defaultDamageColliders, true);
            }
            else
            {
                ObjectListStatus(currentAttack.damageColliders, true);
            }
        }

        public void CloseDamageColliders()
        {
            if (currentAttack == null)
            {
                return;
            }

            if (currentAttack.isDefaultDamageCollider || currentAttack.damageColliders.Length == 0)
            {
                ObjectListStatus(defaultDamageColliders, false);
            }
            else
            {
                ObjectListStatus(currentAttack.damageColliders, false);
            }
        }
        
        void ObjectListStatus(GameObject[] list, bool status)
        {
            for (int i = 0; i < list.Length; i++)
            {
                list[i].SetActive(status);
                Debug.Log(list[i].name + " is: " + list[i].activeInHierarchy);
            }
        }

        #endregion
    }
}