using RPGController.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class InventoryManager : MonoBehaviour
    {
        //Use these name lists to create RuntimeWeapon items
        public List<string> rightHandWeapons;
        public List<string> leftHandWeapons;
        public List<string> spellItems;

        public int right_Index;
        public int left_Index;
        public int spell_Index;
        List<RuntimeWeapon> runtime_Right_Weapon = new List<RuntimeWeapon>();
        List<RuntimeWeapon> runtime_Left_Weapon = new List<RuntimeWeapon>();
        List<RuntimeSpellItems> runtime_Spells = new List<RuntimeSpellItems>();

        public RuntimeSpellItems currentSpell;
        Action currentSlot;

        public RuntimeWeapon rightHandWeapon;
        public bool hasLeftHandWeapon = true;
        public RuntimeWeapon leftHandWeapon;
        public GameObject parryCollider;
        StateManager states;

        public void Init(StateManager st)
        {
            states = st;

            LoadInventory();

            ParryCollider parryCol = parryCollider.GetComponent<ParryCollider>();
            parryCol.InitPlayer(st);
            CloseParryCollider();
        }

        public void LoadInventory() {

            for (int i = 0; i < rightHandWeapons.Count; i++)
            {
                WeaponToRuntimeWeapon(ResourcesManager.Instance.GetWeapon(rightHandWeapons[i]));
            }

            for (int i = 0; i < leftHandWeapons.Count; i++)
            {
                WeaponToRuntimeWeapon(ResourcesManager.Instance.GetWeapon(leftHandWeapons[i]), true);
            }

            if (runtime_Right_Weapon.Count > 0)
            {
                if (right_Index > runtime_Right_Weapon.Count -1)
                {
                    right_Index = 0;
                }
                rightHandWeapon = runtime_Right_Weapon[right_Index];
            }


            if (runtime_Left_Weapon.Count > 0)
            {
                if (left_Index > runtime_Left_Weapon.Count - 1)
                {
                    left_Index = 0;
                }
                leftHandWeapon = runtime_Left_Weapon[left_Index];
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

            InitAllDamageColliders(states);
            CloseAllDamageColliders();

        }

        public void EquipWeapon(RuntimeWeapon weapon, bool isLeft = false) {

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
            states.animator.Play(targetIdle);

            QuickSlot quickSlot = QuickSlot.Instance;
            quickSlot.UpdateSlot(
                (isLeft) ?
                QSlotType.leftHand : QSlotType.rightHand, weapon.Instance.itemIcon);

            weapon.weaponModel.SetActive(true);
        }
        
        public void EquipSpell(RuntimeSpellItems spell) {

            currentSpell = spell;

            QuickSlot quickSlot = QuickSlot.Instance;
            quickSlot.UpdateSlot(QSlotType.spell, spell.Instance.itemIcon);
        }

        //Gets the current weapon equipped by the player
        public Weapon GetCurrentWeapon(bool isLeft) {
            if (isLeft)
            {
                return leftHandWeapon.Instance;
            }
            else
            {
                return rightHandWeapon.Instance;
            }
        }

        public void OpenAllDamageColliders() {

            if (rightHandWeapon.weaponHook != null)
                rightHandWeapon.weaponHook.OpenDamageColliders();

            if (leftHandWeapon.weaponHook != null)
                leftHandWeapon.weaponHook.OpenDamageColliders();
        }

        //Close damaga colliders of weapon on left and right hands
        public void CloseAllDamageColliders() {

            if (rightHandWeapon.weaponHook != null)
                rightHandWeapon.weaponHook.CloseDamageColliders();

            if (leftHandWeapon.weaponHook != null)
                leftHandWeapon.weaponHook.CloseDamageColliders();
        }

        public void InitAllDamageColliders(StateManager stateManager) {

            if (rightHandWeapon.weaponHook != null)
                rightHandWeapon.weaponHook.InitDamageColliders(stateManager);

            if (leftHandWeapon.weaponHook != null)
                leftHandWeapon.weaponHook.InitDamageColliders(stateManager);
        }

        public void CloseParryCollider() {
            parryCollider.SetActive(false);
        }

        public void OpenParryCollider()
        {
            parryCollider.SetActive(true);

        }

        public RuntimeSpellItems SpellToRuntimeSpell(Spell spell, bool isLeftHand = false) {

            GameObject go = new GameObject();
            RuntimeSpellItems instSpell = go.AddComponent<RuntimeSpellItems>();

            instSpell.Instance = new Spell();
            StaticFunctions.DeepCopySpell(spell, instSpell.Instance);
            go.name = spell.itemName;


            runtime_Spells.Add(instSpell);

            return instSpell;
        }

        public void CreateSpellParticle(RuntimeSpellItems instSpell, bool isLeftHand) {
            if (instSpell.currentParticle == null)
            {
                instSpell.currentParticle = Instantiate(instSpell.Instance.particle_prefab) as GameObject;
            }

            Transform parent = states.animator.GetBoneTransform((isLeftHand) ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
            instSpell.currentParticle.transform.parent = parent;
            instSpell.currentParticle.transform.localRotation = Quaternion.identity;
            instSpell.currentParticle.transform.localPosition = Vector3.zero;
            instSpell.currentParticle.SetActive(false);
        }

        public RuntimeWeapon WeaponToRuntimeWeapon(Weapon weapon, bool isLeftHand = false) {

            GameObject go = new GameObject();
            RuntimeWeapon inst = go.AddComponent<RuntimeWeapon>();
            go.name = weapon.itemName;

            inst.Instance = new Weapon();
            StaticFunctions.DeepCopyWeapon(weapon, inst.Instance);

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
            inst.weaponHook.InitDamageColliders(states);

            if (isLeftHand)
            {
                runtime_Left_Weapon.Add(inst);
            }
            else
            {
                runtime_Right_Weapon.Add(inst);
            }

            inst.weaponModel.SetActive(false);

            return inst;
        }

        public void ChangeToNextWeapon(bool isLeft) {
            if (isLeft)
            {
                if (left_Index < runtime_Left_Weapon.Count - 1)
                    left_Index++;
                else
                    left_Index = 0;

                EquipWeapon(runtime_Left_Weapon[left_Index], true);

            }
            else
            {
                if (right_Index < runtime_Right_Weapon.Count - 1)
                    right_Index++;
                else
                    right_Index = 0;

                EquipWeapon(runtime_Right_Weapon[right_Index]);
            }

            states.actionManager.UpdateActionsOneHanded();
        }

        public void ChangeToNextSpell() {
            if (spell_Index < runtime_Spells.Count -1)
            {
                spell_Index++;
            }
            else
            {
                spell_Index = 0;
            }

            EquipSpell(runtime_Spells[spell_Index]);
        }
    }

    [System.Serializable]
    public class Item {
        public string itemName;
        public Sprite itemIcon;
        public string itemDescription;

    }

    [System.Serializable]
    public class Weapon : Item
    {
        public string oh_idle;  //One handed idle animation name
        public string th_idle;  //Two handed idle animation name

        public List<Action> actions;
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
                if (listOfActions[i].input == actInput)
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
}