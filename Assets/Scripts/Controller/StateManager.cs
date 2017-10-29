using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour {

    [Header("Initilazation")]
    public GameObject activeModel;

    [Header("Inputs")]
    public float horizontal;
    public float vertical;
    public float moveAmount;
    public Vector3 moveDirection;
    public bool rt, rb, lb, lt;     //Controller inputs

    [Header("Stats")]
    public float moveSpeed = 2f;      //Walking & jogging speed
    public float runSpeed = 3.5f;     //Running speed
    public float rotateSpeed = 5f;      //Movement turn speed
    public float toGround = 0.5f;

    [Header("States")]
    public bool isRunning;
    public bool onGround;
    public bool lockOn;
    public bool inAction;

    [HideInInspector]
    public Animator animator;

    [HideInInspector]
    public Rigidbody rigid;

    [HideInInspector]
    public LayerMask ignoreLayers;

    public float delta;

    public void Init()
    {
        SetupAnimator();
        rigid = GetComponent<Rigidbody>();
        rigid.angularDrag = 999;
        rigid.drag = 4;
        rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        gameObject.layer = 8;
        ignoreLayers = ~(1 << 9);

        animator.SetBool("OnGround", true);
    }

    void SetupAnimator() {
        if (activeModel == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.Log("No character animator found");
            }
            else
            {
                activeModel = animator.gameObject;
            }
        }

        if (animator == null)
        {
            animator = activeModel.GetComponent<Animator>();
        }
    }

    public void Tick(float d) {

        delta = d;
        onGround = OnGround();
        animator.SetBool("OnGround", onGround);

    }

    public void FixedTick(float d) {

        delta = d;

        inAction = !animator.GetBool("CanMove");

        if (inAction)
        {
            return;
        }

        DetectAction();
        if (inAction)
        {
            return;
        }
        //While moving there's no need for drag, but if the character is not moving
        //increase the drag, so that it won't slide across the ground surface
        rigid.drag = (moveAmount > 0 || !onGround == false) ? 0 : 4;

        float targetSpeed = moveSpeed;

        //If running, go faster
        if (isRunning)
        {
            targetSpeed = runSpeed;
        }

        if (onGround)
        {
            rigid.velocity = moveDirection * (targetSpeed * moveAmount);
        }

        if (isRunning)
        {
            lockOn = false;
        }

        if (!lockOn)
        {
            Vector3 targetDirection = moveDirection;
            targetDirection.y = 0;
            if (targetDirection == Vector3.zero)
            {
                targetDirection = transform.forward;
            }

            Quaternion targetRotationTemp = Quaternion.LookRotation(targetDirection);
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, targetRotationTemp, delta * moveAmount * rotateSpeed);
            transform.rotation = targetRotation;
        }
        
        HandleMovementAnimations();
    }

    void HandleMovementAnimations() {

        animator.SetBool("Run", isRunning);
        animator.SetFloat("Vertical", moveAmount, 0.4f, delta);
    }

    public bool OnGround() {

        bool r = false;

        Vector3 origin = transform.position + (Vector3.up * toGround);
        Vector3 dir = -Vector3.up;
        float distance = toGround + 0.3f;

        RaycastHit hit;
        if (Physics.Raycast(origin, dir, out hit, distance, ignoreLayers))
        {
            Debug.DrawRay(origin, dir * distance, Color.blue);
            r = true;
            Vector3 targetPos = hit.point;
            transform.position = targetPos;
        }

        return r;
    }

    public void DetectAction() {

        if (rb == false && rt== false && lt== false && lb == false)
        {
            return;
        }

        string targetAnimation = null;
        if (rb)
        {
            targetAnimation = "oh_attack_1";
        }

        if (rt)
        {
            targetAnimation = "oh_attack_2";
        }

        if (lt)
        {
            targetAnimation = "oh_attack_3";
        }

        if (lb)
        {
            targetAnimation = "th_attack_1";
        }

        if (string.IsNullOrEmpty(targetAnimation))
            return;

        inAction = true;

        animator.CrossFade(targetAnimation, 0.4f);

    }
}
