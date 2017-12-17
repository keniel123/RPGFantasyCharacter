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
        public int currentSouls;

        public float healthRecoverValue = 60;
        public float manaRecoverValue = 60;
      
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
    
    public enum AttributeType
    {
        Level,
        Vigor,
        Attunement,
        Vitality,
        Endurance,
        Strength,
        Dexterity,
        Intelligence,
        Faith,
        Luck,
        Health,
        Mana,
        Stamina,
        Equip_Load,
        Poise,
        Item_Discovery,
        Attunument_Slots
    }

    public enum AttackDefenseType {
        Physical, Magic, Fire, Lightning, Dark, Critical, Stability, Bleed, Curse, Frost, MagicBuff,
        Strike, Slash, Thrust, Poison
    }

    public enum WeaponDamage {
        Sum, VS_Strike, VS_Slash, VS_Thrust
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

        public string weaponID; //Refers to the weapon's Item id
        public int attackPhysical;
        public int attackStrike;
        public int attackSlash;
        public int attackThrust;
        public int attackMagic = 0;
        public int attackFire = 0;
        public int attackLigthning = 0;
        public int attackDark = 0;
        public int attackFrost;
        public int attackCurse;
        public int attackPoison;
        public int attackCritical;

        public float defensePhysical;
        public float defenseStrike;
        public float defenseThrust;
        public float defenseMagic;
        public float defenseFire;
        public float defenseLigthning;
        public float defenseDark;
        public float defenseFrost;
        public float defenseCurse;
        public float defensePoison;
        public float defenseStability;


        public string weaponType;
        public string damageType;
        public string skillName;
        public float weightCost = 5;
        public float maxDurability = 100; 
    }
}