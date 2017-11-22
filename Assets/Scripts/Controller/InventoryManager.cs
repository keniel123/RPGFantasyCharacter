using RPGController.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class InventoryManager : MonoBehaviour
    {
        public string unarmedId = "Unarmed";
        public RuntimeWeapon unarmedRunTimeWeapon;

        //Use these name lists to create RuntimeWeapon items
        public List<string> rightHandWeapons;
        public List<string> leftHandWeapons;
        public List<string> spellItems;
        public List<string> consumableItems;

        public int right_Index;
        public int left_Index;
        public int spell_Index;
        public int consumable_Index;
        List<RuntimeWeapon> runtime_Right_Weapons = new List<RuntimeWeapon>();
        List<RuntimeWeapon> runtime_Left_Weapons = new List<RuntimeWeapon>();
        List<RuntimeSpellItems> runtime_Spells = new List<RuntimeSpellItems>();
        List<RuntimeConsumableItem> runtime_Consumables = new List<RuntimeConsumableItem>();

        public RuntimeSpellItems currentSpell;
        public RuntimeConsumableItem currentConsumable;

        Action currentSlot;

        public RuntimeWeapon rightHandWeapon;
        public bool hasLeftHandWeapon = true;
        public RuntimeWeapon leftHandWeapon;
        public GameObject parryCollider;
        public GameObject breathCollider;
        public GameObject blockCollider;

        StateManager states;

        public void Init(StateManager st)
        {
            states = st;

            LoadInventory();

            ParryCollider parryCol = parryCollider.GetComponent<ParryCollider>();
            parryCol.InitPlayer(st);
            CloseParryCollider();
            CloseBreathCollider();
            CloseBlockCollider();
        }

        public void LoadInventory()
        {
            unarmedRunTimeWeapon = WeaponToRuntimeWeapon(ResourcesManager.Instance.GetWeapon(unarmedId), false);

            for (int i = 0; i < rightHandWeapons.Count; i++)
            {
                RuntimeWeapon weapon = WeaponToRuntimeWeapon(ResourcesManager.Instance.GetWeapon(rightHandWeapons[i]));

                runtime_Right_Weapons.Add(weapon);

            }

            for (int i = 0; i < leftHandWeapons.Count; i++)
            {
                RuntimeWeapon weapon = WeaponToRuntimeWeapon(ResourcesManager.Instance.GetWeapon(leftHandWeapons[i]), true);
                runtime_Left_Weapons.Add(weapon);
            }

            if (runtime_Right_Weapons.Count > 0)
            {
                if (right_Index > runtime_Right_Weapons.Count - 1)
                {
                    right_Index = 0;
                }
                rightHandWeapon = runtime_Right_Weapons[right_Index];
            }


            if (runtime_Left_Weapons.Count > 0)
            {
                if (left_Index > runtime_Left_Weapons.Count - 1)
                {
                    left_Index = 0;
                }
                leftHandWeapon = runtime_Left_Weapons[left_Index];
            }

            if (rightHandWeapon != null)
            {
                EquipWeapon(rightHandWeapon, false);
            }
            if (leftHandWeapon != null)
            {
                EquipWeapon(leftHandWeapon, true);
                hasLeftHandWeapon = true;
            }

            for (int i = 0; i < spellItems.Count; i++)
            {
                SpellToRuntimeSpell(ResourcesManager.Instance.GetSpell(spellItems[i]));
            }

            if (runtime_Spells.Count > 0)
            {
                if (spell_Index > runtime_Spells.Count)
                {
                    spell_Index = 0;
                }

                EquipSpell(runtime_Spells[spell_Index]);
            }

            //Consumables
            for (int i = 0; i < consumableItems.Count; i++)
            {
                RuntimeConsumableItem runtimeConsumable = ConsumableToRunTime(ResourcesManager.Instance.GetConsumable(consumableItems[i]));
                runtime_Consumables.Add(runtimeConsumable);
            }


            if (runtime_Consumables.Count > 0)
            {
                if (consumable_Index > runtime_Consumables.Count -1)
                {
                    consumable_Index = 0;
                }

                EquipConsumable(runtime_Consumables[consumable_Index]);
            }

            InitAllDamageColliders(states);
            CloseAllDamageColliders();

        }

        public void EquipWeapon(RuntimeWeapon weapon, bool isLeft = false)
        {

            if (isLeft)
            {
                if (leftHandWeapon != null)
                {
                    leftHandWeapon.weaponModel.SetActive(false);
                }

                leftHandWeapon = weapon;
            }
            else
            {
                if (rightHandWeapon != null)
                {
                    rightHandWeapon.weaponModel.SetActive(false);
                }

                rightHandWeapon = weapon;
            }

            string targetIdle = weapon.Instance.oh_idle;
            targetIdle += (isLeft) ? StaticStrings._leftPrefix : StaticStrings._rightPrefix;
            states.animator.SetBool(StaticStrings.animParam_Mirror, isLeft);
            states.animator.Play(StaticStrings.animState_ChangeWeapon);
            //Debug.Log("TargetIdle: " + targetIdle);
            states.animator.Play(targetIdle);

            QuickSlot quickSlot = QuickSlot.Instance;
            quickSlot.UpdateSlot(
                (isLeft) ?
                QSlotType.leftHand : QSlotType.rightHand, weapon.Instance.itemIcon);

            weapon.weaponModel.SetActive(true);
        }

        public void EquipSpell(RuntimeSpellItems spell)
        {

            currentSpell = spell;

            QuickSlot quickSlot = QuickSlot.Instance;
            quickSlot.UpdateSlot(QSlotType.spell, spell.Instance.itemIcon);
        }

        public void EquipConsumable(RuntimeConsumableItem consumable)
        {
            currentConsumable = consumable;

            QuickSlot quickSlot = QuickSlot.Instance;
            quickSlot.UpdateSlot(QSlotType.item, consumable.Instance.itemIcon);
        }

        //Gets the current weapon equipped by the player
        public Weapon GetCurrentWeapon(bool isLeft)
        {
            if (isLeft)
            {
                return leftHandWeapon.Instance;
            }
            else
            {
                return rightHandWeapon.Instance;
            }
        }

        public void OpenAllDamageColliders()
        {

            if (rightHandWeapon.weaponHook != null)
                rightHandWeapon.weaponHook.OpenDamageColliders();

            if (leftHandWeapon != null && leftHandWeapon.weaponHook != null)
                leftHandWeapon.weaponHook.OpenDamageColliders();
        }

        //Close damaga colliders of weapon on left and right hands
        public void CloseAllDamageColliders()
        {

            if (rightHandWeapon != null && rightHandWeapon.weaponHook != null)
                rightHandWeapon.weaponHook.CloseDamageColliders();

            if (leftHandWeapon != null && leftHandWeapon.weaponHook != null)
                leftHandWeapon.weaponHook.CloseDamageColliders();
        }

        public void InitAllDamageColliders(StateManager stateManager)
        {

            if (rightHandWeapon != null && rightHandWeapon.weaponHook != null)
                rightHandWeapon.weaponHook.InitDamageColliders(stateManager);

            if (leftHandWeapon != null && leftHandWeapon.weaponHook != null)
                leftHandWeapon.weaponHook.InitDamageColliders(stateManager);
        }

        public void CloseParryCollider()
        {
            parryCollider.SetActive(false);
        }

        public void OpenParryCollider()
        {
            parryCollider.SetActive(true);

        }

        public RuntimeSpellItems SpellToRuntimeSpell(Spell spell, bool isLeftHand = false)
        {

            GameObject go = new GameObject();
            RuntimeSpellItems instSpell = go.AddComponent<RuntimeSpellItems>();

            instSpell.Instance = new Spell();
            StaticFunctions.DeepCopySpell(spell, instSpell.Instance);
            go.name = spell.itemName;


            runtime_Spells.Add(instSpell);

            return instSpell;
        }

        public void CreateSpellParticle(RuntimeSpellItems instSpell, bool isLeftHand, bool parentUnderRoot = false)
        {
            if (instSpell.currentParticle == null)
            {
                instSpell.currentParticle = Instantiate(instSpell.Instance.particle_prefab) as GameObject;
                instSpell.particleHook = instSpell.currentParticle.GetComponentInChildren<ParticleHook>();
                instSpell.particleHook.Init();
            }

            if (!parentUnderRoot)
            {

                Transform parent = states.animator.GetBoneTransform((isLeftHand) ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
                instSpell.currentParticle.transform.parent = parent;
                instSpell.currentParticle.transform.localRotation = Quaternion.identity;
                instSpell.currentParticle.transform.localPosition = Vector3.zero;
            }
            else
            {
                instSpell.currentParticle.transform.parent = transform;
                instSpell.currentParticle.transform.localRotation = Quaternion.identity;
                instSpell.currentParticle.transform.localPosition = new Vector3(0, 1.5f, 0.8f);
            }

            //instSpell.currentParticle.SetActive(false);
        }

        public RuntimeWeapon WeaponToRuntimeWeapon(Weapon weapon, bool isLeftHand = false)
        {

            GameObject go = new GameObject();
            RuntimeWeapon inst = go.AddComponent<RuntimeWeapon>();
            go.name = weapon.itemName;

            inst.Instance = new Weapon();
            StaticFunctions.DeepCopyWeapon(weapon, inst.Instance);

            inst.weaponStats = new WeaponStats();
            WeaponStats weaponStats = ResourcesManager.Instance.GetWeaponStats(weapon.itemName);
            if (weaponStats == null)
            {
                Debug.Log("Couldn't find weaponStats");
            }
            StaticFunctions.DeepCopyWeaponStats(weaponStats, inst.weaponStats);

            //Create weapon 3D model to scene
            inst.weaponModel = Instantiate(weapon.weaponModelPrefab) as GameObject;

            //If the weapon is used by left hand, assign left hand as weapon's parents and vice versa
            Transform parent = states.animator.GetBoneTransform((isLeftHand) ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
            inst.weaponModel.transform.parent = parent;

            //Reset the transform properties of the weapon
            inst.weaponModel.transform.localPosition =
                (isLeftHand) ?
                inst.Instance.left_model_pos : inst.Instance.right_model_pos;

            inst.weaponModel.transform.localEulerAngles =
                (isLeftHand) ?
                inst.Instance.left_model_eulerRot : inst.Instance.right_model_eulerRot;

            inst.weaponModel.transform.localScale = (isLeftHand) ?
                inst.Instance.left_model_scale : inst.Instance.right_model_scale; ;

            inst.weaponHook = inst.weaponModel.GetComponentInChildren<WeaponHook>();
            if (inst.weaponHook == null)
            {
                Debug.Log("Missing component 'WeaponHook' on weapon: " + weapon.itemName);
            }

            inst.weaponHook.InitDamageColliders(states);

            inst.weaponModel.SetActive(false);

            return inst;
        }

        public RuntimeConsumableItem ConsumableToRunTime(Consumable consumable) {
            GameObject go = new GameObject();
            go.name = consumable.itemName;
            RuntimeConsumableItem runTimeConsumable = go.AddComponent<RuntimeConsumableItem>();

            runTimeConsumable.Instance = new Consumable();
            StaticFunctions.DeepCopyConsumable(runTimeConsumable.Instance, consumable);

            //IF you have a prefab 3D model for consumable item, instantiate thr gameobject
            if (runTimeConsumable.Instance.consumablePrefab != null)
            {
                GameObject model = Instantiate(runTimeConsumable.Instance.consumablePrefab) as GameObject;
                Transform parent = states.animator.GetBoneTransform(HumanBodyBones.RightHand);
                model.transform.parent = parent;
                model.transform.localPosition = runTimeConsumable.Instance.model_pos;
                model.transform.localEulerAngles = runTimeConsumable.Instance.model_eulerRot;

                //If the scale is 0, set it to 1 as default
                Vector3 targetScale = runTimeConsumable.Instance.model_scale;
                if (targetScale == Vector3.zero)
                {
                    targetScale = Vector3.one;
                }
                model.transform.localScale = targetScale;

                runTimeConsumable.consumableModel = model;
                runTimeConsumable.consumableModel.SetActive(false);
            }

            return runTimeConsumable;

        }

        public void ChangeToNextWeapon(bool isLeft)
        {
            states.isTwoHanded = false;
            states.HandleTwoHanded();
            
            if (isLeft)
            {
                //There are no weapons in left hand to change
                if (runtime_Left_Weapons.Count == 0)
                {
                    return;
                }

                if (left_Index < runtime_Left_Weapons.Count - 1)
                    left_Index++;
                else
                    left_Index = 0;

                EquipWeapon(runtime_Left_Weapons[left_Index], true);
            }
            else
            {
                //There are no weapons in right hand to change
                if (runtime_Right_Weapons.Count == 0)
                {
                    return;
                }

                if (right_Index < runtime_Right_Weapons.Count - 1)
                    right_Index++;
                else
                    right_Index = 0;

                EquipWeapon(runtime_Right_Weapons[right_Index]);
            }

            states.actionManager.UpdateActionsOneHanded();
        }

        public void ChangeToNextSpell()
        {
            if (spell_Index < runtime_Spells.Count - 1)
            {
                spell_Index++;
            }
            else
            {
                spell_Index = 0;
            }

            EquipSpell(runtime_Spells[spell_Index]);
        }

        public void ChangeToNextConsumable()
        {
            if (consumable_Index < runtime_Consumables.Count - 1)
            {
                consumable_Index++;
            }
            else
            {
                consumable_Index = 0;
            }

            EquipConsumable(runtime_Consumables[consumable_Index]);
        }

        #region DelegateCalls
        public void OpenBreathCollider()
        {
            breathCollider.SetActive(true);
        }

        public void CloseBreathCollider()
        {
            breathCollider.SetActive(false);
        }

        public void OpenBlockCollider()
        {
            blockCollider.SetActive(true);
        }

        public void CloseBlockCollider()
        {
            blockCollider.SetActive(false);
        }

        public void EmitSpellParticle()
        {
            currentSpell.particleHook.Emit(1);
        }


        #endregion
    }

    [System.Serializable]
    public class Item
    {
        public string itemName;
        public Sprite itemIcon;
        public string itemDescription;

    }

    [System.Serializable]
    public class Weapon : Item
    {
        public string oh_idle;  //One handed idle animation name
        public string th_idle;  //Two handed idle animation name

        public List<Action> oneHandedActions;
        public List<Action> twoHandedActions;
        
        //For different weapons, parry and backstab can have different effects
        public float parryMultiplier;
        public float backstabMultiplier;
        public bool LeftHandMirror;

        public GameObject weaponModelPrefab;

        public Vector3 right_model_pos;
        public Vector3 right_model_eulerRot;
        public Vector3 right_model_scale;

        public Vector3 left_model_pos;
        public Vector3 left_model_eulerRot;
        public Vector3 left_model_scale;
        public Action GetAction(List<Action> listOfActions, ActionInput actInput)
        {
            if (listOfActions == null)
            {
                return null;
            }

            for (int i = 0; i < listOfActions.Count; i++)
            {
                if (listOfActions[i].GetFirstInput() == actInput)
                {
                    return listOfActions[i];
                }
            }

            return null;
        }

    }

    [System.Serializable]
    public class Spell : Item
    {
        public SpellType spellType;
        public SpellClass spellClass;
        public List<SpellAction> actions = new List<SpellAction>();
        public GameObject projectile;               //Projectile gameobject 
        public GameObject particle_prefab;    //Effect before shooting projectile
        public string spellEffect;

        public SpellAction GetAction(List<SpellAction> listOfActions, ActionInput actInput)
        {
            if (listOfActions == null)
            {
                return null;
            }

            for (int i = 0; i < listOfActions.Count; i++)
            {
                if (listOfActions[i].actInput == actInput)
                {
                    return listOfActions[i];
                }
            }

            return null;
        }

    }

    [System.Serializable]
    public class Consumable : Item
    {
        public string consumableEffect;
        public string targetAnim;
        public GameObject consumablePrefab;

        public Vector3 model_pos;
        public Vector3 model_eulerRot;
        public Vector3 model_scale;
    }
}