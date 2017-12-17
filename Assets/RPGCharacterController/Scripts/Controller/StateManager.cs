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
    public bool damageIsOn;
    public bool canRotate;
    public bool isSpellCasting;
    public bool isTwoHanded;
    public bool isUsingItem;
    public bool canBeParried;
    public bool isParryOn;
    public bool isBlocking;
    public bool isLeftHand;
    public bool enableIK;
    public bool onEmpty;
    public bool closeWeapons;
    public bool isInvincible;

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
    public PickableItemsManager pickableItemManager;
    
    [HideInInspector]
    public LayerMask ignoreLayers;

    [HideInInspector]
    public LayerMask ignoreForGroundCheck;

    [HideInInspector]
    public Action currentAction;

    [HideInInspector]
    public float airTimer;

    [HideInInspector]
    public string currentItemEffect;

    public ActionInput storeActionInput;
    public ActionInput storePreviousActionInput;

    float _actionDelay;
    float _kickTimer;
    public bool canKick;
    public bool holdKick;
    public bool enableItem;

    public float kickMaxTime = 0.1f;
    public float moveAmountThresh = 0.05f;

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
        ignoreForGroundCheck = ~(1 << 9 | 1 << 10);

        animator.SetBool(StaticStrings.animParam_OnGround, true);

        pickableItemManager = GetComponent<PickableItemsManager>();

        UIManager UIManager = UIManager.Instance;
        characterStats.InitCurrent();
        UIManager.EffectAll(characterStats.hp, characterStats.fp, characterStats.stamina);
        UIManager.InitSouls(characterStats.currentSouls);

        prevGround = true;

        DialogueManager.Instance.Init(this.transform);

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

    float i_timer;
    public void Tick(float d)
    {
        delta = d;
        onGround = OnGround();
        animator.SetBool(StaticStrings.animParam_OnGround, onGround);

        HandleAirTime();
        HandleInvincibility();
        HandleWeaponModelVisibility();
        HandleIK();
        HandleInActionTimer();
        pickableItemManager.Tick();

        if (onEmpty)
        {
            canAttack = true;
            canMove = true;
        }
        
        //Animation is playing, skip following actions
        if (!onEmpty && !canMove && !canAttack)
        {
            return;
        }

        if (canMove && !onEmpty)
        {
            if (moveAmount > 0.3f)
            {
                animator.CrossFade("Empty Override", 0.1f);
                onEmpty = true;
            }
        }

        MonitorKick();

        if (canAttack)
            DetectAction();

        if (canMove)
            DetectItemAction();

        animator.SetBool(StaticStrings.animParam_LockOn, lockOn);

        if (!lockOn)
        {
            HandleMovementAnimations();
        }
        else
        {
            HandleLockOnAnimations(moveDirection);
        }

        animHook.useIK = enableIK;

        //animator.SetBool(StaticStrings.animParam_Block, isBlocking);
        animator.SetBool(StaticStrings.animParam_IsLeft, isLeftHand);

        HandleBlocking();

        if (isSpellCasting)
        {
            HandleSpellCasting();
            return;
        }

        animHook.CloseRoll();
        HandleRolls();
    }
    
    public void FixedTick(float d)
    {
        delta = d;

        isBlocking = false;
        isUsingItem = animator.GetBool(StaticStrings.animParam_Interacting);
        enableItem = animator.GetBool(StaticStrings.animParam_EnableItem);
        animator.SetBool(StaticStrings.animParam_SpellCasting, isSpellCasting);
        
        onEmpty = animator.GetBool(StaticStrings.animParam_OnEmpty);

        //Animation is playing, but you cannot damage (damage colliders are off)
        if (canRotate)
        {
            HandleRotation();
        }

        //Animation is playing, skip following actions
        if (!onEmpty && !canMove && !canAttack)
        {
            return;
        }

        closeWeapons = false;
        
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

    }

    void HandleInActionTimer()
    {
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

            }
        }
    }

    void HandleIK() {

        //If player is not blocking and casting spells, disable IK
        if (!isBlocking && !isSpellCasting)
            enableIK = false;
    }

    void HandleWeaponModelVisibility() {

        if (!closeWeapons)
        {
            GameObject mainModel = null;

            if (inventoryManager.rightHandWeapon != null)
            {
                mainModel = inventoryManager.rightHandWeapon.weaponModel;
            }

            if (mainModel == null)
            {
                if (inventoryManager.leftHandWeapon != null)
                {
                    mainModel = inventoryManager.leftHandWeapon.weaponModel;
                }
            }

            if (mainModel != null)
            {
                mainModel.SetActive(!isUsingItem);
            }

            if (!isTwoHanded)
            {
                if (inventoryManager.leftHandWeapon != null && inventoryManager.rightHandWeapon != null)
                    inventoryManager.leftHandWeapon.weaponModel.SetActive(true);
            }

            if (inventoryManager.currentConsumable != null)
            {
                if (inventoryManager.currentConsumable.consumableModel != null)
                    inventoryManager.currentConsumable.consumableModel.SetActive(enableItem);
            }

        }
        //If the weapons are set to close, disable their 3D models in the scene
        else
        {
            if (inventoryManager.rightHandWeapon != null)
            {
                inventoryManager.rightHandWeapon.weaponModel.SetActive(false);
            }
            if (inventoryManager.leftHandWeapon != null)
            {
                inventoryManager.leftHandWeapon.weaponModel.SetActive(false);
            }
        }

    }

    void HandleInvincibility() {
        if (isInvincible)
        {
            i_timer += delta;
            if (i_timer > 0.5f)
            {
                i_timer = 0;
                isInvincible = false;
            }
        }
    }

    void HandleAirTime()
    {
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

        PlayAnimation(StaticStrings.animState_Rolls);
        
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

        if (inventoryManager.currentConsumable == null)
        {
            return;
        }

        //If the item is not unlimited and the player has no instance of item, skip
        if (inventoryManager.currentConsumable.itemCount < 1 && !inventoryManager.currentConsumable.unlimitedCount)
        {
            return;
        }

        RuntimeConsumableItem slot = inventoryManager.currentConsumable;
        string targetAnim = slot.Instance.targetAnim;
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

    void MonitorKick()
    {
        if (!holdKick)
        {
            if (moveAmount > moveAmountThresh)
            {
                _kickTimer += delta;
                if (_kickTimer < kickMaxTime)
                {
                    canKick = true;
                }
                else
                {
                    _kickTimer = kickMaxTime;
                    holdKick = true;
                    canKick = false;
                }
            }
            else
            {
                _kickTimer -= delta * 0.5f;
                if (_kickTimer < 0)
                {
                    _kickTimer = 0;
                    canKick = false;
                }
            }
        }
        else
        {
            if (moveAmount < moveAmountThresh)
            {
                _kickTimer -= delta;
                if (_kickTimer < 0)
                {
                    _kickTimer = 0;
                    holdKick = false;
                    canKick = false;
                }
            }
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

        if (slot.firstStep.input == ActionInput.rb)
        {
            if (canKick)
            {
                string kickAnim = "kick 1";
                if (slot.overrideKick)
                {
                    kickAnim = slot.kickAnim;
                }

                PlayAnimation(kickAnim, false);
                _kickTimer = 0;

                return;
            }
        }

        //If there is input, play an attack animation
        string targetAnim = null;
        targetAnim = slot.GetActionSteps(ref actionManager.actionStepIndex)
            .targetAnim;

        if (string.IsNullOrEmpty(targetAnim))
        {
            Debug.LogWarning("Animation name is null!");
            return;
        }

        currentAction = slot;

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
        PlayAnimation(targetAnim, slot.mirror);
        characterStats.currentStamina -= slot.staminaCost;
    }

    void SpellAction(Action slot)
    {
        if (characterStats.currentStamina < slot.staminaCost)
        {
            return;
        }

        if (slot.spellClass != inventoryManager.currentSpell.Instance.spellClass
            || characterStats.currentMana < slot.manaCost)
        {
            //Failed to cast spell
            Debug.Log("Cant cast spell!");
            PlayAnimation(StaticStrings.animState_CantCastSpell, slot.mirror);

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
        PlayAnimation(targetAnim);

        currentManaCost = spellSlot.manaCost;
        currentStaminaCost = spellSlot.staminaCost;
        animHook.InitIKForBreathSpell(spellIsMirorred);

        if (spellCastStart != null)
        {
            spellCastStart();
        }
    }

    public void PlayAnimation(string targetAnim, bool isMirrored)
    {
        canAttack = false;
        onEmpty = false;
        canMove = false;
        inAction = true;
        canKick = false;
        animator.SetBool(StaticStrings.animParam_OnEmpty, false);
        animator.SetBool(StaticStrings.animParam_Mirror, isMirrored);
        animator.CrossFade(targetAnim, 0.2f);
    }

    public void PlayAnimation(string targetAnim)
    {
        onEmpty = false;
        canMove = false;
        canAttack = false;
        inAction = true;
        isBlocking = false;
        animator.SetBool(StaticStrings.animParam_OnEmpty, false);
        animator.CrossFade(targetAnim, 0.2f);
    }

    public void InteractLogic() {

        if (pickableItemManager.worldInterCandidate.actionType == UIManager.UIActionType.talk)
        {
            pickableItemManager.worldInterCandidate.InteractActual();
            return;
        }

        Interaction interaction = ResourcesManager.Instance.GetInteraction(pickableItemManager.worldInterCandidate.interactionId);

        if (interaction.oneShot)
        {
            if(pickableItemManager.worldInteractions.Contains(pickableItemManager.worldInterCandidate)){
                pickableItemManager.worldInteractions.Remove(pickableItemManager.worldInterCandidate);
            }
        }

        if (!string.IsNullOrEmpty(interaction.specialEvent))
        {
            SessionManager.Instance.PlayEvent(interaction.specialEvent);
        }

        Vector3 targetDir = pickableItemManager.worldInterCandidate.transform.position - transform.position;
        SnapToRotation(targetDir);

        pickableItemManager.worldInterCandidate.InteractActual();

        PlayAnimation(interaction.targetAnim);
        pickableItemManager.worldInterCandidate = null;
    }

    public void SnapToRotation(Vector3 direction) {
        direction.Normalize();
        direction.y = 0;
        if (direction == Vector3.zero)
        {
            direction = transform.forward;
        }

        Quaternion targetRot = Quaternion.LookRotation(direction);
        transform.rotation = targetRot;
    }

    float currentManaCost;
    float currentStaminaCost;
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

    void EmptySpellCastDelegates()
    {
        spellCastStart = null;
        spellCastStop = null;
        spellCastLoop = null;
    }

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

                EmptySpellCastDelegates();
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
            //animator.CrossFade(targetAnim, 0.2f);
            PlayAnimation(targetAnim);
        }
    }

    bool blockAnim;
    string block_Idle_Anim;
    void HandleBlocking()
    {
        if (!isBlocking)
        {
            if (blockAnim)
            {
                PlayAnimation(block_Idle_Anim);
                //animator.CrossFade(block_Idle_Anim, 0.1f);
                blockAnim = false;
            }
        }
        else
        {

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

        characterStats.currentStamina -= currentStaminaCost;
        characterStats.currentMana -= currentManaCost;
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
            //animator.CrossFade("parry_attack", 0.2f);
            PlayAnimation(StaticStrings.animState_ParryReceived);
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

        if (!blockAnim)
        {
            block_Idle_Anim = (!isTwoHanded)
                ? inventoryManager.GetCurrentWeapon(isLeftHand).oh_idle
                : inventoryManager.GetCurrentWeapon(isLeftHand).th_idle;

            block_Idle_Anim += (isLeftHand) ? StaticStrings._leftPrefix : StaticStrings._rightPrefix;

            string targetAnim = slot.firstStep.targetAnim;
            targetAnim += (isLeftHand) ? StaticStrings._leftPrefix : StaticStrings._rightPrefix;
            PlayAnimation(targetAnim);
            //animator.CrossFade(targetAnim, 0.1f);
            blockAnim = true;
        }
    }

    void ParryAction(Action slot)
    {
        //If there is input, play an attack animation
        string targetAnim = null;
        targetAnim = slot.GetActionSteps(ref actionManager.actionStepIndex).targetAnim;

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
        //animator.CrossFade(targetAnim, 0.2f);
        PlayAnimation(targetAnim);
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

    public void Jump()
    {

        animHook.jumping = true;

        onEmpty = false;
        canMove = false;
        canAttack = false;
        inAction = true;
        animator.Play(StaticStrings.animState_JumpStart);

        //You cant block while jumping
        isBlocking = false;

        skipGroundCheck = true;
        Vector3 targetVelocity = transform.forward * 7;
        targetVelocity.y += 5;
        rigid.velocity = targetVelocity;
    }

    bool skipGroundCheck;
    float skipTimer;
    bool prevGround;
    public bool OnGround()
    {
        if (skipGroundCheck)
        {
            skipTimer += delta;
            if (skipTimer > 0.1f)
            {
                skipGroundCheck = false;
            }

            prevGround = false;
            return false;
        }

        skipTimer = 0;

        bool r = false;

        Vector3 origin = transform.position + (Vector3.up * toGround);
        Vector3 dir = -Vector3.up;
        float distance = toGround + 0.2f;

        RaycastHit hit;
        if (Physics.Raycast(origin, dir, out hit, distance, ignoreForGroundCheck))
        {
            Debug.DrawRay(origin, dir * distance, Color.blue);
            r = true;
            Vector3 targetPos = hit.point;
            transform.position = targetPos;
        }

        if (r && !prevGround)
        {
            Land();
        }

        prevGround = r;

        return r;
    }

    void Land()
    {
        animHook.jumping = false;

        if (airTimer < 0.8f)
        {
            return;
        }

        onEmpty = false;
        canMove = false;
        canAttack = false;
        inAction = true;
        isBlocking = false;

        if (moveAmount == 0)
        {
            animator.Play(StaticStrings.animState_JumpLand);
        }
        else
        {
            //Roll
            if (moveDirection == Vector3.zero)
            {
                moveDirection = transform.forward;
            }

            Quaternion targetRot = Quaternion.LookRotation(moveDirection);
            transform.rotation = targetRot;
            animHook.InitForRoll();
            animHook.rootMotionMultiplier = rollSpeed;

            animator.SetFloat(StaticStrings.Input_Vertical, 1);
            animator.SetFloat(StaticStrings.Input_Horizontal, 0);

            animator.CrossFade(StaticStrings.animState_Rolls, 0.2f);
        }
    }

    public void HandleTwoHanded()
    {
        bool isRight = true;
        if (inventoryManager.rightHandWeapon == null)
        {
            return;
        }

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

            animator.CrossFade(targetAnim, 0.2f);
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

    public void SubstractStaminaOverTime()
    {
        characterStats.currentStamina -= currentStaminaCost;
    }

    public void SubstractManaOverTime()
    {
        characterStats.currentMana -= currentManaCost;
    }

    public void AffectBlocking()
    {
        isBlocking = true;
    }

    public void StopAffectingBlocking()
    {
        isBlocking = false;
    }

    public void DoDamage(AIAttacks attack) {
        
        if (isInvincible)
        {
            return;
        }

        int damageTaken = 5;

        //characterStats.poise += damageTaken;
        characterStats.currentHealth -= damageTaken;

        if (canMove)
        {
            if (attack != null && attack.hasReactAnim)
            {
                animator.Play(attack.reactAnimation);
            }
            else
            {
                int rand = Random.Range(0, 100);
                string targetAnim = (rand > 50) ? StaticStrings.damage1 : StaticStrings.damage2;
                animator.Play(targetAnim);
            }
        }

        Debug.Log("Damage is: " + damageTaken + ", Poise is: " + characterStats.poise);

        animator.SetBool(StaticStrings.animParam_OnEmpty, false);
        canMove = false;
        onEmpty = false;
        inAction = true;
        isInvincible = true;
        animator.applyRootMotion = true;
    }
}
