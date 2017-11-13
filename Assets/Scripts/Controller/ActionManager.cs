using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class ActionManager : MonoBehaviour
    {
        public List<Action> actionSlots = new List<Action>();
        public ItemAction consumableItem;

        StateManager states;

        public void Init(StateManager st)
        {
            states = st;

            UpdateActionsOneHanded();

        }

        public void UpdateActionsOneHanded()
        {
            EmptyAllSlots();

            DeepCopyAction(states.inventoryManager.rightHandWeapon, ActionInput.rb, ActionInput.rb);
            DeepCopyAction(states.inventoryManager.rightHandWeapon, ActionInput.rt, ActionInput.rt);

            if (states.inventoryManager.hasLeftHandWeapon)
            {
                DeepCopyAction(states.inventoryManager.leftHandWeapon, ActionInput.rb, ActionInput.lb, true);
                DeepCopyAction(states.inventoryManager.leftHandWeapon, ActionInput.rt, ActionInput.lt, true);
            }
            else
            {
                DeepCopyAction(states.inventoryManager.rightHandWeapon, ActionInput.lb, ActionInput.lb);
                DeepCopyAction(states.inventoryManager.rightHandWeapon, ActionInput.lt, ActionInput.lt);
            }

        }

        public void DeepCopyAction(Weapon weapon, ActionInput actionInput, ActionInput assign, bool isLeftHand = false) {

            Action action = GetAction(assign);
            Action weaponAct = weapon.GetAction(weapon.actions, actionInput);

            //If the weapon has no action, skip
            if (weaponAct == null)
            {
                return;
            }

            action.targetAnim = weaponAct.targetAnim;
            action.type = weaponAct.type;
            action.canBeParried = weaponAct.canBeParried;
            action.chageSpeed = weaponAct.chageSpeed;
            action.animSpeed = weaponAct.animSpeed;
            action.canBackStab = weaponAct.canBackStab;
            action.overrideDamageAnimation = weaponAct.overrideDamageAnimation;
            action.damageAnim = weaponAct.damageAnim;

            if (isLeftHand )
            {
                //Left hand animations are just mirror version of right hand animations
                action.mirror = true;
            }
            
            DeepCopyWeaponStats(weaponAct.weaponStats, action.weaponStats);
        }

        public void DeepCopyWeaponStats(WeaponStats weaponStats_From, WeaponStats weaponStats_To) {

            weaponStats_To.physicalDamage = weaponStats_From.physicalDamage;
            weaponStats_To.strikeDamage = weaponStats_From.strikeDamage;
            weaponStats_To.thrustDamage = weaponStats_From.thrustDamage;
            weaponStats_To.slashDamage = weaponStats_From.slashDamage;

            weaponStats_To.magicDamage = weaponStats_From.magicDamage;
            weaponStats_To.lightningDamage = weaponStats_From.lightningDamage;
            weaponStats_To.fireDamage = weaponStats_From.fireDamage;
            weaponStats_To.darkDamage = weaponStats_From.darkDamage;

        }

        public void UpdateActionsTwoHanded()
        {
            EmptyAllSlots();
            Weapon weapon = states.inventoryManager.rightHandWeapon;

            //For each action of the weapon, assign the target animation
            for (int i = 0; i < weapon.twoHandedActions.Count; i++)
            {
                Action action = GetAction(weapon.twoHandedActions[i].input);
                action.targetAnim = weapon.twoHandedActions[i].targetAnim;
                action.type = weapon.twoHandedActions[i].type;
            }
        }

        void EmptyAllSlots() {
            //Get previous actions
            for (int i = 0; i < 4; i++)
            {
                Action a = GetAction((ActionInput)i);
                a.targetAnim = null;
                a.mirror = false;
                a.type = ActionType.attack;
            }
        }

        //Constructor
        ActionManager() {

            //If there are no assigned actions slots, create them
            for (int i = 0; i < 4; i++)
            {
                Action act = new Action();
                act.input = (ActionInput)i;
                actionSlots.Add(act);
            }
        }

        public Action GetActionSlot(StateManager st) {
            //Find the action input
            ActionInput aInput = GetActionInput(st);
            return GetAction(aInput);
        }

        Action GetAction(ActionInput actInput) {

            for (int i = 0; i < actionSlots.Count; i++)
            {
                if (actionSlots[i].input == actInput)
                {
                    return actionSlots[i];
                }
            }

            return null;
        }

        public ActionInput GetActionInput(StateManager st) {
            if (st.rb)
                return ActionInput.rb;
            if (st.rt)
                return ActionInput.rt;
            if (st.lt)
                return ActionInput.lt;
            if (st.lb)
                return ActionInput.lb;

            return ActionInput.rb;
        }

        public bool IsLeftHandSlot(Action slot) {
            return (slot.input == ActionInput.lb || slot.input == ActionInput.lt);

        }
    }

    public enum ActionInput{
       rb, rt,lb,lt
    }

    public enum ActionType {
        attack, block, spells, parry
    }

    [Serializable]
    public class Action {

        public ActionInput input;
        public ActionType type;
        public string targetAnim;
        public bool mirror = false;
        public bool canBeParried = true;
        public bool chageSpeed = false;
        public float animSpeed = 1;
        public bool canBackStab = false;

        public bool overrideDamageAnimation;
        public string damageAnim;

        public WeaponStats weaponStats;

    }

    [Serializable]
    public class ItemAction
    {
        public string targetAnim;
        public string itemID;
    }
}