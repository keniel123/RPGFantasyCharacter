using RPGController.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class InventoryManager : MonoBehaviour
    {
        //Use these weapon name lists to create RuntimeWeapon items
        public List<string> rightHandWeapons;
        public List<string> leftHandWeapons;

        public RuntimeWeapon rightHandWeapon;
        public bool hasLeftHandWeapon = true;
        public RuntimeWeapon leftHandWeapon;
        public GameObject parryCollider;
        StateManager states;

        public void Init(StateManager st)
        {
            states = st;

            if (rightHandWeapons.Count > 0)
            {
                //Get the first weapon from Resources with the weapon that corresponds
                //the first name in right weapon names list
                rightHandWeapon = WeaponToRuntimeWeapon(
                    ResourcesManager.Instance.GetWeapon(rightHandWeapons[0])
                );
            }

            if (leftHandWeapons.Count > 0)
            {
                rightHandWeapon = WeaponToRuntimeWeapon(
                    ResourcesManager.Instance.GetWeapon(leftHandWeapons[0]), true);

                hasLeftHandWeapon = true;
            }
        

            if (rightHandWeapon != null)
            {
                EquipWeapon(rightHandWeapon, false);
            }
            if (leftHandWeapon != null)
            {
                EquipWeapon(leftHandWeapon, true);
            }

            hasLeftHandWeapon = (leftHandWeapon != null);

            InitAllDamageColliders(st);
            CloseAllDamageColliders();

            ParryCollider parryCol = parryCollider.GetComponent<ParryCollider>();
            parryCol.InitPlayer(st);
            CloseParryCollider();
        }

        public void EquipWeapon(RuntimeWeapon weapon, bool isLeft = false) {
            
            string targetIdle = weapon.Instance.oh_idle;
            targetIdle += (isLeft) ? "_left" : "_right";
            states.animator.SetBool(StaticStrings.animParam_Mirror, isLeft);
            states.animator.Play(StaticStrings.animState_ChangeWeapon);
            states.animator.Play(targetIdle);

            QuickSlot quickSlot = QuickSlot.Instance;
            quickSlot.UpdateSlot(
                (isLeft) ?
                QSlotType.leftHand : QSlotType.rightHand, weapon.Instance.itemIcon);

            weapon.weaponModel.SetActive(true);
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

        public RuntimeWeapon WeaponToRuntimeWeapon(Weapon weapon, bool isLeftHand = false) {

            GameObject go = new GameObject();
            RuntimeWeapon inst = go.AddComponent<RuntimeWeapon>();

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

            return inst;
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


        public Action GetAction(List<Action> listOfActions, ActionInput actInput) {
            
            for (int i = 0; i < actions.Count; i++)
            {
                if (listOfActions[i].input == actInput)
                {
                    return listOfActions[i];
                }
            }

            return null;
        }
    }
}