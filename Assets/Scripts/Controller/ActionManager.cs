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

            if (states.inventoryManager.hasLeftHandWeapon)
            {
                UpdateActionsWithLeftHand();
                return;
            }

            Weapon weapon = states.inventoryManager.rightHandWeapon;

            //For each action of the weapon, assign the target animation
            for (int i = 0; i < weapon.actions.Count; i++)
            {
                Action action = GetAction(weapon.actions[i].input);
                action.targetAnim = weapon.actions[i].targetAnim;
            }
        }

        public void UpdateActionsWithLeftHand() {
            Weapon rightWeapon = states.inventoryManager.rightHandWeapon;
            Weapon leftWeapon = states.inventoryManager.leftHandWeapon;

            Action rb = GetAction(ActionInput.rb);
            Action rt = GetAction(ActionInput.rt);

            Action weapon_RB = rightWeapon.GetAction(rightWeapon.actions, ActionInput.rb);
            rb.targetAnim = weapon_RB.targetAnim;
            rb.type = weapon_RB.type;
            rb.canBeParried = weapon_RB.canBeParried;
            rb.chageSpeed = weapon_RB.chageSpeed;
            rb.animSpeed = weapon_RB.animSpeed;

            Action weapon_RT = rightWeapon.GetAction(rightWeapon.actions, ActionInput.rt);
            rt.targetAnim = weapon_RT.targetAnim;
            rt.type = weapon_RT.type;
            rt.canBeParried = weapon_RT.canBeParried;
            rt.chageSpeed = weapon_RT.chageSpeed;
            rt.animSpeed = weapon_RT.animSpeed;

            Action lb = GetAction(ActionInput.lb);
            Action lt = GetAction(ActionInput.lt);

            Action weapon_LB = leftWeapon.GetAction(leftWeapon.actions, ActionInput.rb);
            lb.targetAnim = weapon_LB.targetAnim;
            lb.type = weapon_LB.type;
            lb.canBeParried = weapon_LB.canBeParried;
            lb.chageSpeed = weapon_LB.chageSpeed;
            lb.animSpeed = weapon_LB.animSpeed;

            Action weapon_LT = leftWeapon.GetAction(leftWeapon.actions, ActionInput.rt);
            lt.targetAnim = weapon_LT.targetAnim;
            lt.type = weapon_LT.type;
            lt.canBeParried = weapon_LT.canBeParried;
            lt.chageSpeed = weapon_LT.chageSpeed;
            lt.animSpeed = weapon_LT.animSpeed;

            if (leftWeapon.LeftHandMirror)
            {
                //Left hand animations are just mirror version of right hand animations
                lb.mirror = true;
                lt.mirror = true;
            }
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
    }

    [Serializable]
    public class ItemAction
    {
        public string targetAnim;
        public string itemID;
    }
}