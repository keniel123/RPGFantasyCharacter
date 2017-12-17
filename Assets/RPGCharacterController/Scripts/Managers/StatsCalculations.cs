using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public static class StatsCalculations
    {
        //Calculates damage for base weapon (which has no additional abilities like poison, magic etc.)
        public static int CalculateBaseDamage(WeaponStats weaponStats, CharacterStats charStats, float multiplier = 1) {

            //Extract the resistance from weapon's standalone damage
            float physical = (weaponStats.attackPhysical * multiplier) - charStats.physical;
            float strike = (weaponStats.attackStrike * multiplier) - charStats.vs_strike;
            float slash = (weaponStats.attackSlash * multiplier) - charStats.vs_slash;
            float thrust = (weaponStats.attackThrust * multiplier) - charStats.vs_thrust;

            //Total base damage
            float totalDamage = physical + strike + slash + thrust;
            
            float magic = (weaponStats.attackMagic * multiplier) - charStats.magic;
            float fire = (weaponStats.attackFire * multiplier) - charStats.fire;
            float lighting = (weaponStats.attackLigthning * multiplier) - charStats.lightning;
            float dark = (weaponStats.attackDark * multiplier) - charStats.dark;

            //Add the extra attributes of weapon
            totalDamage += magic + fire + lighting + dark;

            if (totalDamage <= 0)
            {
                totalDamage = 1;
            }

            return Mathf.RoundToInt(totalDamage);
        }
    }
}