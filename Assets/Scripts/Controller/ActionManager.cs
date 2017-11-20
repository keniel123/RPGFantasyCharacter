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
                action.actionSteps = weapon.twoHandedActions[i].actionSteps;
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
                    a.actionSteps = null;
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

        public Action GetActionFromInput(ActionInput actInput) {
            return StaticFunctions.GetAction(actInput, actionSlots);
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
        public string defaultTargetAnim;

        public List<ActionSteps> actionSteps;

        public bool mirror = false;
        public bool canBeParried = true;
        public bool chageSpeed = false;
        public float animSpeed = 1;
        public bool canParry = false;
        public bool canBackStab = false;
        public float staminaCost = 5;
        public float manaCost;

        ActionSteps defaultStep;

        public ActionSteps GetActionSteps(ref int index) {

            //If the weapon has no defined action steps, create a branch with default target animation clip
            if (actionSteps == null || actionSteps.Count == 0)
            {
                if (defaultStep == null)
                {
                    defaultStep = new ActionSteps();
                    defaultStep.animationBranches = new List<ActionAnimation>();

                    ActionAnimation actAnim = new ActionAnimation();
                    actAnim.input = input;
                    actAnim.targetAnim = defaultTargetAnim;
                    defaultStep.animationBranches.Add(actAnim);
                }

                return defaultStep;

            }

            if (index > actionSteps.Count - 1)
            {
                index = 0;
            }

            ActionSteps retVal = actionSteps[index];

            if (index > actionSteps.Count - 1)
            {
                index = 0;
            }
            else
            {
                index++;
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
    public class ActionSteps {

        public List<ActionAnimation> animationBranches = new List<ActionAnimation>();

        public ActionAnimation GetBranch(ActionInput input) {
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
    public class ActionAnimation{
        public ActionInput input;
        public string targetAnim;
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