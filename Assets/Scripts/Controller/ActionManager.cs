using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class ActionManager : MonoBehaviour
    {
        public int actionStepIndex;
        public List<Action> actionSlots = new List<Action>();

        StateManager states;

        public void Init(StateManager st)
        {
            states = st;

            UpdateActionsOneHanded();

        }

        public void UpdateActionsOneHanded()
        {
            EmptyAllSlots();

            if (states.inventoryManager.rightHandWeapon !=null)
            {

            StaticFunctions.DeepCopyAction(states.inventoryManager.rightHandWeapon.Instance, ActionInput.rb, ActionInput.rb, actionSlots);
            StaticFunctions.DeepCopyAction(states.inventoryManager.rightHandWeapon.Instance, ActionInput.rt, ActionInput.rt, actionSlots);
            }

            if (states.inventoryManager.leftHandWeapon == null)
            {
                return;
            }
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

            if (states.inventoryManager.rightHandWeapon == null)
            {
                return;
            }

            Weapon weapon = states.inventoryManager.rightHandWeapon.Instance;

            //For each action of the weapon, assign the target animation
            for (int i = 0; i < weapon.twoHandedActions.Count; i++)
            {
                Action action = StaticFunctions.GetAction(weapon.twoHandedActions[i].GetFirstInput(), actionSlots);
                action.firstStep.targetAnim = weapon.twoHandedActions[i].firstStep.targetAnim;
                StaticFunctions.DeepCopyStepsList(weapon.twoHandedActions[i], action);
                action.actionType = weapon.twoHandedActions[i].actionType;

            }
        }

        void EmptyAllSlots()
        {
            //Reset previous actions
            for (int i = 0; i < 4; i++)
            {
                Action a = StaticFunctions.GetAction((ActionInput)i, actionSlots);

                if (a == null)
                {
                    return;
                }

                //a.firstStep = null;
                a.comboSteps = null;
                a.mirror = false;
                a.actionType = ActionType.attack;
            }

            //Right hand unarmed actions
            StaticFunctions.DeepCopyAction(states.inventoryManager.unarmedRunTimeWeapon.Instance, ActionInput.rb, ActionInput.rb, actionSlots);
            StaticFunctions.DeepCopyAction(states.inventoryManager.unarmedRunTimeWeapon.Instance, ActionInput.rt, ActionInput.rt, actionSlots);

            //Left hand unarmed actions
            StaticFunctions.DeepCopyAction(states.inventoryManager.unarmedRunTimeWeapon.Instance, ActionInput.rb, ActionInput.lb, actionSlots, true);
            StaticFunctions.DeepCopyAction(states.inventoryManager.unarmedRunTimeWeapon.Instance, ActionInput.rt, ActionInput.lt, actionSlots, true);

        }

        public Action GetActionSlot(StateManager st)
        {
            //Find the action input
            ActionInput aInput = GetActionInput(st);
            return StaticFunctions.GetAction(aInput, actionSlots);
        }

        public Action GetActionFromInput(ActionInput actInput)
        {
            return StaticFunctions.GetAction(actInput, actionSlots);
        }

        public ActionInput GetActionInput(StateManager st)
        {
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

        public bool IsLeftHandSlot(Action slot)
        {
            return (slot.GetFirstInput() == ActionInput.lb || slot.GetFirstInput() == ActionInput.lt);
        }
    }

    public enum ActionInput
    {
        rb, rt, lb, lt, none
    }

    public enum ActionType
    {
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
    public class Action
    {
        public ActionType actionType;
        public SpellClass spellClass;
        public ActionAnimation firstStep;
        public List<ActionAnimation> comboSteps;
        public bool mirror = false;
        public bool canBeParried = true;
        public bool chageSpeed = false;
        public float animSpeed = 1;
        public bool canParry = false;
        public bool canBackStab = false;
        public float staminaCost = 5;
        public float manaCost;

        public bool overrideKick;
        public string kickAnim;

        public ActionInput GetFirstInput()
        {
            if (firstStep == null)
            {
                firstStep = new ActionAnimation();
            }

            return firstStep.input;
        }

        public ActionAnimation GetActionSteps(ref int index)
        {
            if (index == 0)
            {
                if (comboSteps.Count == 0)
                {
                    index = 0;
                }
                else
                {
                    //Even if you dont have combo steps
                    index++;
                }
                return firstStep;
            }

            ActionAnimation retVal = comboSteps[index-1];
            index++;
            if (index > comboSteps.Count - 1)
            {
                index = 0;
            }

            return retVal;
        }

        [HideInInspector]
        public float parryMultiplier;
        [HideInInspector]
        public float backstabMultiplier;

        public bool overrideDamageAnimation;
        public string damageAnim;
    }

    [Serializable]
    public class ActionSteps
    {

        public List<ActionAnimation> animationBranches = new List<ActionAnimation>();

        public ActionAnimation GetBranch(ActionInput input)
        {
            for (int i = 0; i < animationBranches.Count; i++)
            {
                if (animationBranches[i].input == input)
                {
                    return animationBranches[i];
                }
            }

            return animationBranches[0];
        }
    }

    [Serializable]
    public class ActionAnimation
    {
        public ActionInput input;
        public string targetAnim;
    }
    [Serializable]
    public class SpellAction
    {
        public ActionInput actInput;
        public string targetAnim;
        public string throwAnim;
        public float castTime;
        public float manaCost;
        public float staminaCost;
    }

}