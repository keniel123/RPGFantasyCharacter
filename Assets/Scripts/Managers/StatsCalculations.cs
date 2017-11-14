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
            float physical = (weaponStats.physicalDamage * multiplier) - charStats.physical;
            float strike = (weaponStats.strikeDamage * multiplier) - charStats.vs_strike;
            float slash = (weaponStats.slashDamage * multiplier) - charStats.vs_slash;
            float thrust = (weaponStats.thrustDamage * multiplier) - charStats.vs_thrust;

            //Total base damage
            float totalDamage = physical + strike + slash + thrust;
            
            float magic = (weaponStats.magicDamage * multiplier) - charStats.magic;
            float fire = (weaponStats.fireDamage * multiplier) - charStats.fire;
            float lighting = (weaponStats.lightningDamage * multiplier) - charStats.lightning;
            float dark = (weaponStats.darkDamage * multiplier) - charStats.dark;

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