using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace RPGController
{
    public static class StaticStrings
    {
        //Inputs
        public static string Input_Vertical = "Vertical";
        public static string Input_Horizontal = "Horizontal";
        public static string B = "B";
        public static string A = "A";
        public static string Y = "Y";
        public static string X = "X";
        public static string RT = "RT";
        public static string LT = "LT";
        public static string RB = "RB";
        public static string Pad_X = "Pad_X";
        public static string Pad_Y = "Pad_Y";

        public static string LB = "LB";
        public static string Lock = "Lock";

        //Animator Parameters
        public static string animParam_Vertical = "Vertical";
        public static string animParam_Horizontal = "Horizontal";
        public static string animParam_IsTwoHanded = "IsTwoHanded";
        public static string animParam_CanMove = "CanMove";
        public static string animParam_Interacting = "Interacting";
        public static string animParam_LockOn = "Lock On";
        public static string animParam_OnGround = "OnGround";
        public static string animParam_Run = "Run";
        public static string animParam_Mirror = "Mirror";
        public static string animParam_Block = "Block";
        public static string animParam_IsLeft = "IsLeft";
        public static string animParam_AnimSpeed = "AnimSpeed";
        public static string animParam_ParryAttack = "parry_attack";
        public static string animParam_SpellCasting = "SpellCasting";


        //Animator States
        public static string animState_Rolls = "Rolls";
        public static string animState_AttackInterrupt = "attack_interrupt";
        public static string animState_ParryReceived = "parry_received";
        public static string animState_BackStabbed = "getting_backstabbed";
        public static string animState_ChangeWeapon = "changeWeapon";
        public static string animState_EmptyBoth = "Empty Both Hands";
        public static string animState_EmptyLeft = "Empty Left Hand";
        public static string animState_EmptyRight = "Empty Right Hand";
        public static string animState_EquipWeapon_OH = "equipWeapon_oh";

        public static string damage1 = "damage_1";
        public static string damage2 = "damage_2";
        public static string damage3 = "damage_3";

        //Other
        public static string _leftPrefix = "_left";
        public static string _rightPrefix = "_right";

        //ScriptableObjects
        public static string WeaponScriptableObject_FileName = "RPGController.WeaponScriptableObject";
        public static string SpellScriptableObject_FileName = "RPGController.SpellItemScriptableObject";

        //XML Datapath

        public static string itemFolder = "/Items/";

        public static string SaveLocation()
        {
            string path = Application.streamingAssetsPath;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }
    }
}