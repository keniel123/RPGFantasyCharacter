using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class InventoryManager : MonoBehaviour
    {
        public Weapon currentWeapon;

        public void Init()
        {

        }

    }

    [System.Serializable]
    public class Weapon
    {
        public List<Action> actions;
        public List<Action> twoHandedActions;
        public GameObject weaponModel;
    }
}