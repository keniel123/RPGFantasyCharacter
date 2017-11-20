using RPGController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    [Header("Initilazation")]
    public GameObject activeModel;

    [Header("Player Stats")]
    public Attributes attributes;
    public CharacterStats characterStats;

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
    public float backStabOffset = 1.4f;


    [Header("States")]
    public bool isRunning;
    public bool onGround;
    public bool lockOn;
    public bool inAction;
    public bool canMove;
    public bool canAttack;
    public bool isSpellCasting;
    public bool isTwoHanded;
    public bool isUsingItem;
    public bool canBeParried;
    public bool isParryOn;
    public bool isBlocking;
    public bool isLeftHand;
    public bool enableIK;
    public bool onEmpty;

    [Header("Other")]
    public EnemyTarget lockOnTarget;
    public Transform lockOnTransform;
    public AnimationCurve rollAnimCurve;
    //public EnemyStates parryTarget;

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

    [HideInInspector]
    public Action currentAction;

    [HideInInspector]
    public float airTimer;

    public ActionInput storeActionInput;
    public ActionInput storePreviousActionInput;

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

        animator.SetBool(StaticStrings.animParam_OnGround, true);

        characterStats.InitCurrent();
        UIManager.Instance.EffectAll(characterStats.hp, characterStats.fp, characterStats.stamina);
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
        animator.SetBool(StaticStrings.animParam_OnGround, onGround);

        if (!onGround)
        {
            airTimer += delta;
        }
        else
        {
            airTimer = 0;
        }

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
            animHook.InitForRoll();
            animHook.rootMotionMultiplier = rollSpeed;

        }
        //If stepping back
        else
        {
            animHook.rootMotionMultiplier = 1.3f;
        }

        animator.SetFloat(StaticStrings.Input_Vertical, v);
        animator.SetFloat(StaticStrings.Input_Horizontal, h);

        onEmpty = false;
        canMove = false;
        canAttack = false;
        inAction = true;
        animator.CrossFade(StaticStrings.animState_Rolls, 0.2f);

        //You cant block while rolling
        isBlocking = false;
    }

    public void DetectItemAction()
    {

        //You cannot use an item while already using another or blocking
        if (!onEmpty || isUsingItem || isBlocking)
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
        if (!canAttack && (!onEmpty || isUsingItem || isSpellCasting))
        {
            return;
        }

        //If there is no input, don't do anything
        if (rb == false && rt == false && lt == false && lb == false)
        {
            return;
        }

        ActionInput targetInput = actionManager.GetActionInput(this);
        storeActionInput = targetInput;
        if (!onEmpty)
        {
            animHook.killDelta = true;
            targetInput = storePreviousActionInput;
        }

        storePreviousActionInput = targetInput;

        //If you can't move but you're not empty, the attack is a combo action
        Action slot = actionManager.GetActionFromInput(targetInput);

        if (slot == null)
        {
            Debug.Log("Detect action slot is null!");
            return;
        }

        switch (slot.actionType)
        {
            case ActionType.attack:
                AttackAction(slot);
                break;
            case ActionType.block:
                BlockAction(slot);
                break;
            case ActionType.spells:
                SpellAction(slot);
                break;
            case ActionType.parry:
                ParryAction(slot);
                break;
            default:
                break;
        }
    }

    void AttackAction(Action slot)
    {

        if (characterStats.currentStamina < slot.staminaCost)
        {
            return;
        }

        if (CheckForParry(slot))
        {
            return;
        }

        if (CheckForBackStab(slot))
        {
            return;
        }

        //If there is input, play an attack animation
        string targetAnim = null;
        targetAnim = slot.GetActionSteps(ref actionManager.actionStepIndex)
            .GetBranch(storeActionInput).targetAnim;
        
        if (string.IsNullOrEmpty(targetAnim))
        {
            Debug.LogWarning("Animation name is null!");
            return;
        }

        currentAction = slot;
        onEmpty = false;
        canAttack = false;
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
        animator.SetFloat(StaticStrings.animParam_AnimSpeed, targetSpeed);
        animator.SetBool(StaticStrings.animParam_Mirror, slot.mirror);
        //animator.CrossFade(targetAnim, 0.2f);
        animator.Play(targetAnim);
        characterStats.currentStamina -= slot.staminaCost;
    }

    void SpellAction(Action slot)
    {
        if (slot.spellClass != inventoryManager.currentSpell.Instance.spellClass
            || characterStats.stamina < slot.staminaCost || characterStats.currentMana < slot.manaCost)
        {
            //Failed to cast spell
            Debug.Log("Cant cast spell!");
            animator.SetBool(StaticStrings.animParam_Mirror, slot.mirror);
            animator.CrossFade(StaticStrings.animState_CantCastSpell, 0.2f);
            canMove = false;
            canAttack = false;
            inAction = true;
            return;
        }

        ActionInput inp = actionManager.GetActionInput(this);
        if (inp == ActionInput.lb)
        {
            inp = ActionInput.rb;
        }
        if (inp == ActionInput.lt)
        {
            inp = ActionInput.rt;
        }

        Spell spellInst = inventoryManager.currentSpell.Instance;
        SpellAction spellSlot = spellInst.GetAction(spellInst.actions, inp);

        if (spellSlot == null)
        {
            Debug.Log("CAN'T FIND SPELL SLOT");
            return;
        }

        SpellEffectManager.Instance.UseSpellEffect(spellInst.spellEffect, this);

        isSpellCasting = true;
        spellCastTime = 0;
        max_SpellCastTime = spellSlot.castTime;
        spellTargetAnim = spellSlot.throwAnim;
        spellIsMirorred = slot.mirror;
        currentSpellType = spellInst.spellType;

        string targetAnim = spellSlot.targetAnim;
        if (spellIsMirorred)
        {
            targetAnim += StaticStrings._leftPrefix;
        }
        else
        {
            targetAnim += StaticStrings._rightPrefix;
        }

        projectileCandidate = inventoryManager.currentSpell.Instance.projectile;
        //If spellIsMirorred == true --> Left hand
        inventoryManager.CreateSpellParticle(inventoryManager.currentSpell, spellIsMirorred, (spellInst.spellType == SpellType.Looping));
        animator.SetBool(StaticStrings.animParam_SpellCasting, true);
        animator.SetBool(StaticStrings.animParam_Mirror, slot.mirror);
        animator.CrossFade(targetAnim, 0.2f);

        characterStats.currentStamina -= slot.staminaCost;
        characterStats.currentMana -= slot.manaCost;

        animHook.InitIKForBreathSpell(spellIsMirorred);

        if (spellCastStart != null)
        {
            spellCastStart();
        }
    }

    float spellCastTime;
    float max_SpellCastTime;
    string spellTargetAnim;
    bool spellIsMirorred;
    SpellType currentSpellType;
    GameObject projectileCandidate;

    //Spell Casting delegates
    public delegate void SpellCastStart();
    public SpellCastStart spellCastStart;

    public delegate void SpellCastStop();
    public SpellCastStop spellCastStop;

    public delegate void SpellCastLoop();
    public SpellCastStart spellCastLoop;

    void HandleSpellCasting()
    {

        if (currentSpellType == SpellType.Looping)
        {
            enableIK = true;
            animHook.currentHand = (spellIsMirorred) ? AvatarIKGoal.LeftHand : AvatarIKGoal.RightHand;

            if ((rb == false && lb == false) || characterStats.currentMana < 2)
            {
                isSpellCasting = false;
                enableIK = false;

                inventoryManager.breathCollider.SetActive(false);
                inventoryManager.blockCollider.SetActive(false);

                if (spellCastStop != null)
                {
                    spellCastStop();
                }

                return;
            }

            if (spellCastLoop != null)
            {
                spellCastLoop();
            }

            characterStats.currentMana -= 0.5f;

            return;
        }

        spellCastTime += delta;
        if (inventoryManager.currentSpell.currentParticle != null)
        {
            inventoryManager.currentSpell.currentParticle.SetActive(true);
        }

        if (spellCastTime > max_SpellCastTime)
        {
            onEmpty = false;
            canMove = false;
            canAttack = false;
            inAction = true;
            isSpellCasting = false;

            string targetAnim = spellTargetAnim;
            animator.SetBool(StaticStrings.animParam_Mirror, spellIsMirorred);
            animator.CrossFade(targetAnim, 0.2f);

        }
    }

    public void ThrowProjectile()
    {
        if (projectileCandidate == null)
        {
            return;
        }

        GameObject go = Instantiate(projectileCandidate) as GameObject;
        Transform parent = animator.GetBoneTransform((spellIsMirorred) ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
        go.transform.position = parent.position;

        if (lockOnTransform && lockOn)
        {
            go.transform.LookAt(lockOnTransform.position);
        }
        else
        {
            go.transform.rotation = transform.rotation;
        }

        Projectile projectileBehaviour = go.GetComponent<Projectile>();
        projectileBehaviour.Init();
    }

    bool CheckForParry(Action slot)
    {

        if (!slot.canParry)
        {
            return false;
        }

        EnemyStates parryTarget = null;
        Vector3 origin = transform.position;
        origin.y += 1;
        Vector3 raycastDir = transform.forward;
        RaycastHit hit;

        if (Physics.Raycast(origin, raycastDir, out hit, 3, ignoreLayers))
        {
            parryTarget = hit.transform.root.GetComponent<EnemyStates>();
        }

        if (parryTarget == null)
        {
            return false;
        }

        if (parryTarget.parriedBy == null)
        {
            return false;
        }

        //Direction towards the player
        Vector3 direction = parryTarget.transform.position - transform.position;
        direction.Normalize();
        direction.y = 0;
        float angle = Vector3.Angle(transform.forward, direction);

        Debug.Log("Parry angle: " + angle);
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

            parryTarget.IsGettingParried(slot, inventoryManager.GetCurrentWeapon(slot.mirror));

            onEmpty = false;
            canMove = false;
            canAttack = false;
            inAction = true;
            animator.SetBool(StaticStrings.animParam_Mirror, slot.mirror);
            animator.CrossFade("parry_attack", 0.2f);
            lockOnTarget = null;
            return true;
        }

        return true;
    }

    bool CheckForBackStab(Action slot)
    {

        //If the weapon has no back stab ability, skip
        if (!slot.canBackStab)
        {
            return false;
        }

        EnemyStates backStabTarget = null;
        Vector3 origin = transform.position;
        origin.y += 1;
        Vector3 raycastDir = transform.forward;
        RaycastHit hit;

        if (Physics.Raycast(origin, raycastDir, out hit, 1, ignoreLayers))
        {
            backStabTarget = hit.transform.GetComponentInParent<EnemyStates>();
        }

        if (backStabTarget == null)
        {
            return false;
        }

        //Direction towards the player
        Vector3 direction = transform.position - backStabTarget.transform.position;
        direction.Normalize();
        direction.y = 0;
        float angle = Vector3.Angle(backStabTarget.transform.forward, direction);

        Debug.Log("Backstab angle: " + angle);

        if (angle > 150)
        {
            //Get target position
            Vector3 targetPos = direction * backStabOffset;
            targetPos += backStabTarget.transform.position;
            transform.position = targetPos;

            backStabTarget.transform.rotation = transform.rotation;
            backStabTarget.IsGettingBackStabbed(slot, inventoryManager.GetCurrentWeapon(slot.mirror));
            onEmpty = false;
            canMove = false;
            canAttack = false;
            inAction = true;
            animator.SetBool(StaticStrings.animParam_Mirror, slot.mirror);
            animator.CrossFade(StaticStrings.animParam_ParryAttack, 0.2f);
            lockOnTarget = null;
            return true;
        }

        return false;
    }

    void BlockAction(Action slot)
    {
        enableIK = true;
        isBlocking = true;

        //If it's mirrored, than that means you're blocking with the left hand and vice versa
        isLeftHand = slot.mirror;
        animHook.currentHand = (isLeftHand) ? AvatarIKGoal.LeftHand : AvatarIKGoal.RightHand;
        animHook.InitIKForShield(isLeftHand);
    }

    void ParryAction(Action slot)
    {
        //If there is input, play an attack animation
        string targetAnim = null;
        targetAnim = slot.GetActionSteps(ref actionManager.actionStepIndex).GetBranch(storeActionInput).targetAnim;

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

        animator.SetFloat(StaticStrings.animParam_AnimSpeed, targetSpeed);
        canBeParried = slot.canBeParried;
        onEmpty = false;
        canMove = false;
        canAttack = false;
        inAction = true;
        animator.SetBool(StaticStrings.animParam_Mirror, slot.mirror);
        animator.CrossFade(targetAnim, 0.2f);

    }

    public void FixedTick(float d)
    {
        delta = d;

        isBlocking = false;
        isUsingItem = animator.GetBool(StaticStrings.animParam_Interacting);
        animator.SetBool(StaticStrings.animParam_SpellCasting, isSpellCasting);

        inventoryManager.rightHandWeapon.weaponModel.SetActive(!isUsingItem);

        animator.SetBool(StaticStrings.animParam_Block, isBlocking);
        animator.SetBool(StaticStrings.animParam_IsLeft, isLeftHand);

        //If player is not blocking and casting spells, disable IK
        if (!isBlocking && !isSpellCasting)
        {
            enableIK = false;
        }

        animHook.useIK = enableIK;

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

        onEmpty = animator.GetBool(StaticStrings.animParam_OnEmpty);
        //Get the can move state from animator
        //canMove = animator.GetBool(StaticStrings.animParam_CanMove);

        if (onEmpty)
        {
            canAttack = true;
            canMove = true;
        }

        if (!onEmpty && !canMove && !canAttack)
        {
            return;
        }

        if (canMove && !onEmpty)
        {
            if (moveAmount > 0.3f)
            {
                animator.CrossFade("Empty Override", 0.075f);
                onEmpty = true;
            }
        }

        if (canAttack)
        {
            if (IsInput())
            {
                animator.CrossFade("Empty Override", 0.075f);
            }
        }

        if (canAttack)
        {
            DetectAction();
        }

        if (!canMove)
        {
            DetectItemAction();
        }

        animator.applyRootMotion = false;

        //While moving there's no need for drag, but if the character is not moving
        //increase the drag, so that it won't slide across the ground surface
        rigid.drag = (moveAmount > 0 || onGround == false) ? 0 : 4;

        float targetSpeed = moveSpeed;

        //If the player is using an item or casting a spell, move slowly
        if (isUsingItem || isSpellCasting)
        {
            isRunning = false;
            moveAmount = Mathf.Clamp(moveAmount, 0, 0.65f);
        }

        //If running, go faster
        if (isRunning)
        {
            targetSpeed = runSpeed;
        }

        if (onGround && canMove)
        {
            rigid.velocity = moveDirection * (targetSpeed * moveAmount);
        }

        if (isRunning)
        {
            lockOn = false;
        }

        HandleRotation();
        animator.SetBool(StaticStrings.animParam_LockOn, lockOn);

        if (!lockOn)
        {
            HandleMovementAnimations();
        }
        else
        {
            HandleLockOnAnimations(moveDirection);
        }

        if (isSpellCasting)
        {
            HandleSpellCasting();
            return;
        }


        animHook.CloseRoll();
        HandleRolls();
    }

    public bool IsInput()
    {
        if (rt || rb || lb || lt || rollInput)
        {
            return true;
        }

        return false;

    }
    void HandleRotation()
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
    }

    void HandleMovementAnimations()
    {
        animator.SetBool(StaticStrings.animParam_Run, isRunning);
        animator.SetFloat(StaticStrings.Input_Vertical, moveAmount, 0.4f, delta);
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
        float distance = toGround + 0.2f;

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
        //animator.SetBool(StaticStrings.animParam_IsTwoHanded, isTwoHanded);

        bool isRight = true;
        //Get the default (right hand) weapon currently equiiped
        Weapon weapon = inventoryManager.rightHandWeapon.Instance;

        //If the right hand weapon doesn't exist, then get the left hand weapon
        if (weapon == null)
        {
            weapon = inventoryManager.leftHandWeapon.Instance;
            isRight = false;
        }

        //Still, if the ledt hand weapon is null, then return
        if (weapon == null)
        {
            return;
        }

        if (isTwoHanded)
        {
            animator.CrossFade(weapon.th_idle, 0.2f);
            actionManager.UpdateActionsTwoHanded();

            if (isRight)
            {
                if (inventoryManager.leftHandWeapon.Instance != null)
                    inventoryManager.leftHandWeapon.weaponModel.SetActive(false);
            }
            else
            {
                if (inventoryManager.rightHandWeapon.Instance != null)
                    inventoryManager.rightHandWeapon.weaponModel.SetActive(false);
            }
        }
        else
        {
            string targetAnim = weapon.oh_idle;
            targetAnim += (isRight) ? StaticStrings._rightPrefix : StaticStrings._leftPrefix;

            //animator.CrossFade(targetAnim, 0.2f);
            animator.Play(StaticStrings.animState_EquipWeapon_OH);
            actionManager.UpdateActionsOneHanded();

            if (isRight)
            {
                if (inventoryManager.leftHandWeapon.Instance != null)
                    inventoryManager.leftHandWeapon.weaponModel.SetActive(true);
            }
            else
            {
                if (inventoryManager.rightHandWeapon.Instance != null)
                    inventoryManager.rightHandWeapon.weaponModel.SetActive(true);
            }
        }
    }

    public void IsGettingParried()
    {


    }

    public void AddHealth()
    {
        characterStats.fp++;
    }

    public void MonitorStats()
    {
        if (isRunning && moveAmount > 0)
        {
            Debug.Log("Running");
            characterStats.currentStamina -= delta * 5;
        }
        else
        {
            characterStats.currentStamina += delta;
        }

        if (characterStats.currentStamina > characterStats.fp)
        {
            characterStats.currentStamina = characterStats.fp;
        }

        if (characterStats.currentStamina < 0)
        {
            characterStats.currentStamina = 0;
        }

        characterStats.currentHealth = Mathf.Clamp(characterStats.currentHealth, 0, characterStats.hp);
        characterStats.currentMana = Mathf.Clamp(characterStats.currentMana, 0, characterStats.fp);
    }
}
