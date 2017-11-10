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
            Weapon weapon = states.inventoryManager.currentWeapon;

            //For each action of the weapon, assign the target animation
            for (int i = 0; i < weapon.actions.Count; i++)
            {
                Action action = GetAction(weapon.actions[i].input);
                action.targetAnim = weapon.actions[i].targetAnim;
            }
        }

        public void UpdateActionsTwoHanded()
        {
            EmptyAllSlots();
            Weapon weapon = states.inventoryManager.currentWeapon;

            //For each action of the weapon, assign the target animation
            for (int i = 0; i < weapon.twoHandedActions.Count; i++)
            {
                Action action = GetAction(weapon.twoHandedActions[i].input);
                action.targetAnim = weapon.twoHandedActions[i].targetAnim;
            }
        }

        void EmptyAllSlots() {
            //Get previous actions
            for (int i = 0; i < 4; i++)
            {
                Action a = GetAction((ActionInput)i);
                a.targetAnim = null;
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

    [Serializable]
    public class Action {

        public ActionInput input;
        public string targetAnim;
    }

    [Serializable]
    public class ItemAction
    {
        public string targetAnim;
        public string itemID;
    }
}