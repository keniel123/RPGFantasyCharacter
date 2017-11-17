using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    [Serializable]
    public class CharacterStats
    {
        [Header("Current")]
        public float currentHealth;
        public float currentMana;
        public float currentStamina;

        [Header("Base Power")]
        public int hp = 100;    //Life
        public int fp = 100;    //Focus
        public int stamina = 100;
        public float equipLoad = 20;
        public float poise = 20;
        public int itemDiscover = 111;

        [Header("Attack Power")]
        public int rightWeapon_1 = 51;
        public int rightWeapon_2 = 51;
        public int rightWeapon_3 = 51;

        public int leftWeapon_1 = 51;
        public int leftWeapon_2 = 51;
        public int leftWeapon_3 = 51;

        [Header("Defense")]
        public int physical = 87;
        public int vs_strike = 87;
        public int vs_slash = 87;
        public int vs_thrust = 87;

        public int magic = 30;
        public int lightning = 30;
        public int fire = 30;
        public int dark = 30;

        [Header("Resistances")]
        public int bleed = 100;
        public int poison = 100;
        public int frost = 100;
        public int curse = 100;

        public int attunementSlots = 0;

        public void InitCurrent() {
            
            if (statEffects != null)
            {
                statEffects();
            }

            currentHealth = hp;
            currentMana = fp;
            currentStamina = stamina;

        }

        public delegate void StatEffects();
        public StatEffects statEffects;

        public void AddHealth() {
            hp += 5;
        }

        public void RemoveHealth()
        {
            hp -= 5;
        }
    }


    //Attributes specific to player's character, enemies don't have these attributes
    [Serializable]
    public class Attributes
    {
        public int level = 1;
        public int souls = 0;
        public int vigor = 11;
        public int attunement = 11;
        public int endurance = 11;
        public int vitality = 11;
        public int strength = 11;
        public int dexterity = 11;
        public int intelligence = 11;
        public int faith = 11;
        public int luck = 11;
    }


    [Serializable]
    public class WeaponStats {
        public int physicalDamage;
        public int strikeDamage;
        public int slashDamage;
        public int thrustDamage;
        public int magicDamage = 0;
        public int fireDamage = 0;
        public int lightningDamage = 0;
        public int darkDamage = 0;


    }
}