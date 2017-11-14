using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class WeaponScriptableObject : ScriptableObject
    {
        public List<Weapon> weaponsAll = new List<Weapon>();
    }
}