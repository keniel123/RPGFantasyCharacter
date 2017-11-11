using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class EnemyStates : MonoBehaviour
    {
        public float health;
        public bool isInvincible;
        public bool canMove;
        public bool isDead;

        public Animator animator;
        public Rigidbody rigid;

        EnemyTarget enemyTarget;
        AnimatorHook animHook;

        public float delta;
        List<Rigidbody> ragdollRigids = new List<Rigidbody>();
        List<Collider> ragdollColliders = new List<Collider>();

        void Start()
        {
            health = 100;

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

            canMove = animator.GetBool("CanMove");

            if (health <= 0)
            {
                isDead = true;
                EnableRagdoll();
            }

            if (isInvincible)
            {
                isInvincible = !canMove;
            }

            if (canMove)
            {
                animator.applyRootMotion = false;
            }
        }

        public void DoDamage(float damageTaken)
        {
            if (isInvincible)
            {
                return;
            }

            Debug.Log("Taken damage: " + damageTaken);
            health -= damageTaken;
            isInvincible = true;
            animator.Play("damage_1");
            animator.applyRootMotion = true;
            animator.SetBool("CanMove",false);
        }
    }
}