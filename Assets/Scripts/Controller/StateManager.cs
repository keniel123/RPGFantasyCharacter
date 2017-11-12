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
    public bool itemInput;

    [Header("Stats")]
    public float moveSpeed = 2f;        //Walking & jogging speed
    public float runSpeed = 3.5f;       //Running speed
    public float rotateSpeed = 5f;      //Movement turn speed
    public float toGround = 0.5f;
    public float rollSpeed = 1.0f;
    public float parryOffset = 1.4f;

    [Header("States")]
    public bool isRunning;
    public bool onGround;
    public bool lockOn;
    public bool inAction;
    public bool canMove;
    public bool isTwoHanded;
    public bool isUsingItem;
    public bool canBeParried;
    public bool isParryOn;
    public bool isBlocking;
    public bool isLeftHand;

    [Header("Other")]
    public EnemyTarget lockOnTarget;
    public Transform lockOnTransform;
    public AnimationCurve rollAnimCurve;
    public EnemyStates parryTarget;

    [HideInInspector]
    public Animator animator;

    [HideInInspector]
    public Rigidbody rigid;

    [HideInInspector]
    public AnimatorHook animHook;

    [HideInInspector]
    public ActionManager actionManager;
    
    [HideInInspector]
    public InventoryManager inventoryManager;

    [HideInInspector]
    public LayerMask ignoreLayers;

    float _actionDelay;

    [HideInInspector]
    public float delta;

    public void Init()
    {
        SetupAnimator();
        rigid = GetComponent<Rigidbody>();
        rigid.drag = 4;
        rigid.angularDrag = 999;
        rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        inventoryManager = GetComponent<InventoryManager>();
        inventoryManager.Init(this);

        actionManager = GetComponent<ActionManager>();
        actionManager.Init(this);

        animHook = activeModel.GetComponent<AnimatorHook>();

        if (animHook == null)
        {
            animHook = activeModel.AddComponent<AnimatorHook>();
        }

        animHook.Init(this, null);

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
        if (!rollInput || isUsingItem)
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

            animHook.rootMotionMultiplier = rollSpeed;
            animHook.InitForRoll();

        }
        //If stepping back
        else
        {
            animHook.rootMotionMultiplier = 1.3f;
        }
        
        animator.SetFloat("Vertical", v);
        animator.SetFloat("Horizontal", h);

        canMove = false;
        inAction = true;
        animator.CrossFade("Rolls", 0.2f);
    }

    public void DetectItemAction() {

        //You cannot use an item while already using another or blocking
        if (!canMove || isUsingItem || isBlocking)
        {
            return;
        }

        if (!itemInput)
        {
            return;
        }

        ItemAction itemAction = actionManager.consumableItem;
        string targetAnim = itemAction.targetAnim;
        if (string.IsNullOrEmpty(targetAnim))
        {
            Debug.LogError("No animation found for item action!");
            return;
        }

        isUsingItem = true;
        animator.Play(targetAnim);
    }

    public void DetectAction()
    {
        if (!canMove || isUsingItem)
        {
            return;
        }

        //If there is no input, don't do anything
        if (rb == false && rt == false && lt == false && lb == false)
        {
            return;
        }


        //If there is an action get the action slot
        Action slot = actionManager.GetActionSlot(this);
        if (slot == null)
        {
            return;
        }

        switch (slot.type)
        {   
            case ActionType.attack:
                AttackAction(slot);
                break;
            case ActionType.block:
                BlockAction(slot);
                break;
            case ActionType.spells:
                break;
            case ActionType.parry:
                ParryAction(slot);
                break;
            default:
                break;
        }
    }

    void AttackAction(Action slot) {

        if (CheckForParry(slot))
        {
            return;
        }

        //If there is input, play an attack animation
        string targetAnim = null;
        targetAnim = slot.targetAnim;

        if (string.IsNullOrEmpty(targetAnim))
        {
            Debug.LogWarning("Animation name is null!");
            return;
        }

        canMove = false;
        inAction = true;

        float targetSpeed = 1;
        if (slot.chageSpeed)
        {
            targetSpeed = slot.animSpeed;
            if (targetSpeed == 0)
            {
                targetSpeed = 1;
            }
        }

        canBeParried = slot.canBeParried;
        animator.SetFloat("AnimSpeed", targetSpeed);
        animator.SetBool("Mirror", slot.mirror);
        animator.CrossFade(targetAnim, 0.2f);

    }

    bool CheckForParry(Action slot) {
        if (parryTarget == null)
        {
            return false;
        }

        //Check distance
        float distance = Vector3.Distance(parryTarget.transform.position, transform.position);
        if (distance > 3)
        {
            return false;
        }

        //Direction towards the player
        Vector3 direction = parryTarget.transform.position - transform.position;
        direction.Normalize();
        direction.y = 0;
        float angle = Vector3.Angle(transform.forward, direction);

        if (angle < 60)
        {
            //Get target position
            Vector3 targetPos = -direction * parryOffset;
            targetPos += parryTarget.transform.position;
            transform.position = targetPos;

            if (direction == Vector3.zero)
            {
                direction = -parryTarget.transform.forward;
            }

            //Make enemy look at player
            Quaternion enemyRot = Quaternion.LookRotation(-direction);
            parryTarget.transform.rotation = enemyRot;
            
            Quaternion playerRot = Quaternion.LookRotation(direction);
            transform.rotation = playerRot;

            parryTarget.IsGettingParried();


            canMove = false;
            inAction = true;
            animator.SetBool("Mirror", slot.mirror);
            animator.CrossFade("parry_attack", 0.2f);
            
            return true;
        }

        return true;
    }

    void BlockAction(Action slot) {
        isBlocking = true;

        //If it's mirrored, than that means you're blocking with the left hand and vice versa
        isLeftHand = slot.mirror;
    }

    void ParryAction(Action slot)
    {
        //If there is input, play an attack animation
        string targetAnim = null;
        targetAnim = slot.targetAnim;

        if (string.IsNullOrEmpty(targetAnim))
        {
            Debug.LogWarning("Animation name is null!");
            return;
        }

        float targetSpeed = 1;
        if (slot.chageSpeed)
        {
            targetSpeed = slot.animSpeed;
            if (targetSpeed == 0)
            {
                targetSpeed = 1;
            }
        }

        animator.SetFloat("AnimSpeed", targetSpeed);
        canBeParried = slot.canBeParried;
        canMove = false;
        inAction = true;
        animator.SetBool("Mirror", slot.mirror);
        animator.CrossFade(targetAnim, 0.2f);

    }

    public void FixedTick(float d)
    {
        delta = d;

        isBlocking = false;
        isUsingItem = animator.GetBool("Interacting");
        DetectAction();
        DetectItemAction();
        inventoryManager.rightHandWeapon.weaponModel.SetActive(!isUsingItem);

        animator.SetBool("Block", isBlocking);
        animator.SetBool("IsLeft", isLeftHand);

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

        //animHook.rootMotionMultiplier = 1;
        animHook.CloseRoll();
        HandleRolls();

        animator.applyRootMotion = false;

        //While moving there's no need for drag, but if the character is not moving
        //increase the drag, so that it won't slide across the ground surface
        rigid.drag = (moveAmount > 0 || !onGround == false) ? 0 : 4;

        float targetSpeed = moveSpeed;

        //If the player is using an item, move slowly
        if (isUsingItem)
        {
            isRunning = false;
            moveAmount = Mathf.Clamp(moveAmount, 0, 0.45f);
        }

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
            Vector3 targetDirection = (lockOn == false) ? //If you're not locked on assign move direction
                moveDirection
                :
                (lockOnTransform != null) ? //If you're locked on a target, check if the target is NULL or not
                lockOnTransform.transform.position - transform.position //If it's not null, assign this as move direction
                :
                moveDirection;
            targetDirection.y = 0;
            if (targetDirection == Vector3.zero)
            {
                targetDirection = transform.forward;
            }

            Quaternion targetRotationTemp = Quaternion.LookRotation(targetDirection);
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, targetRotationTemp, delta * moveAmount * rotateSpeed);
            transform.rotation = targetRotation;

            animator.SetBool("Lock On", lockOn);

            if (!lockOn)
            {
                HandleMovementAnimations();
            }
            else
            {
                HandleLockOnAnimations(moveDirection);
            }
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

        if (isTwoHanded)
        {
            actionManager.UpdateActionsTwoHanded();
        }
        else
        {
            actionManager.UpdateActionsOneHanded();
        }
    }

    public void IsGettingParried() {


    }
}
