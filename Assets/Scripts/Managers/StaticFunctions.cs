using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public static class StaticFunctions
    {

        public static void DeepCopyWeapon(Weapon from, Weapon to) {

            to.itemIcon = from.itemIcon;
            to.oh_idle = from.oh_idle;
            to.th_idle = from.th_idle;
            to.actions = new List<Action>();
            //Copy one handed actions
            for (int i = 0; i < from.actions.Count; i++)
            {
                Action oneHandedAction = new Action();
                oneHandedAction.weaponStats = new WeaponStats();
                DeepCopyActionToAction(oneHandedAction, from.actions[i]);
                to.actions.Add(oneHandedAction);
            }

            to.twoHandedActions = new List<Action>();
            //Copy two handed actions
            for (int i = 0; i < from.twoHandedActions.Count; i++)
            {
                Action twoHandedAction = new Action();
                twoHandedAction.weaponStats = new WeaponStats();
                DeepCopyActionToAction(twoHandedAction, from.twoHandedActions[i]);
                to.twoHandedActions.Add(twoHandedAction);
            }

            to.parryMultiplier = from.parryMultiplier;
            to.backstabMultiplier = from.backstabMultiplier;
            to.LeftHandMirror = from.LeftHandMirror;
            to.weaponModelPrefab = from.weaponModelPrefab;

            to.left_model_pos = from.left_model_pos;
            to.left_model_eulerRot = from.left_model_eulerRot;
            to.left_model_scale = from.left_model_scale;

            to.right_model_pos = from.right_model_pos;
            to.right_model_eulerRot = from.right_model_eulerRot;
            to.right_model_scale = from.right_model_scale;

        }


        public static void DeepCopyActionToAction(Action action, Action weaponAct) {

            action.targetAnim = weaponAct.targetAnim;
            action.type = weaponAct.type;
            action.canBeParried = weaponAct.canBeParried;
            action.chageSpeed = weaponAct.chageSpeed;
            action.animSpeed = weaponAct.animSpeed;
            action.canBackStab = weaponAct.canBackStab;
            action.overrideDamageAnimation = weaponAct.overrideDamageAnimation;
            action.damageAnim = weaponAct.damageAnim;

            DeepCopyWeaponStats(weaponAct.weaponStats, action.weaponStats);
        }

        public static void DeepCopyAction(Weapon weapon, ActionInput actionInput, ActionInput assign, List<Action> actionList,bool isLeftHand = false)
        {

            Action action = GetAction(assign, actionList);
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
            action.parryMultiplier = weapon.parryMultiplier;
            action.backstabMultiplier = weapon.backstabMultiplier;

            if (isLeftHand)
            {
                //Left hand animations are just mirror version of right hand animations
                action.mirror = true;
            }

            DeepCopyWeaponStats(weaponAct.weaponStats, action.weaponStats);
        }

        public static void DeepCopyWeaponStats(WeaponStats weaponStats_From, WeaponStats weaponStats_To)
        {

            weaponStats_To.physicalDamage = weaponStats_From.physicalDamage;
            weaponStats_To.strikeDamage = weaponStats_From.strikeDamage;
            weaponStats_To.thrustDamage = weaponStats_From.thrustDamage;
            weaponStats_To.slashDamage = weaponStats_From.slashDamage;

            weaponStats_To.magicDamage = weaponStats_From.magicDamage;
            weaponStats_To.lightningDamage = weaponStats_From.lightningDamage;
            weaponStats_To.fireDamage = weaponStats_From.fireDamage;
            weaponStats_To.darkDamage = weaponStats_From.darkDamage;

        }

        public static Action GetAction(ActionInput actInput, List<Action> actionSlots)
        {

            for (int i = 0; i < actionSlots.Count; i++)
            {
                if (actionSlots[i].input == actInput)
                {
                    return actionSlots[i];
                }
            }

            return null;
        }

    }
}