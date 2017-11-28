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
        public List<int> rightHandWeapons;
        public List<int> leftHandWeapons;
        public List<string> spellItems;
        public List<int> consumableItems;

        public int right_Index;
        public int left_Index;
        public int spell_Index;
        public int consumable_Index;
        List<RuntimeWeapon> runtime_Right_Weapons = new List<RuntimeWeapon>();
        List<RuntimeWeapon> runtime_Left_Weapons = new List<RuntimeWeapon>();
        List<RuntimeSpellItems> runtime_Spells = new List<RuntimeSpellItems>();
        List<RuntimeConsumableItem> runtime_Consumables = new List<RuntimeConsumableItem>();

        List<RuntimeConsumableItem> consumable_Indexes = new List<RuntimeConsumableItem>();
        List<RuntimeWeapon> leftHand_Indexes = new List<RuntimeWeapon>();
        List<RuntimeWeapon> rightHand_Indexes = new List<RuntimeWeapon>();
        
        public RuntimeSpellItems currentSpell;
        public RuntimeConsumableItem currentConsumable;
        RuntimeConsumableItem emptyItem;
        Action currentSlot;

        public RuntimeWeapon rightHandWeapon;
        public RuntimeWeapon leftHandWeapon;
        public GameObject parryCollider;
        public GameObject breathCollider;
        public GameObject blockCollider;

        StateManager states;
        QuickSlot quickSlotManager;
        GameObject referencesParent;

        public void Init(StateManager st)
        {
            states = st;
            quickSlotManager = QuickSlot.Instance;

            LoadLists();
            ClearReferences();
            LoadInventory();

            ParryCollider parryCol = parryCollider.GetComponent<ParryCollider>();
            parryCol.InitPlayer(st);
            CloseParryCollider();
            CloseBreathCollider();
            CloseBlockCollider();
        }

        void LoadLists(){

            rightHandWeapons.Clear();
            leftHandWeapons.Clear();
            consumableItems.Clear();
            spellItems.Clear();
            
            for (int i = 0; i < 3; i++)
            {
                //-1 --> Unarmed Weapon
                rightHandWeapons.Add(-1);
                leftHandWeapons.Add(-1);
            }

            for (int i = 0; i < 10; i++)
            {
                //-1 --> Empty item
                consumableItems.Add(-1);
            }

            SessionManager sessionManager = SessionManager.Instance;

            for (int i = 0; i < sessionManager._equipped_RightHand.Count; i++)
            {
                rightHandWeapons[i] = sessionManager._equipped_RightHand[i];//sessionManager.rightHand_Weapons_Equipped[i];
            }

            for (int i = 0; i < sessionManager._equipped_LeftHand.Count; i++)
            {
                leftHandWeapons[i] = sessionManager._equipped_LeftHand[i];
            }

            for (int i = 0; i < sessionManager._equipped_Consumables.Count; i++)
            {
                Debug.Log("Consumable: " + i +": " + sessionManager._equipped_Consumables[i]);
                consumableItems[i] = sessionManager._equipped_Consumables[i];
            }

            spellItems.AddRange(sessionManager.spells_Equipped);

        }
        
        public void ClearReferences() {

            if (runtime_Right_Weapons != null)
            {
                //Clear right hand weapons
                for (int i = 0; i < runtime_Right_Weapons.Count; i++)
                {
                    Destroy(runtime_Right_Weapons[i].weaponModel);
                }

                runtime_Right_Weapons.Clear();
            }

            if (runtime_Left_Weapons != null)
            {
                //Clear left hand weapons
                for (int i = 0; i < runtime_Left_Weapons.Count; i++)
                {
                    Destroy(runtime_Left_Weapons[i].weaponModel);
                }

                runtime_Left_Weapons.Clear();
            }

            leftHandWeapon = null;
            rightHandWeapon = null;
            
            if (runtime_Consumables != null)
            {
                //Clear consumables
                for (int i = 0; i < runtime_Consumables.Count; i++)
                {
                    if(runtime_Consumables[i] !=null)
                        if(runtime_Consumables[i].consumableModel)
                            Destroy(runtime_Consumables[i].consumableModel);
                }

                runtime_Consumables.Clear();
            }

            if (runtime_Spells != null)
            {
                for (int i = 0; i < runtime_Spells.Count; i++)
                {
                    if (runtime_Spells[i].currentParticle)
                    {
                        Destroy(runtime_Spells[i].currentParticle);
                    }
                }

                runtime_Spells.Clear();
            }

            if (referencesParent)
            {
                Destroy(referencesParent);
            }
            referencesParent = new GameObject("Run Time References");
        }

        public void LoadInventory(bool updateActions = false)
        {
            SessionManager sessionManger = SessionManager.Instance;

            unarmedRunTimeWeapon = WeaponToRuntimeWeapon(ResourcesManager.Instance.GetWeapon(unarmedId), false);
            unarmedRunTimeWeapon.isUnarmed = true;

            //3 --> number of equipment slots in inventory
            for (int i = 0; i < 3; i++)
            {
                runtime_Right_Weapons.Add(unarmedRunTimeWeapon);
                runtime_Left_Weapons.Add(unarmedRunTimeWeapon);
            }

            //There are 10 slots for consumables in inventory
            for (int i = 0; i < 10; i++)
            {
                runtime_Consumables.Add(emptyItem);
            }

            for (int i = 0; i < rightHandWeapons.Count; i++)
            {
                if (rightHandWeapons[i] == -1)
                {
                    runtime_Right_Weapons[i] = unarmedRunTimeWeapon;
                }
                else
                {
                    ItemInventoryInstance invInstance = sessionManger.GetWeaponItem(rightHandWeapons[i]);
                    RuntimeWeapon weapon = WeaponToRuntimeWeapon(ResourcesManager.Instance.GetWeapon(invInstance.itemId));
                    runtime_Right_Weapons[i] = weapon;
                }
            }

            for (int i = 0; i < leftHandWeapons.Count; i++)
            {
                if (leftHandWeapons[i] == -1)
                {
                    runtime_Left_Weapons[i] = unarmedRunTimeWeapon;
                }
                else
                {
                    ItemInventoryInstance invInstance = sessionManger.GetWeaponItem(leftHandWeapons[i]);
                    RuntimeWeapon weapon = WeaponToRuntimeWeapon(ResourcesManager.Instance.GetWeapon(invInstance.itemId), true);
                    runtime_Left_Weapons[i] = weapon;
                }

            }

            for (int i = 0; i < spellItems.Count; i++)
            {
                SpellToRuntimeSpell(ResourcesManager.Instance.GetSpell(spellItems[i]));
            }
            
            emptyItem = ConsumableToRunTime(ResourcesManager.Instance.GetConsumable("empty"));
            emptyItem.isEmpty = true;
            //Consumables
            for (int i = 0; i < consumableItems.Count; i++)
            {
                if (consumableItems[i] == -1)
                {
                    runtime_Consumables[i] = emptyItem;
                }
                else
                {
                    ItemInventoryInstance invInstance = sessionManger.GetConsumableItem(consumableItems[i]);
                    RuntimeConsumableItem runtimeConsumable = ConsumableToRunTime(ResourcesManager.Instance.GetConsumable(invInstance.itemId));
                    runtime_Consumables[i] = runtimeConsumable;
                }
            }

            InitAllDamageColliders(states);
            CloseAllDamageColliders();

            MakeIndexesList();
            EquipInventory();

            if (updateActions)
            {
                states.actionManager.UpdateActionsOneHanded();
            }
        }

        public void EquipInventory() {

            //Runtime Weapons

                if (right_Index > rightHand_Indexes.Count - 1)
                {
                    right_Index = 0;
                }
                rightHandWeapon = rightHand_Indexes[right_Index];
            
                if (left_Index > leftHand_Indexes.Count - 1)
                {
                    left_Index = 0;
                }
                leftHandWeapon = leftHand_Indexes[left_Index];

            //RightHand Weapons
            EquipWeapon(rightHandWeapon, false);
            
            //LeftHand weapons
            EquipWeapon(leftHandWeapon, true);
            //hasLeftHandWeapon = true;

            //Consumables
                if (consumable_Index > consumable_Indexes.Count - 1)
                {
                    consumable_Index = 0;
                }

                EquipConsumable(consumable_Indexes[consumable_Index]);
            

            //Spells
            if (runtime_Spells.Count > 0)
            {
                if (spell_Index > runtime_Spells.Count)
                {
                    spell_Index = 0;
                }

                EquipSpell(runtime_Spells[spell_Index]);
            }
            else
            {
                quickSlotManager.ClearSlot(QSlotType.spell);
            }

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

            Item i = ResourcesManager.Instance.GetItem(weapon.Instance.item_id, Itemtype.Weapon);
            
            quickSlotManager.UpdateSlot(
                (isLeft) ?
                QSlotType.leftHand : QSlotType.rightHand, i.itemIcon);

            weapon.weaponModel.SetActive(true);
        }

        public void EquipSpell(RuntimeSpellItems spell)
        {
            currentSpell = spell;

            Item i = ResourcesManager.Instance.GetItem(spell.Instance.item_id, Itemtype.Spell);

            quickSlotManager.UpdateSlot(QSlotType.spell, i.itemIcon);
        }

        public void EquipConsumable(RuntimeConsumableItem consumable)
        {
            currentConsumable = consumable;

            Item i = ResourcesManager.Instance.GetItem(consumable.Instance.item_id, Itemtype.Consumable);
            quickSlotManager.UpdateSlot(QSlotType.item, i.itemIcon);
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
            go.transform.parent = referencesParent.transform;

            RuntimeSpellItems instSpell = go.AddComponent<RuntimeSpellItems>();

            instSpell.Instance = new Spell();
            StaticFunctions.DeepCopySpell(spell, instSpell.Instance);
            go.name = spell.item_id;


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
            //Debug.Log("Converting to runtime weapon: " + weapon.item_id);
            GameObject go = new GameObject();
            go.transform.parent = referencesParent.transform;
            RuntimeWeapon inst = go.AddComponent<RuntimeWeapon>();
            go.name = weapon.item_id;

            inst.Instance = new Weapon();
            StaticFunctions.DeepCopyWeapon(weapon, inst.Instance);

            inst.weaponStats = new WeaponStats();
            WeaponStats weaponStats = ResourcesManager.Instance.GetWeaponStats(weapon.item_id);
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
                Debug.Log("Missing component 'WeaponHook' on weapon: " + weapon.item_id);
            }

            inst.weaponHook.InitDamageColliders(states);

            inst.weaponModel.SetActive(false);

            return inst;
        }

        public RuntimeConsumableItem ConsumableToRunTime(Consumable consumable) {
            GameObject go = new GameObject();
            go.transform.parent = referencesParent.transform;
            go.name = consumable.item_id;
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
                if (leftHand_Indexes.Count == 0)
                {
                    return;
                }

                if (left_Index < leftHand_Indexes.Count - 1)
                    left_Index++;
                else
                    left_Index = 0;

                EquipWeapon(leftHand_Indexes[left_Index], true);
            }
            else
            {
                //There are no weapons in right hand to change
                if (rightHand_Indexes.Count == 0)
                {
                    return;
                }

                if (right_Index < rightHand_Indexes.Count - 1)
                    right_Index++;
                else
                    right_Index = 0;

                EquipWeapon(rightHand_Indexes[right_Index]);
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
            if (consumable_Index < consumable_Indexes.Count - 1)
            {
                consumable_Index++;
            }
            else
            {
                consumable_Index = 0;
            }

            EquipConsumable(consumable_Indexes[consumable_Index]);
        }

        void MakeIndexesList() {

            //Consumables
            consumable_Indexes.Clear();

            for (int i = 0; i < runtime_Consumables.Count; i++)
            {
                if (runtime_Consumables[i].isEmpty)
                {
                    continue;
                }

                //If item is not empty, add index
                consumable_Indexes.Add(runtime_Consumables[i]);
            }

            //1 item + 1 empty
            if (consumable_Indexes.Count < 2 )
            {
                consumable_Indexes.Add(emptyItem);
            }

            //Right Weapons
            for (int i = 0; i < runtime_Right_Weapons.Count; i++)
            {
                if (runtime_Right_Weapons[i].isUnarmed)
                {
                    continue;
                }

                //If item is not empty, add index
                rightHand_Indexes.Add(runtime_Right_Weapons[i]);
            }

            //1 item + 1 empty
            if (rightHand_Indexes.Count < 2)
            {
                rightHand_Indexes.Add(unarmedRunTimeWeapon);
            }

            //Left Weapons
            for (int i = 0; i < runtime_Left_Weapons.Count; i++)
            {
                if (runtime_Left_Weapons[i].isUnarmed)
                {
                    continue;
                }

                //If item is not empty, add index
                leftHand_Indexes.Add(runtime_Left_Weapons[i]);
            }

            //1 item + 1 empty
            if (leftHand_Indexes.Count < 2)
            {
                leftHand_Indexes.Add(unarmedRunTimeWeapon);
            }

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
        public string item_id;
        public string name_item;
        public string itemDescription;
        public string skillDescription;
        public Sprite itemIcon;
    }

    [System.Serializable]
    public class Weapon
    {
        public string item_id;
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
    public class Spell 
    {
        public string item_id;
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
    public class Consumable
    {
        public string item_id;
        public string consumableEffect;
        public string targetAnim;
        public GameObject consumablePrefab;

        public Vector3 model_pos;
        public Vector3 model_eulerRot;
        public Vector3 model_scale;
    }
    
}