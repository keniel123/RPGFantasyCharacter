﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class EnemyStates : MonoBehaviour
    {
        public int health;
        public CharacterStats characterStats;

        public bool canParried = true;
        public bool isParryOn = true;
        //public bool doParry = false;
        public bool isInvincible;
        public bool dontDoAnything;
        public bool canMove;
        public bool isDead;
        public StateManager parriedBy;

        public Animator animator;
        public Rigidbody rigid;

        EnemyTarget enemyTarget;
        AnimatorHook animHook;

        public float airTimer;
        public float delta;
        public float poiseDegrade = 2;

        List<Rigidbody> ragdollRigids = new List<Rigidbody>();
        List<Collider> ragdollColliders = new List<Collider>();

        public delegate void SpellEffectLoop();
        public SpellEffectLoop spellEffectLoop;

        float timer;

        void Start()
        {
            health = 10000;

            animator = GetComponentInChildren<Animator>();
            enemyTarget = GetComponent<EnemyTarget>();
            enemyTarget.Init(this);

            rigid = GetComponent<Rigidbody>();

            animHook = animator.GetComponent<AnimatorHook>();

            if (animHook == null)
            {
                animHook = animator.gameObject.AddComponent<AnimatorHook>();
            }

            animHook.Init(null, this);
            InitRagdoll();
            isParryOn = false;

        }

        void InitRagdoll() {

            Rigidbody[] rigs = GetComponentsInChildren<Rigidbody>();
            for (int i = 0; i < rigs.Length; i++)
            {
                if (rigs[i] == rigid)
                {
                    continue;
                }

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

        void Update()
        {
            delta = Time.deltaTime;

            canMove = animator.GetBool(StaticStrings.animParam_CanMove);

            if (spellEffectLoop != null)
            {
                spellEffectLoop();
            }

            if (dontDoAnything)
            {
                dontDoAnything = !canMove;
                return;
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

                //DEBUG
                timer += Time.deltaTime;
                if (timer > 3)
                {
                    DoAction();
                    timer = 0;
                }
            }

            characterStats.poise -= delta * poiseDegrade;
            if (characterStats.poise < 0)
            {
                characterStats.poise = 0;
            }

        }

        void DoAction() {
            
            animator.Play("oh_attack_1");
            animator.applyRootMotion = true;
            animator.SetBool(StaticStrings.animParam_CanMove, false);

        }

        public void DoDamage(Action act, Weapon currentWeapon)
        {
            if (isInvincible)
            {
                return;
            }

            //int damageTaken = StatsCalculations.CalculateBaseDamage(currentWeapon.weaponStats, characterStats);
            int damageTaken = 5;

            characterStats.poise += damageTaken;
            health -= damageTaken;
            
            Debug.Log("Damage is: " + damageTaken + ", Poise is: " + characterStats.poise);

            if (canMove || characterStats.poise > 100)
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
            animator.SetBool(StaticStrings.animParam_CanMove,false);
        }

        public void DoDamage_() {

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
    }
}