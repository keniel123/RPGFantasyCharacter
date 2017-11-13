using RPGController.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class InventoryManager : MonoBehaviour
    {
        public Weapon rightHandWeapon;
        public bool hasLeftHandWeapon = true;
        public Weapon leftHandWeapon;
        public GameObject parryCollider;
        StateManager states;

        public void Init(StateManager st)
        {
            states = st;
            EquipWeapon(rightHandWeapon, false);
            EquipWeapon(leftHandWeapon, true);
            InitAllDamageColliders(st);
            CloseAllDamageColliders();

            ParryCollider parryCol = parryCollider.GetComponent<ParryCollider>();
            parryCol.InitPlayer(st);
            CloseParryCollider();
        }

        public void EquipWeapon(Weapon weapon, bool isLeft = false) {
            string targetIdle = weapon.oh_idle;
            targetIdle += (isLeft) ? "_left" : "_right";
            states.animator.SetBool(StaticStrings.animParam_Mirror, isLeft);
            states.animator.Play("changeWeapon");
            states.animator.Play(targetIdle);

            QuickSlot quickSlot = QuickSlot.Instance;
            quickSlot.UpdateSlot(
                (isLeft) ?
                QSlotType.leftHand : QSlotType.rightHand, weapon.weaponIcon);

            if (isLeft)
            {

            }
        }

        //Gets the current weapon equipped by the player
        public Weapon GetCurrentWeapon(bool isLeft) {
            if (isLeft)
            {
                return leftHandWeapon;
            }
            else
            {
                return rightHandWeapon;
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
    }

    [System.Serializable]
    public class Weapon
    {
        public string weaponID;
        public Sprite weaponIcon;
        public string oh_idle;  //One handed idle animation name
        public string th_idle;  //Two handed idle animation name

        public List<Action> actions;
        public List<Action> twoHandedActions;
        //For different weapons, parry and backstab can have different effects
        public WeaponStats parryStats;
        public WeaponStats backstabStats;

        public bool LeftHandMirror;
        public GameObject weaponModel;
        public WeaponHook weaponHook;

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