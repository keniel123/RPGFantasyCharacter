using RPGController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{

    [Header("Initilazation")]
    public GameObject activeModel;

    [Header("Inputs")]
    public float horizontal;
    public float vertical;
    public float moveAmount;
    public Vector3 moveDirection;
    public bool rt, rb, lt, lb; //Input buttons and axises
    public bool rollInput;

    [Header("Stats")]
    public float moveSpeed = 2f;        //Walking & jogging speed
    public float runSpeed = 3.5f;       //Running speed
    public float rotateSpeed = 5f;      //Movement turn speed
    public float toGround = 0.5f;
    public float rollSpeed = 1.0f;

    [Header("States")]
    public bool isRunning;
    public bool onGround;
    public bool lockOn;
    public bool inAction;
    public bool canMove;
    public bool isTwoHanded;

    [Header("Other")]
    public EnemyTarget lockOnTarget;

    [HideInInspector]
    public Animator animator;

    [HideInInspector]
    public Rigidbody rigid;

    [HideInInspector]
    public AnimatorHook animHook;

    [HideInInspector]
    public LayerMask ignoreLayers;

    float _actionDelay;

    public float delta;

    public void Init()
    {
        SetupAnimator();
        rigid = GetComponent<Rigidbody>();
        rigid.angularDrag = 999;
        rigid.drag = 4;
        rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        animHook = activeModel.AddComponent<AnimatorHook>();
        animHook.Init(this);

        gameObject.layer = 8;
        ignoreLayers = ~(1 << 9);

        animator.SetBool("OnGround", true);
    }

    void SetupAnimator()
    {
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

    public void Tick(float d)
    {

        delta = d;
        onGround = OnGround();
        animator.SetBool("OnGround", onGround);

    }

    void HandleRolls()
    {
        if (!rollInput)
        {
            return;
        }

        float v = vertical;
        float h = horizontal;
        v = (moveAmount > 0.3f) ? 1 : 0;
        h = 0;

        /////////////////////////////////////////////////////////
        //WHEN YOU HAVE BETTER ROLLING ANIMATIONS USE THIS PART//
        /////////////////////////////////////////////////////////
        /*if (!lockOn)
        {
            v = (moveAmount > 0.3f) ? 1:0;
            h = 0;
        }
        else
        {
            //Eliminate trivial input values
            if (Mathf.Abs(v) > 0.3f)
            {
                v = 0;
            }

            if (Mathf.Abs(h) < 0.3f)
            {
                h = 0;
            }
        }*/

        //So that you can still jump backwards
        if (v != 0)
        {

            if (moveDirection == Vector3.zero)
            {
                moveDirection = transform.forward;
            }

            Quaternion targetRot = Quaternion.LookRotation(moveDirection);
            transform.rotation = targetRot;
        }

        animHook.rootMotionMultiplier = rollSpeed;

        animator.SetFloat("Vertical", v);
        animator.SetFloat("Horizontal", h);

        canMove = false;
        inAction = true;
        animator.CrossFade("Rolls", 0.2f);
    }

    public void DetectAction()
    {

        if (!canMove)
        {
            return;
        }

        //If there is no input, don't do anything
        if (rb == false && rt == false && lt == false && lb == false)
        {
            return;
        }

        //If there is input, play an attack animation
        string targetAnim = null;

        if (rb)
        {
            targetAnim = "oh_attack_1";
        }
        if (rt)
        {
            targetAnim = "oh_attack_2";
        }
        if (lt)
        {
            targetAnim = "oh_attack_3";
        }
        if (lb)
        {
            targetAnim = "th_attack_1";
        }

        if (string.IsNullOrEmpty(targetAnim))
        {
            Debug.LogWarning("Animation name is null!");
            return;
        }

        canMove = false;
        inAction = true;
        animator.CrossFade(targetAnim, 0.2f);

    }

    public void FixedTick(float d)
    {

        delta = d;

        DetectAction();

        if (inAction)
        {
            animator.applyRootMotion = true;

            _actionDelay += delta;
            if (_actionDelay > 0.3f)
            {
                inAction = false;
                _actionDelay = 0;
            }
            else
            {
                return;
            }
        }

        //Get the can move state from animator
        canMove = animator.GetBool("CanMove");

        if (!canMove)
        {
            return;
        }

        animHook.rootMotionMultiplier = 1;
        HandleRolls();

        animator.applyRootMotion = false;

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
            Vector3 targetDirection = (lockOn == false) ? moveDirection : lockOnTarget.transform.position - transform.position;
            targetDirection.y = 0;
            if (targetDirection == Vector3.zero)
            {
                targetDirection = transform.forward;
            }

            Quaternion targetRotationTemp = Quaternion.LookRotation(targetDirection);
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, targetRotationTemp, delta * moveAmount * rotateSpeed);
            transform.rotation = targetRotation;

            animator.SetBool("Lock On", lockOn);
        }

        if (!lockOn)
        {
            HandleMovementAnimations();
        }
        else
        {
            HandleLockOnAnimations(moveDirection);
        }
    }

    void HandleMovementAnimations()
    {

        animator.SetBool("Run", isRunning);
        animator.SetFloat("Vertical", moveAmount, 0.4f, delta);

    }

    void HandleLockOnAnimations(Vector3 moveDirection)
    {
        Vector3 relativeDir = transform.InverseTransformDirection(moveDirection);
        float h = relativeDir.x;
        float v = relativeDir.z;

        animator.SetFloat("Vertical", v, 0.2f, delta);
        animator.SetFloat("Horizontal", h, 0.2f, delta);
    }

    public bool OnGround()
    {

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

    public void HandleTwoHanded()
    {
        animator.SetBool("IsTwoHanded", isTwoHanded);
    }
}
