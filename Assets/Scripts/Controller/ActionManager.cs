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

            StaticFunctions.DeepCopyAction(states.inventoryManager.rightHandWeapon.Instance, ActionInput.rb, ActionInput.rb, actionSlots);
            StaticFunctions.DeepCopyAction(states.inventoryManager.rightHandWeapon.Instance, ActionInput.rt, ActionInput.rt, actionSlots);

            if (states.inventoryManager.hasLeftHandWeapon)
            {
                StaticFunctions.DeepCopyAction(states.inventoryManager.leftHandWeapon.Instance, ActionInput.rb, ActionInput.lb, actionSlots, true);
                StaticFunctions.DeepCopyAction(states.inventoryManager.leftHandWeapon.Instance, ActionInput.rt, ActionInput.lt, actionSlots, true);
            }
            else
            {
                StaticFunctions.DeepCopyAction(states.inventoryManager.rightHandWeapon.Instance, ActionInput.lb, ActionInput.lb, actionSlots);
                StaticFunctions.DeepCopyAction(states.inventoryManager.rightHandWeapon.Instance, ActionInput.lt, ActionInput.lt, actionSlots);
            }
            
        }

        public void UpdateActionsTwoHanded()
        {
            EmptyAllSlots();
            Weapon weapon = states.inventoryManager.rightHandWeapon.Instance;

            //For each action of the weapon, assign the target animation
            for (int i = 0; i < weapon.twoHandedActions.Count; i++)
            {
                Action action = StaticFunctions.GetAction(weapon.twoHandedActions[i].input, actionSlots);
                action.targetAnim = weapon.twoHandedActions[i].targetAnim;
                action.actionType = weapon.twoHandedActions[i].actionType;
            }
        }

        void EmptyAllSlots() {
            //Reset previous actions
            for (int i = 0; i < 4; i++)
            {
                Action a = StaticFunctions.GetAction((ActionInput)i, actionSlots);

                if (a != null)
                {
                    a.targetAnim = null;
                    a.mirror = false;
                    a.actionType = ActionType.attack;
                }
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
            return StaticFunctions.GetAction(aInput, actionSlots);
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

            return ActionInput.none;
        }

        public bool IsLeftHandSlot(Action slot) {
            return (slot.input == ActionInput.lb || slot.input == ActionInput.lt);

        }
    }

    public enum ActionInput{
       rb, rt,lb,lt, none
    }

    public enum ActionType {
        attack, block, spells, parry
    }

    public enum SpellClass
    {
        Pyromancy, Miracles, Sorcery
    }

    public enum SpellType
    {
        Projectile, Buff, Looping
    }

    [Serializable]
    public class Action {

        public ActionInput input;
        public ActionType actionType;
        public SpellClass spellClass;
        public string targetAnim;
        public bool mirror = false;
        public bool canBeParried = true;
        public bool chageSpeed = false;
        public float animSpeed = 1;
        public bool canParry = false;
        public bool canBackStab = false;

        [HideInInspector]
        public float parryMultiplier;
        [HideInInspector]
        public float backstabMultiplier;

        public bool overrideDamageAnimation;
        public string damageAnim;

        public WeaponStats weaponStats;

    }

    [Serializable]
    public class SpellAction {
        public ActionInput actInput;
        public string targetAnim;
        public string throwAnim;
        public float castTime;
    }

    [Serializable]
    public class ItemAction
    {
        public string targetAnim;
        public string itemID;
    }
}