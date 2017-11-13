using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public static class StatsCalculations
    {
        //Calculates damage for base weapon (which has no additional abilities like poison, magic etc.)
        public static int CalculateBaseDamage(WeaponStats weaponStats, CharacterStats charStats) {

            //Extract the resistance from weapon's standalone damage
            int physical = weaponStats.physicalDamage - charStats.physical;
            int strike = weaponStats.strikeDamage - charStats.vs_strike;
            int slash = weaponStats.slashDamage - charStats.vs_slash;
            int thrust = weaponStats.thrustDamage - charStats.vs_thrust;

            //Total base damage
            int totalDamage = physical + strike + slash + thrust;

            int magic = weaponStats.magicDamage - charStats.magic;
            int fire = weaponStats.fireDamage - charStats.fire;
            int lighting = weaponStats.lightningDamage- charStats.lightning;
            int dark = weaponStats.darkDamage - charStats.dark;

            //Add the extra attributes of weapon
            totalDamage += magic + fire + lighting + dark;

            if (totalDamage <= 0)
            {
                totalDamage = 1;
            }

            return totalDamage;
        }
    }
}