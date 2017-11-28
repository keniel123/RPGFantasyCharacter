using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class RuntimeWeapon : MonoBehaviour
    {
        public bool isUnarmed;
        public Weapon Instance;
        public GameObject weaponModel;
        public WeaponHook weaponHook;
        public WeaponStats weaponStats;

    }
}